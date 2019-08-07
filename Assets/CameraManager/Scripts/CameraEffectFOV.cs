using UnityEngine;

namespace Framework.SCamera
{
    [System.Serializable]
    public class CameraEffectFOV : CameraEffectBase
    {
        [Tooltip("最小FOV缩放系数")]
        public float            minScaleOfFOV       = 0.5f;

        [Tooltip("最大FOV缩放系数")]
        public float            maxScaleOfFOV       = 2.0f;

        public AnimationCurve   dampCurve           = AnimationCurve.Linear(0, 1, 1, 0);

        public float            FinalScaleOfFOV     { get; private set; }

        private float           _curveLength;

        private float           _startFadeOutFOV;


        public override void OnBegin(Camera cam, float duration)
        {
            base.OnBegin(cam, duration);

            FinalScaleOfFOV = 1;
            _curveLength = dampCurve.keys.Length > 0 ? dampCurve.keys[dampCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(Camera cam, float time)
        {
            FinalScaleOfFOV = Mathf.Clamp(dampCurve.Evaluate(time * _curveLength / _duration), minScaleOfFOV, maxScaleOfFOV);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            FinalScaleOfFOV = 1;
        }

        public override void OnBeginFadeOut(Camera cam, float duration)
        {
            base.OnBeginFadeOut(cam, duration);

            _startFadeOutFOV = FinalScaleOfFOV;
        }

        public override void OnSampleFadeOut(Camera cam, float time)
        {
            base.OnSampleFadeOut(cam, time);

            FinalScaleOfFOV = Mathf.Lerp(_startFadeOutFOV, 1, time / _durationFadeOut);
        }

        public override void OnEndFadeOut()
        {
            base.OnEndFadeOut();

            FinalScaleOfFOV = 1;
        }
    }
}