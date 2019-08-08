using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位数据", fileName = "CameraViewInfo")]
    public class CameraViewInfo : ScriptableObject
    {
        public Vector3  rig                 { get; set; }
        public Vector3  rigOffset           = new Vector3(0, 1.8f, 0);
        public Vector3  rigOffsetWhenAim    = new Vector3(0, 1.8f, 0);
        private Vector3 rigVelocity;

        public float    fov                 { get; set; }
        public float    defaultFOV          = 50;
        private float   fovVelocity;

        public float    pitch               { get; set;}
        public float    defaultPitch        = 30;
        public float    minPitch            = -40;
        public float    maxPitch            = 70;
        private float   pitchVelocity;

        public float    yaw                 { get; set;}
        public float    defaultYaw          = 0;                // 相对viewTarget的值
        private float   yawVelocity;

        public float    distance            { get; set;}
        public float    defaultDistance     = 15;
        public float    minDistance         = 1;
        public float    maxDistance         = 50;
        private float   distanceVelocity;

        /// <summary>
        /// 初始化静态数据、运行时数据、临时变量
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfo"></param>
        public void Init(Transform InViewTarget, CameraViewInfo InViewInfo)
        {
            CopyFrom(InViewInfo);

            fov = defaultFOV;
            pitch = defaultPitch;
            yaw = (InViewTarget != null ? InViewTarget.transform.rotation.eulerAngles.y : 0) + defaultYaw;
            distance = defaultDistance;

            rigVelocity = Vector3.zero;
            fovVelocity = 0;
            pitchVelocity = 0;
            yawVelocity = 0;
            distanceVelocity = 0;
        }

        /// <summary>
        /// 仅复制静态数据
        /// </summary>
        /// <param name="InViewInfo"></param>
        public void CopyFrom(CameraViewInfo InViewInfo)
        {
            rigOffset          = InViewInfo.rigOffset;
            rigOffsetWhenAim   = InViewInfo.rigOffsetWhenAim;

            defaultFOV         = InViewInfo.defaultFOV;

            defaultPitch       = InViewInfo.defaultPitch;
            minPitch           = InViewInfo.minPitch;
            maxPitch           = InViewInfo.maxPitch;

            defaultYaw         = InViewInfo.defaultYaw;

            defaultDistance    = InViewInfo.defaultDistance;
            minDistance        = InViewInfo.minDistance;
            maxDistance        = InViewInfo.maxDistance;
        }

        public bool SmoothDamp(CameraViewInfo a, CameraViewInfo b, float smoothTime)
        {
            rig                 = Vector3.SmoothDamp(a.rig, b.rig, ref rigVelocity, smoothTime);
            
            fov                 = Mathf.SmoothDamp(a.fov, b.fov, ref fovVelocity, smoothTime);
            distance            = Mathf.SmoothDamp(a.distance, b.distance, ref distanceVelocity, smoothTime);

            pitch               = Mathf.SmoothDampAngle(a.pitch, b.pitch, ref pitchVelocity, smoothTime);
            yaw                 = Mathf.SmoothDampAngle(a.yaw, b.yaw, ref yawVelocity, smoothTime);

            const float threshold = 0.05f;
            return Mathf.Abs(fovVelocity) < threshold && Mathf.Abs(pitchVelocity) < threshold && Mathf.Abs(yawVelocity) < threshold;
        }
    }
}