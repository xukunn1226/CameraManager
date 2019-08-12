using UnityEngine;

namespace Framework
{
    public class CameraManager : MonoBehaviour
    {
        public delegate void EventHandler(bool isWatching);
        public event EventHandler           OnPostUpdate;                                               // Update之后，LateUpdate之前的回调，可以获取到相机的最新朝向

        static public CameraManager         instance
        {
            get;
            private set;
        }

        public Camera                       main                                { get; private set; }

        private Vector2                     m_DragDelta;                                                // .x表示Yaw；.y表示Pitch
        private float                       m_PinchDelta;                                               // 
        
        public float                        m_DragSensitivityOnMobile           = 3.0f;                 // 移动平台上对拖拽时灵敏度的加乘(multiply)
        public float                        m_DragSensitivityOnPC               = 0.2f;                 // PC平台上对拖拽时灵敏度的加乘(multiply)

        private float                       m_PinchSensitivityOnMobile          = 1.0f;                 // 【暂时屏蔽】移动平台上pinch时灵敏度的加乘(multiply)       
        private float                       m_PinchSensitivityOnPC              = 0.02f;                // 【暂时屏蔽】PC平台上pinch时灵敏度的加乘(multiply)       

        public float                        m_SmoothTime                        = 0.15f;                // 平滑参数，持续影响fov、distance
        public LayerMask                    m_CollisionMask;                                            // 镜头碰撞Mask
        public float                        m_SphereCastRadius                  = 0.3f;
        
        private class FViewTarget
        {
            public Transform                m_ViewTarget;
            public CameraViewInfo           m_ViewInfo;

            public FViewTarget()
            {
                m_ViewInfo = ScriptableObject.CreateInstance<CameraViewInfo>();
            }

            public void InitViewTarget(Transform InViewTarget, CameraViewInfo InViewInfo)
            {
                m_ViewTarget = InViewTarget;

                m_ViewInfo.Init(InViewTarget, InViewInfo);
            }
        }
        private FViewTarget                 m_CurVT;
        private FViewTarget                 m_PendingVT;

        private Vector3                     m_RigVelocity;
        private float                       m_FovVelocity;
        private float                       m_DisVelocity;
        //private float                       m_PitchVelocity;
        private float                       m_YawVelocity;

        private Ray                         m_Ray                               = new Ray();
        private RaycastHit[]                m_Hits                              = new RaycastHit[16];

        private CameraViewInfoCollection    m_ViewInfoCollection;                                       // 当前的镜头组数据
        private Transform                   m_ViewTarget;                                               // 跟随对象
        private CameraEffectInfo            m_EffectInfo;                                               // 震屏效果

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            main = GetComponentInChildren<Camera>();
            m_CurVT = new FViewTarget();
            m_PendingVT = new FViewTarget();
        }

        private void OnDestroy()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                instance = null;
            }
        }

        /// <summary>
        /// 底层接口，初始化相机视角参数
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfo"></param>
        private void InitViewInfo(Transform InViewTarget, CameraViewInfo InViewInfo)
        {
            m_CurVT.InitViewTarget(InViewTarget, InViewInfo);

            SetPendingViewInfo(InViewTarget, InViewInfo, 0);
        }

        /// <summary>
        /// 底层接口，设置下一个相机视角参数
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfo"></param>
        /// <param name="smoothTime"></param>
        private void SetPendingViewInfo(Transform InViewTarget, CameraViewInfo InViewInfo, float smoothTime = 0.15f)
        {
            m_PendingVT.InitViewTarget(InViewTarget, InViewInfo);

            m_RigVelocity = Vector3.zero;
            m_FovVelocity = 0;
            //m_PitchVelocity = 0;
            m_YawVelocity = 0;
            m_DisVelocity = 0;
            m_SmoothTime = smoothTime;
        }

        /// <summary>
        /// 上层接口，初始化角色默认视角
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfoCollection"></param>
        public void InitViewInfoCollection(Transform InViewTarget, CameraViewInfoCollection InViewInfoCollection, CameraViewInfoCollection.CharacterView defaultView = CameraViewInfoCollection.CharacterView.Run)
        {
            if (InViewTarget == null || InViewInfoCollection == null)
            {
                Debug.LogError("Camera Init: viewTarget == null || viewInfoProfile == null");
                return;
            }

            m_ViewInfoCollection = InViewInfoCollection;
            m_ViewTarget = InViewTarget;

            InitViewInfo(m_ViewTarget, m_ViewInfoCollection != null ? m_ViewInfoCollection[defaultView] : null);
        }
        
        /// <summary>
        /// 常规相机位的切换，根据需求pitch、yaw、distance维持不变
        /// </summary>
        /// <param name="charView"></param>
        /// <param name="isAiming"></param>
        /// <param name="smoothTime"></param>
        public void SetCharacterView(CameraViewInfoCollection.CharacterView charView, bool isAiming = false, float smoothTime = 0.15f)
        {
            SetPendingViewInfo(m_ViewTarget, m_ViewInfoCollection != null ? m_ViewInfoCollection[charView] : null, smoothTime);

            // procedural view data, inherit current pitch，yaw，distance
            m_PendingVT.m_ViewInfo.pitch = m_CurVT.m_ViewInfo.pitch;
            m_PendingVT.m_ViewInfo.yaw = m_isWatching ? m_PendingVT.m_ViewInfo.yaw : m_CurVT.m_ViewInfo.yaw;        // 观察模式下切换charView维持yaw不变 —— corner case
            m_PendingVT.m_ViewInfo.distance = m_CurVT.m_ViewInfo.distance;

            if(isAiming)
            {
                m_PendingVT.m_ViewInfo.rigOffset = m_PendingVT.m_ViewInfo.rigOffsetWhenAim;
            }
        }
        
        private void Update()
        {
            if (m_CurVT.m_ViewTarget == null)
                return;

            if(!m_isWatching)
            {
                ProcessInput();
            }

            UpdateViewInfoExceptRig();

            OnPostUpdate?.Invoke(m_isWatching);
        }

        void LateUpdate()
        {
            if (m_CurVT.m_ViewTarget == null)
                return;

            UpdateRigViewInfo();

            UpdateCamera();

            UpdateCameraEffect();
        }

        private void ProcessInput()
        {
            m_DragDelta = (InputManager.instance != null ? InputManager.instance.DragDelta : Vector2.zero) * GetPlatformDragSensitivity();
            m_DragDelta.y *= -1;

            m_PinchDelta = (InputManager.instance != null ? InputManager.instance.PinchDelta : 0) * GetPlatformPinchSensitivity();
        }

        // 更新除rig之外的其他相机属性
        private void UpdateViewInfoExceptRig()
        {
            // Update m_PendingVT
            {
                // distance
                m_PendingVT.m_ViewInfo.distance = Mathf.Clamp(m_PendingVT.m_ViewInfo.distance + m_PinchDelta, m_PendingVT.m_ViewInfo.minDistance, m_PendingVT.m_ViewInfo.maxDistance);

                // pitch
                m_PendingVT.m_ViewInfo.pitch = NormalizeAngle(Mathf.Clamp(m_PendingVT.m_ViewInfo.pitch + m_DragDelta.y, m_PendingVT.m_ViewInfo.minPitch, m_PendingVT.m_ViewInfo.maxPitch));

                // yaw
                m_PendingVT.m_ViewInfo.yaw = NormalizeAngle(m_PendingVT.m_ViewInfo.yaw + m_DragDelta.x);
            }

            // Update m_CurVT
            {
                // fov
                m_CurVT.m_ViewInfo.fov = Mathf.SmoothDamp(m_CurVT.m_ViewInfo.fov, m_PendingVT.m_ViewInfo.fov, ref m_FovVelocity, m_SmoothTime);

                // distance
                m_CurVT.m_ViewInfo.distance = Mathf.SmoothDamp(m_CurVT.m_ViewInfo.distance, m_PendingVT.m_ViewInfo.distance, ref m_DisVelocity, m_SmoothTime);

                // pitch
                //m_CurVT.m_ViewInfo.pitch = NormalizeAngle(Mathf.SmoothDampAngle(m_CurVT.m_ViewInfo.pitch, m_PendingVT.m_ViewInfo.pitch, ref m_PitchVelocity, m_SmoothTime));
                m_CurVT.m_ViewInfo.pitch = NormalizeAngle(m_PendingVT.m_ViewInfo.pitch);        // 优化，FPS不考虑缓动效果

                // yaw
                if(m_isWatching && m_isRecovering)
                {
                    m_CurVT.m_ViewInfo.yaw = NormalizeAngle(Mathf.SmoothDampAngle(m_CurVT.m_ViewInfo.yaw, m_PendingVT.m_ViewInfo.yaw, ref m_YawVelocity, m_SmoothTime));
                    //Debug.LogFormat("{0}    {1}     {2}     {3}", Time.frameCount, Time.time, m_YawVelocity, (m_PendingVT.m_ViewInfo.yaw - m_CurVT.m_ViewInfo.yaw));
                    //if (Mathf.Abs(m_YawVelocity) < 0.2f)
                    if( Mathf.Abs(m_PendingVT.m_ViewInfo.yaw - m_CurVT.m_ViewInfo.yaw) < 0.5f)
                    {
                        //Debug.LogWarningFormat("{0}    {1}     {2}      {3}", Time.frameCount, Time.time, m_YawVelocity, (m_PendingVT.m_ViewInfo.yaw - m_CurVT.m_ViewInfo.yaw));
                        m_isWatching = false;
                    }
                }
                else
                    m_CurVT.m_ViewInfo.yaw = NormalizeAngle(m_PendingVT.m_ViewInfo.yaw);            // 优化，FPS不考虑缓动效果
            }
        }

        // FPS游戏是先转动角色，在根据角色朝向确定rig，故rig延后更新
        private void UpdateRigViewInfo()
        {
            m_CurVT.m_ViewInfo.rigOffset = Vector3.SmoothDamp(m_CurVT.m_ViewInfo.rigOffset, m_PendingVT.m_ViewInfo.rigOffset, ref m_RigVelocity, m_SmoothTime);

            if (m_CurVT.m_ViewTarget != null)
            {
                m_CurVT.m_ViewInfo.rig = m_CurVT.m_ViewTarget.position + m_CurVT.m_ViewTarget.TransformVector(m_CurVT.m_ViewInfo.rigOffset);
            }
        }

        private void UpdateCamera()
        {
            transform.position = m_CurVT.m_ViewInfo.rig;
            transform.rotation = Quaternion.Euler(m_CurVT.m_ViewInfo.pitch, m_CurVT.m_ViewInfo.yaw, 0);

            main.transform.localPosition = Vector3.forward * CheckCollision() * -1;
            main.fieldOfView = m_CurVT.m_ViewInfo.fov;
        }

        private float CheckCollision()
        {
            m_Ray.origin = transform.position;
            m_Ray.direction = -transform.forward;

            int count = Physics.SphereCastNonAlloc(m_Ray, m_SphereCastRadius, m_Hits, m_CurVT.m_ViewInfo.distance, m_CollisionMask, QueryTriggerInteraction.Ignore);

            float nearest = Mathf.Infinity;
            int index = -1;
            for (int i = 0; i < count; i++)
            {
                if (m_Hits[i].distance < nearest)
                {
                    nearest = m_Hits[i].distance;
                    index = i;
                }
            }

            return index != -1 ? m_Hits[index].distance : m_CurVT.m_ViewInfo.distance;
        }

        private void UpdateCameraEffect()
        {
            if( m_EffectInfo != null && m_EffectInfo.IsPlaying() )
            {
                m_EffectInfo.UpdateCameraEffect(main);

                if( m_EffectInfo.m_ShakePosition.m_Active )
                {
                    main.transform.localPosition += m_EffectInfo.m_ShakePosition.m_FinalPosition;
                }

                if( m_EffectInfo.m_ShakeRotation.m_Active )
                {
                    main.transform.localRotation = m_EffectInfo.m_ShakeRotation.m_FinalRotation;
                }

                if( m_EffectInfo.m_ShakeFOV.m_Active )
                {
                    main.fieldOfView = m_EffectInfo.m_ShakeFOV.m_FinalScaleOfFOV * m_CurVT.m_ViewInfo.fov;
                }
            }
        }
        
        private float GetPlatformDragSensitivity()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return m_DragSensitivityOnPC;
#else
            return m_DragSensitivityOnMobile;
#endif
        }

        private float GetPlatformPinchSensitivity()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return m_PinchSensitivityOnPC;
#else
            return m_PinchSensitivityOnMobile;
#endif
        }

        public void AddCullingMask(int mask)
        {
            if (main != null)
            {
                main.cullingMask |= mask;
            }
        }

        public void RemoveCullingMask(int mask)
        {
            if (main != null)
            {
                main.cullingMask &= ~mask;
            }
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            if( main != null )
            {
                return main.WorldToViewportPoint(position);
            }
            return position;
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            if(main != null)
            {
                return main.WorldToScreenPoint(position);
            }
            return position;
        }

        public Ray ScreenPointToRay(Vector2 position)
        {
            if(main != null)
            {
                Vector2 view = new Vector2(position.x / Screen.width, position.y / Screen.height);
                return main.ViewportPointToRay(view);//正龙那边会改rt大小，不能直接用ScreenPointToRay
            }
            return new Ray();
        }

        public Vector3 position
        {
            get
            {
                return main != null ? main.transform.position : transform.position;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return Quaternion.Euler(m_CurVT.m_ViewInfo.pitch, m_CurVT.m_ViewInfo.yaw, 0);
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                return new Vector3(m_CurVT.m_ViewInfo.pitch, m_CurVT.m_ViewInfo.yaw, 0);
            }
        }

        public Vector3 direction
        {
            get
            {
                return transform.forward;
            }
        }

        public Vector3 directionXZ
        {
            get
            {
                float radian = m_CurVT.m_ViewInfo.yaw * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
            }
        }

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

        private bool m_isWatching;
        private bool m_isRecovering;
        public void BeginWatching()
        {
            m_DragDelta = Vector2.zero;
            m_isWatching = true;
            m_isRecovering = false;
        }

        public void ProcessInputWhenWatching(Vector2 dragDelta)
        {
            m_DragDelta = dragDelta;
            m_DragDelta.y *= -1;
        }

        public void EndWatching(float smoothTime)
        {
            m_DragDelta = Vector2.zero;
            m_isRecovering = true;

            SetPendingViewInfo(m_ViewTarget, m_CurVT.m_ViewInfo, smoothTime);

            m_PendingVT.m_ViewInfo.pitch = m_CurVT.m_ViewInfo.pitch;
            m_PendingVT.m_ViewInfo.yaw = NormalizeAngle(m_ViewTarget != null ? m_ViewTarget.transform.rotation.eulerAngles.y : 0);
            m_PendingVT.m_ViewInfo.distance = m_CurVT.m_ViewInfo.distance;
        }

        ///////////////////////////////////////////// 镜头切换、震屏等接口       
        /// <summary>
        /// 播放震屏接口
        /// </summary>
        /// <param name="effectInfo"></param>
        /// <param name="onFinished"></param>
        public void PlayCameraEffect(CameraEffectInfo effectInfo, System.Action onFinished = null)
        {
            if (effectInfo == null)
            {
                Debug.LogWarning("PlayCameraEffect: CameraEffectProfile == none");
                return;
            }

            if (m_EffectInfo != null && m_EffectInfo.IsPlaying() && m_EffectInfo.m_Priority > effectInfo.m_Priority)
                return;

            StopCameraEffect();

            m_EffectInfo = effectInfo;
            m_EffectInfo.Play(onFinished);
        }

        public void StopCameraEffect()
        {
            if (m_EffectInfo != null && m_EffectInfo.IsPlaying())
            {
                m_EffectInfo.Stop();

                UpdateCameraEffect();
            }
        }

#if UNITY_EDITOR
        public void EditorSetCharacterView(CameraViewInfoCollection.CharacterView charView, bool isAiming = false, float smoothTime = 0.15f)
        {
            SetCharacterView(charView, isAiming, smoothTime);
            curCharView = charView;
        }

        public void GetViewInfo(out Transform viewTarget, out CameraViewInfo viewInfo, out CameraViewInfo srcViewInfo)
        {
            viewTarget = (m_PendingVT != null && m_PendingVT.m_ViewTarget != null) ? m_PendingVT.m_ViewTarget : null;
            viewInfo = (m_PendingVT != null && m_PendingVT.m_ViewInfo != null) ? m_PendingVT.m_ViewInfo : null;
            srcViewInfo = m_ViewInfoCollection != null ? m_ViewInfoCollection[curCharView] : null;
        }

        public void SaveToDefault(Transform viewTarget, CameraViewInfo srcViewInfo, CameraViewInfo dstViewInfo)
        {
            if (srcViewInfo == null || dstViewInfo == null)
            {
                Debug.LogError("CameraController.Save: srcViewInfo == null ||dstViewInfo == null");
                return;
            }

            dstViewInfo.rigOffset           = srcViewInfo.rigOffset;
            dstViewInfo.rigOffsetWhenAim    = srcViewInfo.rigOffsetWhenAim;
            dstViewInfo.defaultFOV          = srcViewInfo.fov;
            dstViewInfo.defaultPitch        = srcViewInfo.pitch;
            dstViewInfo.minPitch            = srcViewInfo.minPitch;
            dstViewInfo.maxPitch            = srcViewInfo.maxPitch;
            dstViewInfo.defaultYaw          = 0;
            dstViewInfo.defaultDistance     = srcViewInfo.distance;
            dstViewInfo.minDistance         = srcViewInfo.minDistance;
            dstViewInfo.maxDistance         = srcViewInfo.maxDistance;
        }

        private CameraViewInfoCollection.CharacterView m_curCharView = CameraViewInfoCollection.CharacterView.Run;
        public CameraViewInfoCollection.CharacterView curCharView { get { return m_curCharView; } set { m_curCharView = value; } }

        public bool m_isAiming { get; set; }


        public void GetEffectProfile(out CameraEffectInfo profile)
        {
            profile = m_EffectInfo;
        }
        public string effectProfileAssetPath { get; set; }
#endif
    }
}