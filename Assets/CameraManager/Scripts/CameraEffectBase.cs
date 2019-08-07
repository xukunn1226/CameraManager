using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{    
    public class CameraEffectBase
    {
        [SerializeField]
        public bool         active;

        protected float     _duration;
        protected float     _durationFadeOut;

        public virtual void OnBegin(Camera cam, float duration)             { _duration = Mathf.Max(0.1f, duration); }

        public virtual void OnSample(Camera cam, float time) { }

        public virtual void OnEnd() { }

        public virtual void OnBeginFadeOut(Camera cam, float duration)      { _durationFadeOut = Mathf.Max(0.1f, duration); }

        public virtual void OnSampleFadeOut(Camera cam, float time) { }

        public virtual void OnEndFadeOut() { }
    }
}