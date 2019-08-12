using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机震屏效果", fileName = "CameraEffectInfo")]
    public class CameraEffectInfo : ScriptableObject
    {
        [Tooltip("震屏时间")]
        public float                        m_Duration        = 1;

        [Tooltip("优先级，值越大优先级越高")]
        public int                          m_Priority;

        public CameraEffectShakePosition    m_ShakePosition   = new CameraEffectShakePosition();
        public CameraEffectShakeRotation    m_ShakeRotation   = new CameraEffectShakeRotation();
        public CameraEffectFOV              m_ShakeFOV        = new CameraEffectFOV();

        private float                       m_StartTime;
        private System.Action               onFinished;

        private enum CameraEffectState
        {
            None,
            Begin,
            Sample,
            End,
        }
        private CameraEffectState           _effectState = CameraEffectState.None;

        private void Begin(Camera cam)
        {
            m_StartTime = Time.time;

            if( m_ShakePosition.m_Active )
            {
                m_ShakePosition.OnBegin(cam, m_Duration);
            }
            if( m_ShakeRotation.m_Active )
            {
                m_ShakeRotation.OnBegin(cam, m_Duration);
            }
            if( m_ShakeFOV.m_Active )
            {
                m_ShakeFOV.OnBegin(cam, m_Duration);
            }
        }

        private void Sample(Camera cam, float time)
        {
            if (m_ShakePosition.m_Active)
            {
                m_ShakePosition.OnSample(cam, time);
            }
            if (m_ShakeRotation.m_Active)
            {
                m_ShakeRotation.OnSample(cam, time);
            }
            if (m_ShakeFOV.m_Active)
            {
                m_ShakeFOV.OnSample(cam, time);
            }
        }

        private void End()
        {
            if (m_ShakePosition.m_Active)
            {
                m_ShakePosition.OnEnd();
            }
            if (m_ShakeRotation.m_Active)
            {
                m_ShakeRotation.OnEnd();
            }
            if (m_ShakeFOV.m_Active)
            {
                m_ShakeFOV.OnEnd();
            }
        }
        
        /// <summary>
        /// 震屏更新流程
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
                        float elapsedTime = Time.time - m_StartTime;
                        if( elapsedTime < m_Duration )
                        {
                            Sample(cam, elapsedTime);
                        }
                        else
                        {
                            _effectState = CameraEffectState.End;
                        }
                    }
                    break;
                case CameraEffectState.End:
                    {
                        End();
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
        public void Play(System.Action onFinished = null)
        {
            _effectState = CameraEffectState.Begin;
            this.onFinished = onFinished;
        }

        /// <summary>
        /// 立即停止播放相机效果
        /// </summary>
        public void Stop()
        {
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