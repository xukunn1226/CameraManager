using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位数据", fileName = "CameraViewInfo")]
    public class CameraViewInfo : ScriptableObject
    {
        public Vector3  rig             = Vector3.zero;
        public Vector3  rigOffset       = new Vector3(0, 1.8f, 0);
        public Vector3  rigOffset2      = new Vector3(0, 1.8f, 0);
        private Vector3 rigVelocity;
        private Vector3 rigOffsetVelocity;

        public float    fov             { get; set; }
        public float    defaultFOV      = 50;
        private float   fovVelocity;

        public float    pitch           { get; set;}
        public float    defaultPitch    = 30;
        public float    minPitch        = -40;
        public float    maxPitch        = 70;
        private float   pitchVelocity;

        public float    yaw             { get; set;}
        public float    defaultYaw      = 0;                // 相对viewTarget的值
        private float   yawVelocity;

        public float    distance        { get; set;}
        public float    defaultDistance = 15;
        public float    minDistance     = 1;
        public float    maxDistance     = 50;
        private float   distanceVelocity;


        public void CopyFrom(CameraViewInfo InViewInfo)
        {
            rig                = InViewInfo.rig;
            rigOffset          = InViewInfo.rigOffset;
            rigOffset2         = InViewInfo.rigOffset2;

            defaultFOV         = InViewInfo.defaultFOV;
            fov                = InViewInfo.fov;

            defaultPitch       = InViewInfo.defaultPitch;
            pitch              = InViewInfo.pitch;
            minPitch           = InViewInfo.minPitch;
            maxPitch           = InViewInfo.maxPitch;

            defaultYaw         = InViewInfo.defaultYaw;
            yaw                = InViewInfo.yaw;

            defaultDistance    = InViewInfo.defaultDistance;
            distance           = InViewInfo.distance;
            minDistance        = InViewInfo.minDistance;
            maxDistance        = InViewInfo.maxDistance;
        }

        public bool SmoothDamp(CameraViewInfo a, CameraViewInfo b, float smoothTime)
        {
            rig         = Vector3.SmoothDamp(a.rig, b.rig, ref rigVelocity, smoothTime);
            rigOffset   = Vector3.SmoothDamp(a.rigOffset, b.rigOffset, ref rigOffsetVelocity, smoothTime);
            rigOffset2  = Vector3.SmoothDamp(a.rigOffset2, b.rigOffset2, ref rigOffsetVelocity, smoothTime);

            fov         = Mathf.SmoothDamp(a.fov, b.fov, ref fovVelocity, smoothTime);
            distance    = Mathf.SmoothDamp(a.distance, b.distance, ref distanceVelocity, smoothTime);

            pitch       = Mathf.SmoothDampAngle(a.pitch, b.pitch, ref pitchVelocity, smoothTime);
            yaw         = Mathf.SmoothDampAngle(a.yaw, b.yaw, ref yawVelocity, smoothTime);

            const float threshold = 0.05f;
            return Mathf.Abs(fovVelocity) < threshold && Mathf.Abs(pitchVelocity) < threshold && Mathf.Abs(yawVelocity) < threshold;
        }
    }
}