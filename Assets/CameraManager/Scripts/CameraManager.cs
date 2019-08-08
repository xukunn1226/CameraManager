using UnityEngine;

namespace Framework
{
    public class CameraManager : MonoBehaviour
    {
        static public CameraManager     instance
        {
            get;
            private set;
        }

        public Camera                   main                                { get; private set; }

        [Space(10)]
        private Vector2                 m_DragDelta;                                                // .x表示Yaw；.y表示Pitch
        private float                   m_PinchDelta;                                               // 
        
        public float                    m_DragSensitivityOnMobile           = 3.0f;                 // 移动平台上对拖拽时灵敏度的加乘(multiply)
        public float                    m_DragSensitivityOnPC               = 0.2f;                 // PC平台上对拖拽时灵敏度的加乘(multiply)

        private float                   m_PinchSensitivityOnMobile          = 1.0f;                 // 【暂时屏蔽】移动平台上pinch时灵敏度的加乘(multiply)       
        private float                   m_PinchSensitivityOnPC              = 0.02f;                // 【暂时屏蔽】PC平台上pinch时灵敏度的加乘(multiply)       

        public float                    m_SmoothTime                        = 0.2f;                 // drag，pinch操作的过渡参数
        public LayerMask                m_CollisionMask;                                            // 镜头碰撞Mask
        public float                    m_SphereCastRadius                  = 0.3f;
        
        private class FViewTarget
        {
            public Transform            m_ViewTarget;
            public CameraViewInfo       m_ViewInfo;

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

        private bool                        m_bTransition;
        private float                       m_SmoothTimeToTarget;

        private Vector3                     m_RigVelocity;
        private float                       m_FovVelocity;
        private float                       m_DisVelocity;
        private float                       m_PitchVelocity;
        private float                       m_YawVelocity;

        private Ray                         m_Ray                                = new Ray();
        private RaycastHit[]                m_Hits                               = new RaycastHit[16];

        private CameraEffectInfo            m_EffectInfo;                       // 震屏效果

        private CameraViewInfoCollection    m_ViewInfoCollection;               // 当前的镜头组数据
        private Transform                   m_ViewTarget;                       // 跟随对象
        

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

        public void AddCullingMask(int mask)
        {
            if( main != null )
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

        public void SetClearFlag(CameraClearFlags flag)
        {
            if (main != null)
            {
                main.clearFlags = flag;
            }
        }

        /// <summary>
        /// 上层接口，初始化角色默认视角
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfoCollection"></param>
        public void InitViewInfoCollection(Transform InViewTarget, CameraViewInfoCollection InViewInfoCollection)
        {
            if (InViewTarget == null || InViewInfoCollection == null)
            {
                Debug.LogError("Camera Init: viewTarget == null || viewInfoProfile == null");
                return;
            }

            m_ViewInfoCollection = InViewInfoCollection;
            m_ViewTarget = InViewTarget;

            InitViewInfo(m_ViewTarget, m_ViewInfoCollection.m_RunView);
        }

        /// <summary>
        /// 上层接口，常规相机位的切换，根据需求pitch、yaw、distance维持不变
        /// </summary>
        /// <param name="InViewInfo"></param>
        /// <param name="smoothTime"></param>
        public void ChangeViewInfo(CameraViewInfo InViewInfo, float smoothTime = 0.2f)
        {
            SetPendingViewInfo(m_ViewTarget, InViewInfo, smoothTime);

            // procedural view info, 继承当前pitch，yaw，distance
            m_PendingVT.m_ViewInfo.pitch = m_CurVT.m_ViewInfo.pitch;
            m_PendingVT.m_ViewInfo.yaw = m_CurVT.m_ViewInfo.yaw;
            m_PendingVT.m_ViewInfo.distance = m_CurVT.m_ViewInfo.distance;
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
        private void SetPendingViewInfo(Transform InViewTarget, CameraViewInfo InViewInfo, float smoothTime = 0.2f)
        {
            m_PendingVT.InitViewTarget(InViewTarget, InViewInfo);

            m_RigVelocity = Vector3.zero;
            m_FovVelocity = 0;
            m_PitchVelocity = 0;
            m_YawVelocity = 0;
            m_DisVelocity = 0;
            m_SmoothTime = smoothTime;
        }
        
        private void ProcessInput()
        {
            m_DragDelta = (InputManager.instance != null ? InputManager.instance.DragDelta : Vector2.zero) * GetPlatformDragSensitivity();
            m_DragDelta.y *= -1;

            m_PinchDelta = 0;
        }

        private void UpdateViewTarget()
        {
            if (m_PendingVT.m_ViewTarget != null)
                m_PendingVT.m_ViewInfo.rig = m_PendingVT.m_ViewTarget.position + m_PendingVT.m_ViewTarget.TransformVector(m_PendingVT.m_ViewInfo.rigOffset);
            
            if ( m_bTransition )
            {
                if( m_SmoothTimeToTarget <= 0.0001f /*|| _curVT.viewInfo.SmoothDamp(_curVT.viewInfo, _pendingVT.viewInfo, _smoothTimeToTarget)*/ )
                {
                    m_bTransition = false;
                    //m_CurVT = new FViewTarget(m_PendingVT.viewTarget, m_PendingVT.viewInfo);
                }

                if(m_bTransition)
                {
                        m_PendingVT.m_ViewInfo.pitch = m_PendingVT.m_ViewInfo.defaultPitch;

                    if(m_CurVT.m_ViewInfo.SmoothDamp(m_CurVT.m_ViewInfo, m_PendingVT.m_ViewInfo, m_SmoothTimeToTarget))
                    {
                        m_bTransition = false;
                        //m_CurVT = new FViewTarget(m_PendingVT.viewTarget, m_PendingVT.viewInfo);
                    }
                }
            }
            else
            {
                m_CurVT.m_ViewInfo.rig = Vector3.SmoothDamp(m_CurVT.m_ViewInfo.rig, m_PendingVT.m_ViewInfo.rig, ref m_RigVelocity, 0);
                
                // fov
                m_CurVT.m_ViewInfo.fov = Mathf.SmoothDamp(m_CurVT.m_ViewInfo.fov, m_PendingVT.m_ViewInfo.fov, ref m_FovVelocity, m_SmoothTime);

                // distance
                m_PendingVT.m_ViewInfo.distance = Mathf.Clamp(m_PendingVT.m_ViewInfo.distance + m_PinchDelta, m_PendingVT.m_ViewInfo.minDistance, m_PendingVT.m_ViewInfo.maxDistance);
                m_CurVT.m_ViewInfo.distance = Mathf.SmoothDamp(m_CurVT.m_ViewInfo.distance, m_PendingVT.m_ViewInfo.distance, ref m_DisVelocity, m_SmoothTime);
                
                // pitch
                m_PendingVT.m_ViewInfo.pitch = NormalizeAngle(Mathf.Clamp(m_PendingVT.m_ViewInfo.pitch + m_DragDelta.y, m_PendingVT.m_ViewInfo.minPitch, m_PendingVT.m_ViewInfo.maxPitch));
                m_CurVT.m_ViewInfo.pitch = NormalizeAngle(Mathf.SmoothDampAngle(m_CurVT.m_ViewInfo.pitch, m_PendingVT.m_ViewInfo.pitch, ref m_PitchVelocity, m_SmoothTime));

                // yaw
                m_PendingVT.m_ViewInfo.yaw = NormalizeAngle(m_PendingVT.m_ViewInfo.yaw + m_DragDelta.x);
                m_CurVT.m_ViewInfo.yaw = NormalizeAngle(Mathf.SmoothDampAngle(m_CurVT.m_ViewInfo.yaw, m_PendingVT.m_ViewInfo.yaw, ref m_YawVelocity, m_SmoothTime));
            }
        }

        private void UpdateCamera()
        {
            transform.position = m_CurVT.m_ViewInfo.rig;
            transform.rotation = Quaternion.Euler(m_CurVT.m_ViewInfo.pitch, m_CurVT.m_ViewInfo.yaw, 0);

            CheckCollision();

            //vcTransform.transform.localPosition = Vector3.forward * _curVT.viewInfo.distance * -1;
            //vcTransform.m_Lens.FieldOfView = _curVT.viewInfo.fov;
        }

        private void CheckCollision()
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

            if( index != -1 )
            {
                m_CurVT.m_ViewInfo.distance = m_Hits[index].distance;
            }
        }

        private void UpdateCameraEffect()
        {
            if( m_EffectInfo != null && m_EffectInfo.IsPlaying() )
            {
                m_EffectInfo.UpdateCameraEffect(main);

                if( m_EffectInfo.shakePosition.active )
                {
                    main.transform.localPosition += m_EffectInfo.shakePosition.FinalPosition;
                }

                if( m_EffectInfo.shakeRotation.active )
                {
                    main.transform.localRotation = m_EffectInfo.shakeRotation.FinalRotation;
                }

                if( m_EffectInfo.shakeFOV.active )
                {
                    main.fieldOfView = m_EffectInfo.shakeFOV.FinalScaleOfFOV * m_CurVT.m_ViewInfo.fov;
                }
            }
        }
        
        /// <summary>
        /// camera update pipeline
        /// </summary>
        void LateUpdate()
        {
            if (m_CurVT == null)
                return;

            ProcessInput();

            UpdateViewTarget();

            UpdateCamera();

            UpdateCameraEffect();
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
                return main != null ? main.transform.rotation : transform.rotation;
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

        ///////////////////////////////////////////// 镜头切换、震屏等接口       
        /// <summary>
        /// 播放震屏接口
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="onFinished"></param>
        public void PlayCameraEffect(CameraEffectInfo profile, System.Action onFinished = null)
        {
            if (profile == null)
            {
                Debug.LogWarning("PlayCameraEffect: CameraEffectProfile == none");
                return;
            }

            if (m_EffectInfo != null && m_EffectInfo.IsPlaying() && m_EffectInfo.priority > profile.priority)
                return;

            StopCameraEffect();

            m_EffectInfo = profile;
            m_EffectInfo.Play(true, onFinished);
        }

        public void StopCameraEffect()
        {
            if (m_EffectInfo != null && m_EffectInfo.IsPlaying())
            {
                m_EffectInfo.Stop(0);

                UpdateCameraEffect();
            }
        }

#if UNITY_EDITOR
        public void EditorSetViewTarget(Transform viewTarget, CameraViewInfo viewInfo, float smoothTime)
        {
            //SetViewTarget(viewTarget, viewInfo, smoothTime);
        }

        public void GetViewInfo(out Transform viewTarget, out CameraViewInfo viewInfo)
        {
            viewTarget = (m_PendingVT != null && m_PendingVT.m_ViewTarget != null) ? m_PendingVT.m_ViewTarget : null;
            viewInfo = (m_PendingVT != null && m_PendingVT.m_ViewInfo != null) ? m_PendingVT.m_ViewInfo : null;
        }

        public void GetEffectProfile(out CameraEffectInfo profile)
        {
            profile = m_EffectInfo;
        }

        public void GetViewInfoProfile(out CameraViewInfoCollection viewInfoProfile)
        {
            viewInfoProfile = m_ViewInfoCollection;
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
        public Camera Camera { get => main; set => main = value; }
#endif
    }
}