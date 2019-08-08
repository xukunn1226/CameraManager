using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位集合数据", fileName = "CameraViewInfoProfile")]
    public class CameraViewInfoCollection : ScriptableObject
    {
        // 各种移动方式下的镜头参数
        public CameraViewInfo   m_WalkView;
        public CameraViewInfo   m_RunView;
        public CameraViewInfo   m_SprintView;
        public CameraViewInfo   m_SquatView;
        public CameraViewInfo   m_RollView;
        public CameraViewInfo   m_JumpView;
        public CameraViewInfo   m_FlyView;

        static public CameraViewInfoCollection CopyFrom(CameraViewInfoCollection src)
        {
            CameraViewInfoCollection dst    = Instantiate(src);
            dst.m_WalkView                  = Instantiate(src.m_WalkView);
            dst.m_RunView                   = Instantiate(src.m_RunView);
            dst.m_SprintView                = Instantiate(src.m_SprintView);
            dst.m_SquatView                 = Instantiate(src.m_SquatView);
            dst.m_RollView                  = Instantiate(src.m_RollView);
            dst.m_JumpView                  = Instantiate(src.m_JumpView);
            dst.m_FlyView                   = Instantiate(src.m_FlyView);
            return dst;
        }
    }
}