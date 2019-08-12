using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位集合数据", fileName = "CameraViewInfoCollection")]
    public class CameraViewInfoCollection : ScriptableObject
    {
        public enum CharacterView
        {
            Walk,
            Run,
            Sprint,
            Squat,
            Roll,
            Jump,
            Fly,
            Max,
        }

        public CameraViewInfo[] m_CharView = new CameraViewInfo[(int)CharacterView.Max];

        public CameraViewInfo this[CharacterView index]
        {
            get
            {
                if (index < 0 || index >= CharacterView.Max)
                    return null;

                return m_CharView[(int)index];
            }
        }
    }
}