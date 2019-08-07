using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SCamera
{
    [System.Serializable]
    public class CameraEffectShakePosition : CameraEffectBase
    {
        public Vector3          strength;

        public AnimationCurve   xCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   yCurve          = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   zCurve          = AnimationCurve.Linear(0, 1, 1, 0);

        public Vector3          FinalPosition   { get; private set; }

        private float           _xCurveLength;
        private float           _yCurveLength;
        private float           _zCurveLength;

        private Vector3         _startFadeOutPosition;

        public override void OnBegin(Camera cam, float duration)
        {
            base.OnBegin(cam, duration);

            FinalPosition = Vector3.zero;
            _xCurveLength = xCurve.keys.Length > 0 ? xCurve.keys[xCurve.keys.Length - 1].time : 0;
            _yCurveLength = yCurve.keys.Length > 0 ? yCurve.keys[yCurve.keys.Length - 1].time : 0;
            _zCurveLength = zCurve.keys.Length > 0 ? zCurve.keys[zCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(Camera cam, float time)
        {
            Vector3 randomPosition = Vector3.zero;

            randomPosition.x = strength.x * xCurve.Evaluate(time * _xCurveLength / _duration);
            randomPosition.y = strength.y * yCurve.Evaluate(time * _yCurveLength / _duration);
            randomPosition.z = strength.z * zCurve.Evaluate(time * _zCurveLength / _duration);

            FinalPosition = randomPosition;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            FinalPosition = Vector3.zero;
        }

        public override void OnBeginFadeOut(Camera cam, float duration)
        {
            base.OnBeginFadeOut(cam, duration);

            _startFadeOutPosition = FinalPosition;
        }

        public override void OnSampleFadeOut(Camera cam, float time)
        {
            base.OnSampleFadeOut(cam, time);

            FinalPosition = Vector3.Lerp(_startFadeOutPosition, Vector3.zero, time / _durationFadeOut);
        }

        public override void OnEndFadeOut()
        {
            base.OnEndFadeOut();

            FinalPosition = Vector3.zero;
        }
    }
}