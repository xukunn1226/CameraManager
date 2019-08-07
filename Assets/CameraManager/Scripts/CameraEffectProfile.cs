using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SCamera
{
    [CreateAssetMenu(menuName = "创建相机震屏效果", fileName = "CameraEffectProfile")]
    public class CameraEffectProfile : ScriptableObject
    {
        [Tooltip("震屏时间")]
        public float                        duration        = 1;

        [Tooltip("优先级，值越大优先级越高")]
        public int                          priority;

        public CameraEffectShakePosition    shakePosition   = new CameraEffectShakePosition();
        public CameraEffectShakeRotation    shakeRotation   = new CameraEffectShakeRotation();
        public CameraEffectFOV              shakeFOV        = new CameraEffectFOV();
        public CameraEffectAspect           shakeAspect     = new CameraEffectAspect();

        private float                       _startTime;
        private System.Action               onFinished;
        private bool                        _autoFinish;        // true: 播放时间到就自动结束; false: 播放时间到则维持在最后时间点，直至主动请求结束
        private float                       _fadeOutTime;       // autoFinish = false时切换回初始状态的时间
        private float                       _fadeOutStartTime;

        private enum CameraEffectState
        {
            None,
            Begin,
            Sample,
            End,
            FadeOutSample,
            FadeOutEnd,
        }
        private CameraEffectState           _effectState = CameraEffectState.None;

        private void Begin(Camera cam)
        {
            _startTime = Time.time;

            if( shakePosition.active )
            {
                shakePosition.OnBegin(cam, duration);
            }
            if( shakeRotation.active )
            {
                shakeRotation.OnBegin(cam, duration);
            }
            if( shakeFOV.active )
            {
                shakeFOV.OnBegin(cam, duration);
            }
            if( shakeAspect.active )
            {
                shakeAspect.OnBegin(cam, duration);
            }
        }

        private void Sample(Camera cam, float time)
        {
            if (shakePosition.active)
            {
                shakePosition.OnSample(cam, time);
            }
            if (shakeRotation.active)
            {
                shakeRotation.OnSample(cam, time);
            }
            if (shakeFOV.active)
            {
                shakeFOV.OnSample(cam, time);
            }
            if (shakeAspect.active)
            {
                shakeAspect.OnSample(cam, time);
            }
        }

        private void End()
        {
            if (shakePosition.active)
            {
                shakePosition.OnEnd();
            }
            if (shakeRotation.active)
            {
                shakeRotation.OnEnd();
            }
            if (shakeFOV.active)
            {
                shakeFOV.OnEnd();
            }
            if (shakeAspect.active)
            {
                shakeAspect.OnEnd();
            }
        }

        private void BeginFadeOut(Camera cam)
        {
            _fadeOutStartTime = Time.time;

            if (shakePosition.active)
            {
                shakePosition.OnBeginFadeOut(cam, _fadeOutTime);
            }
            if (shakeRotation.active)
            {
                shakeRotation.OnBeginFadeOut(cam, _fadeOutTime);
            }
            if (shakeFOV.active)
            {
                shakeFOV.OnBeginFadeOut(cam, _fadeOutTime);
            }
            if (shakeAspect.active)
            {
                shakeAspect.OnBeginFadeOut(cam, _fadeOutTime);
            }
        }

        private void SampleFadeOut(Camera cam, float time)
        {
            if (shakePosition.active)
            {
                shakePosition.OnSampleFadeOut(cam, time);
            }
            if (shakeRotation.active)
            {
                shakeRotation.OnSampleFadeOut(cam, time);
            }
            if (shakeFOV.active)
            {
                shakeFOV.OnSampleFadeOut(cam, time);
            }
            if (shakeAspect.active)
            {
                shakeAspect.OnSampleFadeOut(cam, time);
            }
        }

        private void EndFadeOut()
        {
            if (shakePosition.active)
            {
                shakePosition.OnEndFadeOut();
            }
            if (shakeRotation.active)
            {
                shakeRotation.OnEndFadeOut();
            }
            if (shakeFOV.active)
            {
                shakeFOV.OnEndFadeOut();
            }
            if (shakeAspect.active)
            {
                shakeAspect.OnEndFadeOut();
            }
        }

        /// <summary>
        /// 震屏更新流程
        /// autoFinish = true， Begin -> Sample -> End
        /// autoFinish = false, Begin -> Sample -> End/BeginFadeOut -> SampleFadeOut -> EndFadeOut
        /// </summary>
        /// <param name="cam"></param>
        public void UpdateCameraEffect(Camera cam)
        {
            switch(_effectState)
            {
                case CameraEffectState.Begin:
                    {
                        Begin(cam);
                        _effectState = CameraEffectState.Sample;
                    }
                    break;
                case CameraEffectState.Sample:
                    {
                        float elapsedTime = Time.time - _startTime;
                        if( elapsedTime < duration )
                        {
                            Sample(cam, elapsedTime);
                        }
                        else
                        {
                            if( _autoFinish )
                            {
                                _effectState = CameraEffectState.End;
                            }
                            else
                            {
                                // 非自动结束始终维持在最后
                                Sample(cam, duration);
                            }
                        }
                    }
                    break;
                case CameraEffectState.End:
                    {
                        if( _autoFinish )
                        {
                            End();
                            _effectState = CameraEffectState.None;

                            if (onFinished != null)
                            {
                                onFinished();
                                onFinished = null;
                            }
                        }
                        else
                        {
                            BeginFadeOut(cam);
                            _effectState = CameraEffectState.FadeOutSample;
                        }
                    }
                    break;
                case CameraEffectState.FadeOutSample:
                    {
                        float elapsedTime = Time.time - _fadeOutStartTime;
                        if( elapsedTime < _fadeOutTime )
                        {
                            SampleFadeOut(cam, elapsedTime);
                        }
                        else
                        {
                            _effectState = CameraEffectState.FadeOutEnd;
                        }
                    }
                    break;
                case CameraEffectState.FadeOutEnd:
                    {
                        EndFadeOut();
                        _effectState = CameraEffectState.None;

                        if (onFinished != null)
                        {
                            onFinished();
                            onFinished = null;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 播放相机效果
        /// </summary>
        public void Play(bool autoFinish = true, System.Action onFinished = null)
        {
            _autoFinish = autoFinish;
            _effectState = CameraEffectState.Begin;
            this.onFinished = onFinished;
        }

        /// <summary>
        /// 立即停止播放相机效果
        /// </summary>
        public void Stop(float fadeOutTime)
        {
            _fadeOutTime = fadeOutTime;
            if( IsPlaying() )
            {
                _effectState = CameraEffectState.End;
            }
        }

        public bool IsPlaying()
        {
            return _effectState != CameraEffectState.None;
        }
    }
}