using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{    
    public class CameraEffectBase
    {
        [SerializeField]
        public bool         m_Active;

        protected float     m_Duration;

        public virtual void OnBegin(Camera cam, float duration)             { m_Duration = Mathf.Max(0.1f, duration); }

        public virtual void OnSample(Camera cam, float time) { }

        public virtual void OnEnd() { }
    }
}