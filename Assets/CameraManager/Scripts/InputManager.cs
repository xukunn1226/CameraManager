using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 仅依赖FingerGesture和移动摇杆传入的JoystickAngle
    /// 1、捕获FingerGesture抛出的输入事件，并进行逻辑处理
    /// 2、仅处理场景中触发的输入，UI触发的直接抛弃
    /// </summary>
    [RequireComponent(typeof(ScreenRaycaster))]
    [RequireComponent(typeof(DragRecognizer))]
    //[RequireComponent(typeof(PinchRecognizer))]
    [RequireComponent(typeof(FingerUpDetector))]
    public class InputManager : MonoBehaviour
    {
        static public InputManager  instance        { get; private set; }
        static private float        INVALID_ANGLE   = 999f;

        public Vector2              DragDelta       { get; private set; }
        public float                PinchDelta      { get; private set; }
        public float                JoystickAngle   = INVALID_ANGLE;           // 摇杆产生的角度
        public bool                 JoystickUse     { get { return JoystickAngle != INVALID_ANGLE; } }

        private bool                m_Pinch;

        // 同时支持两个OnDrag操作，记录他们的状态
        private int                 m_FingerIndex1  = -1;
        private bool                m_bOverUI1;
        private int                 m_FingerIndex2  = -1;
        private bool                m_bOverUI2;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            gameObject.GetComponent<FingerUpDetector>().OnFingerUp += OnFingerUp;
            gameObject.GetComponent<DragRecognizer>().OnGesture += OnDrag;
            //gameObject.GetComponent<PinchRecognizer>().OnGesture += OnPinch;
        }

        private bool IsOverUI(Vector3 pos)
        {
            return false;
        }

        private void OnFingerUp(FingerUpEvent e)
        {
            if (m_FingerIndex1 != -1 || m_FingerIndex2 != -1|| m_Pinch || IsOverUI(e.Position))
                return;

            //Debug.LogFormat("--------------OnFingerUp Pos {0}   m_FingerIndex1: {1}     m_FingerIndex2: {2}     m_Pitch: {3}", e.Position, m_FingerIndex1, m_FingerIndex2, m_Pinch);
        }

        private void OnPinch(PinchGesture gesture)
        {
            if(!m_Pinch)
            {
                foreach (var finger in gesture.Fingers)
                {
                    if (IsOverUI(finger.StartPosition))         // 任何touch起始于UI则认为不是Pinch
                    {
                        m_Pinch = false;
                        PinchDelta = 0;
                        return;
                    }
                }
            }
        
            switch (gesture.Phase)
            {
                case ContinuousGesturePhase.Started:
                    {
                        m_Pinch = true;
                        PinchDelta = gesture.Delta;
                    }
                    break;
                case ContinuousGesturePhase.Updated:
                    {
                        PinchDelta = gesture.Delta;
                    }
                    break;
                case ContinuousGesturePhase.None:
                case ContinuousGesturePhase.Ended:
                    {
                        m_Pinch = false;
                        PinchDelta = 0;
                    }
                    break;
            }

            //if (gesture.Phase == ContinuousGesturePhase.Started || gesture.Phase == ContinuousGesturePhase.Ended)
            //{
            //    Debug.LogFormat("PINCH:  ClusterId:{0}    phase:{1}   pos:{2}   fingerCount:{3}     fingerIndex:{4}", gesture.ClusterId, gesture.Phase, gesture.Position, gesture.Fingers.Count, gesture.Fingers[0].Index);
            //}
        }

        /// <summary>
        /// 允许同时最多存在两个OnDrag操作
        /// 只允许有一个
        /// </summary>
        /// <param name="gesture"></param>
        private void OnDrag(DragGesture gesture)
        {        
            switch(gesture.Phase)
            {
                case ContinuousGesturePhase.Started:
                    {
                        bool bDoIt = false;
                        if(m_FingerIndex1 == -1)
                        {
                            m_FingerIndex1 = gesture.Fingers[0].Index;
                            m_bOverUI1 = IsOverUI(gesture.Fingers[0].StartPosition);
                            bDoIt = true;
                        }

                        if(!bDoIt && m_FingerIndex2 == -1)
                        {
                            m_FingerIndex2 = gesture.Fingers[0].Index;
                            m_bOverUI2 = IsOverUI(gesture.Fingers[0].StartPosition);
                        }

                        DragDelta = Vector2.zero;
                        if(!m_bOverUI1 || !m_bOverUI2)
                        {
                            DragDelta = gesture.DeltaMove;
                        }
                    }
                    break;
                case ContinuousGesturePhase.Updated:
                    {
                        DragDelta = Vector2.zero;

                        if (!m_bOverUI1 && m_FingerIndex1 == gesture.Fingers[0].Index)
                            DragDelta = gesture.DeltaMove;
                        else if (!m_bOverUI2 && m_FingerIndex2 == gesture.Fingers[0].Index)
                            DragDelta = gesture.DeltaMove;
                    }
                    break;
                case ContinuousGesturePhase.Ended:
                case ContinuousGesturePhase.None:
                    {
                        DragDelta = Vector2.zero;
                        if (m_FingerIndex1 == gesture.Fingers[0].Index)
                        {
                            m_FingerIndex1 = -1;
                            m_bOverUI1 = false;
                        }
                        if (m_FingerIndex2 == gesture.Fingers[0].Index)
                        {
                            m_FingerIndex2 = -1;
                            m_bOverUI2 = false;
                        }
                    }
                    break;
            }

            // 触发Pinch仍会收到OnDrag消息
            if(m_Pinch)
            {
                DragDelta = Vector2.zero;
            }

            //if (!m_Pinch)
            //{
            //    if (gesture.Phase == ContinuousGesturePhase.Started || gesture.Phase == ContinuousGesturePhase.Ended)
            //    {
            //        Debug.LogFormat("---DRAG:  m_FingerIndex1:{0}    m_FingerIndex2: {5}    phase:{1}   pos:{2}   fingerCount:{3}     fingerIndex:{4}", m_FingerIndex1, gesture.Phase, gesture.Position, gesture.Fingers.Count, gesture.Fingers[0].Index, m_FingerIndex2);
            //    }
            //}
        }
    

    #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Update()
        {
            bool A = Input.GetKey(KeyCode.A);
            bool S = Input.GetKey(KeyCode.S);
            bool D = Input.GetKey(KeyCode.D);
            bool W = Input.GetKey(KeyCode.W);

            if (A || S || D || W)
            {
                int x = -(A ? 1 : 0) + (D ? 1 : 0);
                int y = -(S ? 1 : 0) + (W ? 1 : 0);
                JoystickAngle = Mathf.Atan2(x, y) * 180f / Mathf.PI;
            }
            else
            {
                JoystickAngle = INVALID_ANGLE;
            }
        }
    #endif
    }
}