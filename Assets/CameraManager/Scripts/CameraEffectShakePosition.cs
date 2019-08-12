using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class CameraEffectShakePosition : CameraEffectBase
    {
        public Vector3          m_Strength;

        public AnimationCurve   m_XCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_YCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_ZCurve          = AnimationCurve.Linear(0, 1, 1, 0);

        public Vector3          m_FinalPosition   { get; private set; }

        private float           m_XCurveLength;
        private float           m_YCurveLength;
        private float           m_ZCurveLength;


        public override void OnBegin(Camera cam, float duration)
        {
            base.OnBegin(cam, duration);

            m_FinalPosition = Vector3.zero;
            m_XCurveLength = m_XCurve.keys.Length > 0 ? m_XCurve.keys[m_XCurve.keys.Length - 1].time : 0;
            m_YCurveLength = m_YCurve.keys.Length > 0 ? m_YCurve.keys[m_YCurve.keys.Length - 1].time : 0;
            m_ZCurveLength = m_ZCurve.keys.Length > 0 ? m_ZCurve.keys[m_ZCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(Camera cam, float time)
        {
            Vector3 randomPosition = Vector3.zero;

            randomPosition.x = m_Strength.x * m_XCurve.Evaluate(time * m_XCurveLength / m_Duration);
            randomPosition.y = m_Strength.y * m_YCurve.Evaluate(time * m_YCurveLength / m_Duration);
            randomPosition.z = m_Strength.z * m_ZCurve.Evaluate(time * m_ZCurveLength / m_Duration);

            m_FinalPosition = randomPosition;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            m_FinalPosition = Vector3.zero;
        }
    }
}