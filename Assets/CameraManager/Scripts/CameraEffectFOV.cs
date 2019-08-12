using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class CameraEffectFOV : CameraEffectBase
    {
        [Tooltip("最小FOV缩放系数")]
        public float            m_MinScaleOfFOV       = 0.5f;

        [Tooltip("最大FOV缩放系数")]
        public float            m_MaxScaleOfFOV       = 2.0f;

        public AnimationCurve   m_DampCurve           = AnimationCurve.Linear(0, 1, 1, 0);

        public float            m_FinalScaleOfFOV     { get; private set; }

        private float           m_CurveLength;        

        public override void OnBegin(Camera cam, float duration)
        {
            base.OnBegin(cam, duration);

            m_FinalScaleOfFOV = 1;
            m_CurveLength = m_DampCurve.keys.Length > 0 ? m_DampCurve.keys[m_DampCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(Camera cam, float time)
        {
            m_FinalScaleOfFOV = Mathf.Clamp(m_DampCurve.Evaluate(time * m_CurveLength / m_Duration), m_MinScaleOfFOV, m_MaxScaleOfFOV);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            m_FinalScaleOfFOV = 1;
        }
    }
}