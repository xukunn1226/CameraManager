using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位数据", fileName = "CameraViewInfo")]
    public class CameraViewInfo : ScriptableObject
    {
        public Vector3  rig                 { get; set; }
        public Vector3  rigOffset           = new Vector3(0, 1.8f, 0);
        public Vector3  rigOffsetWhenAim    = new Vector3(0, 1.8f, 0);

        public float    fov                 { get; set; }
        public float    defaultFOV          = 50;

        public float    pitch               { get; set;}
        public float    defaultPitch        = 30;
        public float    minPitch            = -40;
        public float    maxPitch            = 70;

        public float    yaw                 { get; set;}
        public float    defaultYaw          = 0;                // 相对viewTarget的值

        public float    distance            { get; set;}
        public float    defaultDistance     = 15;
        public float    minDistance         = 1;
        public float    maxDistance         = 50;

        /// <summary>
        /// 初始化静态数据、运行时数据
        /// </summary>
        /// <param name="InViewTarget"></param>
        /// <param name="InViewInfo"></param>
        public void Init(Transform InViewTarget, CameraViewInfo InViewInfo)
        {
            // step 1. copy static data
            CopyFrom(InViewInfo);

            // step 2. calc runtime variants
            if(InViewTarget != null)
                rig = InViewTarget.position + InViewTarget.TransformVector(rigOffset);
            fov = defaultFOV;
            pitch = defaultPitch;
            yaw = CameraManager.NormalizeAngle((InViewTarget != null ? InViewTarget.transform.rotation.eulerAngles.y : 0) + defaultYaw);
            distance = defaultDistance;
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
    }
}