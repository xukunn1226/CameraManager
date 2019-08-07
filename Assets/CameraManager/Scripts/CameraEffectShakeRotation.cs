using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SCamera
{
    [System.Serializable]
    public class CameraEffectShakeRotation : CameraEffectBase
    {
        public Vector3          strength;

        public AnimationCurve   xCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   yCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   zCurve          = AnimationCurve.Linear(0, 1, 1, 0);

        public Quaternion       FinalRotation   { get; private set; }

        private float           _xCurveLength;
        private float           _yCurveLength;
        private float           _zCurveLength;

        private Quaternion      _startFadeOutRotation;

        public override void OnBegin(Camera cam, float duration)
        {
            base.OnBegin(cam, duration);

            FinalRotation = Quaternion.identity;
            _xCurveLength = xCurve.keys.Length > 0 ? xCurve.keys[xCurve.keys.Length - 1].time : 0;
            _yCurveLength = yCurve.keys.Length > 0 ? yCurve.keys[yCurve.keys.Length - 1].time : 0;
            _zCurveLength = zCurve.keys.Length > 0 ? zCurve.keys[zCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(Camera cam, float time)
        {
            Vector3 randomRotation = Vector3.zero;
                        
            randomRotation.x = strength.x * xCurve.Evaluate(time * _xCurveLength / _duration);
            randomRotation.y = strength.y * yCurve.Evaluate(time * _yCurveLength / _duration);
            randomRotation.z = strength.z * zCurve.Evaluate(time * _zCurveLength / _duration);
            FinalRotation = Quaternion.Euler(randomRotation);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            FinalRotation = Quaternion.identity;
        }

        public override void OnBeginFadeOut(Camera cam, float duration)
        {
            base.OnBeginFadeOut(cam, duration);

            _startFadeOutRotation = FinalRotation;
        }

        public override void OnSampleFadeOut(Camera cam, float time)
        {
            base.OnSampleFadeOut(cam, time);

            FinalRotation = Quaternion.Lerp(_startFadeOutRotation, Quaternion.identity, time / _durationFadeOut);
        }

        public override void OnEndFadeOut()
        {
            base.OnEndFadeOut();

            FinalRotation = Quaternion.identity;
        }
    }
}