using UnityEngine;

namespace Framework.SCamera
{
    public class CameraController : MonoBehaviour
    {
        static public CameraController  Instance
        {
            get;
            private set;
        }

        public Camera main
        {
            get
            {
                return _cameras[0];
            }
        }

        private Camera[]                _cameras;                                           // [0]: scene camera; [1]: SceneFx camera
        //private CinemachineBrain        _brain;

        [Space(10)]
        private Vector2                 _dragDelta;                                         // .x表示Yaw；.y表示Pitch
        private float                   _pinchDelta;                                        // 
        
        public float                    dragSensitivityOnMobile         = 3.0f;             // 移动平台上对拖拽时灵敏度的加乘(multiply)
        public float                    dragSensitivityOnPC             = 0.2f;             // PC平台上对拖拽时灵敏度的加乘(multiply)

        public float                    pinchSensitivityOnMobile        = 1.0f;             // 移动平台上pinch时灵敏度的加乘(multiply)       
        public float                    pinchSensitivityOnPC            = 0.02f;            // PC平台上pinch时灵敏度的加乘(multiply)       

        public float                    smoothTimeToCloseup             = 0.2f;             // 特写镜头过渡参数
        public float                    smoothTime                      = 0.2f;             // drag，pinch操作的过渡参数
        public float                    smoothTimeDistance              = 0.2f;
        public LayerMask                collisionMask;                                      // 镜头碰撞Mask
        public float                    sphereCastRadius                = 0.3f;
        
        public enum CloseupShotState
        {
            Free,
            Closeup,
            ExtremeCloseup,
        }
        public CloseupShotState         shotState { get; private set; }

        private class FViewTarget
        {
            public Transform        viewTarget;
            public CameraViewInfo   viewInfo;

            public FViewTarget(Transform InViewTarget, CameraViewInfo InViewInfo)
            {
                viewTarget = InViewTarget;

                if( InViewInfo == null && viewInfo == null )
                { // 创建默认数据
                    viewInfo = ScriptableObject.CreateInstance<CameraViewInfo>();
                    Debug.LogWarning("FViewTarget:: InViewInfo == null");
                    return;
                }

                if( InViewInfo != null )
                {
                    viewInfo = CameraViewInfo.CopyFrom(InViewInfo);
                }                
            }
        }
        private FViewTarget             _curVT;
        private FViewTarget             _pendingVT;

        private bool                    _bTransition;
        private float                   _smoothTimeToTarget;

        private Vector3                 _rigVelocity;
        private float                   _fovVelocity;
        private float                   _disVelocity;
        private float                   _pitchVelocity;
        private float                   _yawVelocity;

        private Ray                     _ray                                = new Ray();
        private RaycastHit[]            _hits                               = new RaycastHit[16];

        private CameraEffectProfile     _effectProfile;                     // 震屏效果（技能、轻功时使用）
        private CameraEffectProfile     _moveFastEffectProfile;             // 脱战疾跑时的镜头效果

        private CameraViewInfoProfile   _viewInfoProfile;                   // 主角的镜头组数据
        private Transform               _viewTarget;                        // 主角

        private FViewTarget             _cachedViewTarget;                  // 进入其他镜头前保存当前的镜头参数
        private float                   _cachedFOV;
        private float                   _cachedPitch;
        private float                   _cachedYaw;
        private float                   _cachedDistance;

        private float                   _cachedShadowDistance;

        private bool                    _bWasUseFixedPitch;
        public bool                     bUseFixedPitch      { get; set; }

        //public CinemachineBrain cinemachineBrain
        //{
        //    get
        //    {
        //        if(_brain == null)
        //        {
        //            _brain = transform.GetComponentInChildren<CinemachineBrain>();
        //        }
        //        return _brain;
        //    }
        //}

        private void Awake()
        {
            Instance = this;
            camTransform = GetComponentInChildren<Camera>().transform;         // 顺序查找到第一个含Camera组件的节点
            //vcTransform = GetComponentInChildren<CinemachineVirtualCamera>();
            _cameras = GetComponentsInChildren<Camera>();

            _cachedShadowDistance = QualitySettings.shadowDistance;
        }

        public void AddCullingMask(int mask)
        {
            if( _cameras != null && _cameras.Length > 0 )
            {
                _cameras[0].cullingMask |= mask;
            }
        }

        public void RemoveCullingMask(int mask)
        {
            if (_cameras != null && _cameras.Length > 0)
            {
                _cameras[0].cullingMask &= ~mask;
            }
        }

        public void SetClearFlag(CameraClearFlags flag)
        {
            if (_cameras != null && _cameras.Length > 0)
            {
                _cameras[0].clearFlags = flag;
            }
        }

        /// <summary>
        /// 切换目标位，底层接口，不涉及状态
        /// </summary>
        /// <param name="viewTarget"></param>
        /// <param name="viewInfoProfile"></param>
        /// <param name="bUseDefault">初始视角（fov，pitch，yaw，distance）是否使用默认参数</param>
        /// <param name="smoothTime"></param>
        private void SetViewTarget(Transform viewTarget, CameraViewInfo viewInfo, bool bUseDefault = true, float smoothTime = 0.2f)
        {
            if( viewInfo == null )
            {
                Debug.LogError("SetViewTarget: viewInfo == null");
                return;
            }

            if(_curVT == null)
            {
                _curVT      = new FViewTarget(viewTarget,   viewInfo);
                _pendingVT  = new FViewTarget(viewTarget,   viewInfo);
            }
            else
            {
                _curVT      = new FViewTarget(viewTarget,   _curVT.viewInfo);         // 当前FViewTarget仅改变viewTarget，其他参数保持不变
                _pendingVT  = new FViewTarget(viewTarget,   viewInfo);
            }

            // 初始化
            if (bUseDefault)
            {
                _pendingVT.viewInfo.fov         = _pendingVT.viewInfo.defaultFOV;
                _pendingVT.viewInfo.pitch       = _pendingVT.viewInfo.defaultPitch;
                _pendingVT.viewInfo.yaw         = NormalizeAngle((viewTarget != null ? viewTarget.transform.rotation.eulerAngles.y : 0) + _pendingVT.viewInfo.defaultYaw);
                _pendingVT.viewInfo.distance    = _pendingVT.viewInfo.defaultDistance;
            }

            // 切换目标时重置
            _rigVelocity = Vector3.zero;
            _fovVelocity = 0;
            _pitchVelocity = 0;
            _yawVelocity = 0;
            _disVelocity = 0;

            _bTransition = true;
            _smoothTimeToTarget = smoothTime;

            if (_smoothTimeToTarget < 0.0001f)
            {
                UpdateTransition();
            }
        }

        private void ProcessInput()
        {
            //_dragDelta = KInput.instance.DragDelta * GetPlatformDragSensitivity();
            _dragDelta.y *= -1;

            //_pinchDelta = KInput.instance.PinchDelta * GetPlatformPinchSensitivity() * -1;
        }

        private void UpdateTransition()
        {
            if (_pendingVT.viewTarget != null)
                _pendingVT.viewInfo.rig = _pendingVT.viewTarget.position + _pendingVT.viewTarget.TransformVector(_pendingVT.viewInfo.rigOffset);
            
            if ( _bTransition )
            {
                if( _smoothTimeToTarget <= 0.0001f /*|| _curVT.viewInfo.SmoothDamp(_curVT.viewInfo, _pendingVT.viewInfo, _smoothTimeToTarget)*/ )
                {
                    _bTransition = false;
                    _curVT = new FViewTarget(_pendingVT.viewTarget, _pendingVT.viewInfo);
                }

                if(_bTransition)
                {
                    if (bUseFixedPitch)                     // 3D
                        _pendingVT.viewInfo.pitch = _pendingVT.viewInfo.fixedPitch;
                    else if (_bWasUseFixedPitch)            // 2.5D -> 3D
                        _pendingVT.viewInfo.pitch = _pendingVT.viewInfo.defaultPitch;

                    if(_curVT.viewInfo.SmoothDamp(_curVT.viewInfo, _pendingVT.viewInfo, _smoothTimeToTarget))
                    {
                        _bTransition = false;
                        _curVT = new FViewTarget(_pendingVT.viewTarget, _pendingVT.viewInfo);
                    }
                }
            }
            else
            {
                _curVT.viewInfo.rig = Vector3.SmoothDamp(_curVT.viewInfo.rig, _pendingVT.viewInfo.rig, ref _rigVelocity, 0);
                
                // fov
                _curVT.viewInfo.fov = Mathf.SmoothDamp(_curVT.viewInfo.fov, _pendingVT.viewInfo.fov, ref _fovVelocity, smoothTime);

                // distance
                _pendingVT.viewInfo.distance = Mathf.Clamp(_pendingVT.viewInfo.distance + _pinchDelta, _pendingVT.viewInfo.minDistance, _pendingVT.viewInfo.maxDistance);
                _curVT.viewInfo.distance = Mathf.SmoothDamp(_curVT.viewInfo.distance, _pendingVT.viewInfo.distance, ref _disVelocity, smoothTimeDistance);
                
                // pitch
                _pendingVT.viewInfo.pitch = NormalizeAngle(Mathf.Clamp(_pendingVT.viewInfo.pitch + _dragDelta.y, _pendingVT.viewInfo.minPitch, _pendingVT.viewInfo.maxPitch));
                if (bUseFixedPitch)                     // 3D
                    _pendingVT.viewInfo.pitch = _pendingVT.viewInfo.fixedPitch;
                else if (_bWasUseFixedPitch)            // 2.5D -> 3D
                    _pendingVT.viewInfo.pitch = _pendingVT.viewInfo.defaultPitch;
                _curVT.viewInfo.pitch = NormalizeAngle(Mathf.SmoothDampAngle(_curVT.viewInfo.pitch, _pendingVT.viewInfo.pitch, ref _pitchVelocity, smoothTime));

                // yaw
                _pendingVT.viewInfo.yaw = NormalizeAngle(_pendingVT.viewInfo.yaw + _dragDelta.x);
                _curVT.viewInfo.yaw = NormalizeAngle(Mathf.SmoothDampAngle(_curVT.viewInfo.yaw, _pendingVT.viewInfo.yaw, ref _yawVelocity, smoothTime));
            }

            _bWasUseFixedPitch = bUseFixedPitch;
        }

        private void UpdateCamera()
        {
            transform.position = _curVT.viewInfo.rig;
            transform.rotation = Quaternion.Euler(_curVT.viewInfo.pitch, _curVT.viewInfo.yaw, 0);

            CheckCollision();

            //vcTransform.transform.localPosition = Vector3.forward * _curVT.viewInfo.distance * -1;
            //vcTransform.m_Lens.FieldOfView = _curVT.viewInfo.fov;
        }

        private void CheckCollision()
        {
            _ray.origin = transform.position;
            _ray.direction = -transform.forward;

            int count = Physics.SphereCastNonAlloc(_ray, sphereCastRadius, _hits, _curVT.viewInfo.distance, collisionMask, QueryTriggerInteraction.Ignore);

            float nearest = Mathf.Infinity;
            int index = -1;
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].distance < nearest)
                {
                    nearest = _hits[i].distance;
                    index = i;
                }
            }

            if( index != -1 )
            {
                _curVT.viewInfo.distance = _hits[index].distance;
            }
        }

        private void UpdateCameraEffect()
        {
            if( _effectProfile != null && _effectProfile.IsPlaying() )
            {
                _effectProfile.UpdateCameraEffect(_cameras[0]);

                if( _effectProfile.shakePosition.active )
                {
                    //vcTransform.transform.localPosition += _effectProfile.shakePosition.FinalPosition;
                }

                if( _effectProfile.shakeRotation.active )
                {
                    //vcTransform.transform.localRotation = _effectProfile.shakeRotation.FinalRotation;
                }

                if( _effectProfile.shakeFOV.active )
                {
                    //vcTransform.m_Lens.FieldOfView = _effectProfile.shakeFOV.FinalScaleOfFOV * _curVT.viewInfo.fov;
                }
            }
        }

        // 同步场景相机参数到特效相机
        private void SyncSceneToFxCamera()
        {
            if(_cameras[0] != null && _cameras[1] != null)
            {
                _cameras[1].fieldOfView = _cameras[0].fieldOfView;
                _cameras[1].nearClipPlane = _cameras[0].nearClipPlane;
                _cameras[1].farClipPlane = _cameras[0].farClipPlane;
            }
        }

        private void UpdateMoveFastCameraEffect()
        {
            //if (_moveFastEffectProfile != null && _moveFastEffectProfile.IsPlaying())
            //{
            //    _moveFastEffectProfile.UpdateCameraEffect(_cameras[0]);

            //    if (_moveFastEffectProfile.shakePosition.active)
            //    {
            //        if (ActorAuthority.instance != null)
            //        {
            //            Vector3 worldPos = ActorAuthority.instance.transform.TransformVector(_moveFastEffectProfile.shakePosition.FinalPosition);
            //            vcTransform.transform.localPosition += transform.InverseTransformVector(worldPos);
            //        }
            //        else
            //        {
            //            vcTransform.transform.localPosition += _moveFastEffectProfile.shakePosition.FinalPosition;
            //        }
            //    }

            //    if (_moveFastEffectProfile.shakeRotation.active)
            //    {
            //        vcTransform.transform.localRotation = _moveFastEffectProfile.shakeRotation.FinalRotation;
            //    }

            //    if (_moveFastEffectProfile.shakeFOV.active)
            //    {
            //        vcTransform.m_Lens.FieldOfView = _moveFastEffectProfile.shakeFOV.FinalScaleOfFOV * _curVT.viewInfo.fov;
            //    }
            //}
        }


        /// <summary>
        /// camera update pipeline
        /// </summary>
        void LateUpdate()
        {
            if (_curVT == null)
                return;

            ProcessInput();

            UpdateTransition();

            UpdateCamera();

            UpdateCameraEffect();

            UpdateMoveFastCameraEffect();
            
            SyncSceneToFxCamera();
        }

        private float GetPlatformDragSensitivity()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return dragSensitivityOnPC;
#else
            return dragSensitivityOnMobile;
#endif
        }

        private float GetPlatformPinchSensitivity()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return pinchSensitivityOnPC;
#else
            return pinchSensitivityOnMobile;
#endif
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            if( _cameras.Length > 0 )
            {
                return _cameras[0].WorldToViewportPoint(position);
            }
            return position;
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            if(_cameras.Length > 0)
            {
                return _cameras[0].WorldToScreenPoint(position);
            }
            return position;
        }

        public Ray ScreenPointToRay(Vector2 position)
        {
            if(_cameras.Length > 0)
            {
                Camera camera = _cameras[0];
                Vector2 view = new Vector2(position.x / Screen.width, position.y / Screen.height);
                return camera.ViewportPointToRay(view);//正龙那边会改rt大小，不能直接用ScreenPointToRay
            }
            return new Ray();
        }

        public Vector3 position
        {
            get
            {
                return camTransform != null ? camTransform.position : transform.position;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return camTransform != null ? camTransform.rotation : transform.rotation;
            }
        }

        public Transform camTransform
        {
            get; private set;
        }

        //public CinemachineVirtualCamera vcTransform
        //{
        //    get; private set;
        //}

        static public float NormalizeAngle(float angle)
        {
            angle = angle % 360;

            if (angle < -180)
                return angle + 360;
            else if (angle > 180)
                return angle - 360;
            else
                return angle;
        }
        
        /// <summary>
        /// 从当前viewInfo切换至free viewinfo，可选择是否保留当前视角的pitch，yaw
        /// </summary>
        /// <param name="bKeepPitch">保持pitch不变</param>
        /// <param name="bKeepYaw">保持yaw不变</param>
        private void ReturnToFreeView(bool bKeepPitch, bool bKeepYaw, float smoothTime = 0.2f)
        {
            // 仅pitch,yaw保持closeupView,其他数据还原freeView
            CameraViewInfo newFreeViewInfo = CameraViewInfo.CopyFrom(_viewInfoProfile.freeView);

            if (bKeepPitch)
                newFreeViewInfo.pitch = _curVT.viewInfo.pitch;
            if (bKeepYaw)
                newFreeViewInfo.yaw = _curVT.viewInfo.yaw;
            newFreeViewInfo.fov = newFreeViewInfo.defaultFOV;
            newFreeViewInfo.distance = newFreeViewInfo.defaultDistance;
            SetViewTarget(_viewTarget, newFreeViewInfo, false, smoothTime);
        }

        private void SaveCurrentViewTarget()
        {
            if( _cachedViewTarget == null )
            {
                _cachedViewTarget = new FViewTarget(_curVT.viewTarget, _curVT.viewInfo);
                _cachedFOV = _curVT.viewInfo.fov;
                _cachedPitch = _curVT.viewInfo.pitch;
                _cachedYaw = _curVT.viewInfo.yaw;
                _cachedDistance = _curVT.viewInfo.distance;
                //_cachedViewTarget.viewInfo.defaultFOV = _curVT.viewInfo.fov;
                //_cachedViewTarget.viewInfo.defaultPitch = _curVT.viewInfo.pitch;
                //_cachedViewTarget.viewInfo.defaultYaw = NormalizeAngle(_curVT.viewInfo.yaw - (_curVT.viewTarget != null ? _curVT.viewTarget.transform.rotation.eulerAngles.y : 0));
                //_cachedViewTarget.viewInfo.defaultDistance = _curVT.viewInfo.distance;
            }
        }

        private void LoadPreviousViewTarget(float time = 0)
        {
            if(_cachedViewTarget != null )
            {
                _cachedViewTarget.viewInfo.fov = _cachedFOV;
                _cachedViewTarget.viewInfo.pitch = _cachedPitch;
                _cachedViewTarget.viewInfo.yaw = _cachedYaw;
                _cachedViewTarget.viewInfo.distance = _cachedDistance;
                SetViewTarget(_cachedViewTarget.viewTarget, _cachedViewTarget.viewInfo, false, time);

                _cachedViewTarget = null;
            }
        }

        ///////////////////////////////////////////// 镜头切换、震屏等接口
        /// <summary>
        /// 朝向主角
        /// </summary>
        /// <param name="viewTarget"></param>
        /// <param name="viewInfoProfile"></param>
        /// <param name="smoothTime"></param>
        public void LookPlayer(Transform viewTarget, CameraViewInfoProfile viewInfoProfile, float smoothTime)
        {
            if (viewInfoProfile == null)
            {
                Debug.LogError("SetViewTarget: viewInfoProfile == null");
                return;
            }

            // 记录主角相关数据
            _viewInfoProfile = CameraViewInfoProfile.CopyFrom(viewInfoProfile);
            _viewTarget = viewTarget;

            SetViewTarget(_viewTarget, _viewInfoProfile.freeView, true, smoothTime);

            shotState = CloseupShotState.Free;
        }

        /// <summary>
        /// 切换至自由视角
        /// </summary>
        /// <param name="bForce"></param>
        public void LookPlayerFree(bool bForce = false)
        {
            //if (shotState != CloseupShotState.Free || bForce)
            //    EnterFreeView();

            shotState = CloseupShotState.Free;
        }

        /// <summary>
        /// 看向玩家手中八卦盘
        /// </summary>
        public void LookBaguapan()
        {
            // 保存当前镜头参数
            SaveCurrentViewTarget();
            SetViewTarget(_viewTarget, _viewInfoProfile.openGSUIView, true, 0.35f);
        }
        public void LookBaguapanFree()
        {
            LoadPreviousViewTarget();
        }
        
        /// <summary>
        /// NPC对话时看NPC的镜头接口
        /// </summary>
        /// <param name="viewTarget"></param>
        /// <param name="viewInfo"></param>
        /// <param name="smoothTime"></param>
        public void DialogViewNPC(Transform viewTarget, CameraViewInfo viewInfo, float smoothTime)
        {
            if( viewInfo == null )
            {
                Debug.LogError("LookNPC: viewInfo == null");
                return;
            }

            // 保存当前镜头参数
            SaveCurrentViewTarget();

            //if(viewTarget == ActorAuthority.instance && viewInfo == null)
            { // 使用角色内置的对话视角

            }

            viewInfo.fixedPitch = viewInfo.defaultPitch;
            SetViewTarget(viewTarget, viewInfo, true, smoothTime);
        }

        /// <summary>
        /// NPC对话时看本地角色的镜头接口
        /// </summary>
        /// <param name="type">0: 正面看；1：侧面看</param>
        /// <param name="overrideViewInfo"></param>
        /// <param name="smoothTime"></param>
        public void DialogViewPlayer(int type, CameraViewInfo overrideViewInfo, float smoothTime)
        {
            //if(overrideViewInfo != null)
            //{
            //    overrideViewInfo.fixedPitch = overrideViewInfo.defaultPitch;
            //    SetViewTarget(ActorAuthority.instance.transform, overrideViewInfo, true, smoothTime);
            //}
            //else
            //{
            //    SetViewTarget(ActorAuthority.instance.transform, type == 1 ? _viewInfoProfile.dialogSideView : _viewInfoProfile.dialogPositiveView, true, smoothTime);
            //}
        }

        /// <summary>
        /// 离开NPC特写镜头
        /// </summary>
        public void LookNPCFree()
        {
            // 还原之前镜头
            LoadPreviousViewTarget();
        }

        //进入查看邮件
        public void LookMail()
        {
            // 保存当前镜头参数
            SaveCurrentViewTarget();

            SetViewTarget(_viewTarget, _viewInfoProfile.mailView, true, 0.2f);
        }
        //离开查看邮件
        public void LookMailFree()
        {
            // 还原之前镜头
            LoadPreviousViewTarget(0.2f);
        }

        /// <summary>
        /// 播放震屏接口
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="onFinished"></param>
        public void PlayCameraEffect(CameraEffectProfile profile, System.Action onFinished = null)
        {
            if (profile == null)
            {
                Debug.LogWarning("PlayCameraEffect: CameraEffectProfile == none");
                return;
            }

            if (_effectProfile != null && _effectProfile.IsPlaying() && _effectProfile.priority > profile.priority)
                return;

            StopCameraEffect();

            _effectProfile = profile;
            _effectProfile.Play(true, onFinished);
        }

        public void StopCameraEffect()
        {
            if (_effectProfile != null && _effectProfile.IsPlaying())
            {
                _effectProfile.Stop(0);

                UpdateCameraEffect();
            }
        }

        public void PlayMoveFastCameraEffect(CameraEffectProfile profile, System.Action onFinished = null)
        {
            if (profile == null)
            {
                Debug.LogWarning("PlayMoveFastCameraEffect: CameraEffectProfile == none");
                return;
            }

            if (_moveFastEffectProfile != null && _moveFastEffectProfile.IsPlaying())
                return;
            
            _moveFastEffectProfile = profile;
            _moveFastEffectProfile.Play(false, onFinished);
        }

        public void StopMoveFastCameraEffect(float fadeOutTime = 0)
        {
            if (_moveFastEffectProfile != null && _moveFastEffectProfile.IsPlaying())
            {
                _moveFastEffectProfile.Stop(fadeOutTime);

                UpdateMoveFastCameraEffect();
            }
        }

#if UNITY_EDITOR
        public void EditorSetViewTarget(Transform viewTarget, CameraViewInfo viewInfo, float smoothTime)
        {
            SetViewTarget(viewTarget, viewInfo, true, smoothTime);
        }

        public void GetViewInfo(out Transform viewTarget, out CameraViewInfo viewInfo)
        {
            viewTarget = (_pendingVT != null && _pendingVT.viewTarget != null) ? _pendingVT.viewTarget : null;
            viewInfo = (_pendingVT != null && _pendingVT.viewInfo != null) ? _pendingVT.viewInfo : null;
        }

        public void GetEffectProfile(out CameraEffectProfile profile)
        {
            profile = _effectProfile;
        }

        public void GetViewInfoProfile(out CameraViewInfoProfile viewInfoProfile)
        {
            viewInfoProfile = _viewInfoProfile;
        }

        public void SaveToDefault(Transform viewTarget, CameraViewInfo viewInfo)
        {
            if (viewInfo == null)
            {
                Debug.LogError("CameraController.Save: viewInfo == null");
                return;
            }

            viewInfo.defaultFOV         = viewInfo.fov;
            viewInfo.defaultYaw         = NormalizeAngle(viewInfo.yaw - (viewTarget != null ? viewTarget.transform.rotation.eulerAngles.y : 0));
            viewInfo.defaultPitch       = viewInfo.pitch;
            viewInfo.defaultDistance    = viewInfo.distance;
        }

        public string assetPath { get; set; }

        public string effectProfileAssetPath { get; set; }

        public string viewInfoProfileAssetPath { get; set; }
#endif
    }
}