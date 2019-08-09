using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位集合数据", fileName = "CameraViewInfoCollection")]
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
    }
}