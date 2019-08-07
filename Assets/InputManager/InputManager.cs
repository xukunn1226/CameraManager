using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScreenRaycaster))]
[RequireComponent(typeof(DragRecognizer))]
[RequireComponent(typeof(PinchRecognizer))]
[RequireComponent(typeof(FingerUpDetector))]
public class InputManager : MonoBehaviour
{
    static public InputManager instance { get; private set; }

    public Vector2 DragDelta { get; private set; }
    public float PinchDelta { get; private set; }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        gameObject.GetComponent<FingerUpDetector>().OnFingerUp += OnFingerUp;
        gameObject.GetComponent<DragRecognizer>().OnGesture += OnDrag;
        gameObject.GetComponent<PinchRecognizer>().OnGesture += OnPinch;
    }

    private void OnFingerUp(FingerUpEvent e)
    {
        Debug.LogFormat("TimeHeldDown {0}   Pos {1}", e.TimeHeldDown, e.Position);
    }

    private void OnPinch(PinchGesture gesture)
    {
        switch (gesture.Phase)
        {
            case ContinuousGesturePhase.Started:
                {
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
                    PinchDelta = 0;
                }
                break;
        }
        Debug.LogFormat("OnPinch: {0}   {1}", gesture.Delta, gesture.Position);
    }

    private void OnDrag(DragGesture gesture)
    {
        switch(gesture.Phase)
        {
            case ContinuousGesturePhase.Started:
                DragDelta = Vector2.zero;
                break;
            case ContinuousGesturePhase.Updated:
                DragDelta = gesture.DeltaMove;
                break;
            case ContinuousGesturePhase.Ended:
            case ContinuousGesturePhase.None:
                DragDelta = Vector2.zero;
                break;
        }
        Debug.LogFormat("OnDrag: {0}    {1}", gesture.DeltaMove, gesture.Position);
    }
}
