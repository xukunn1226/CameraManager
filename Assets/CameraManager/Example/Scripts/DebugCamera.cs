using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DebugCamera : MonoBehaviour
{
    public GameObject m_Player;
    public CameraViewInfoCollection m_Collection;

    private bool m_isAiming;
    private bool m_isWatching;

    private void Start()
    {
        CameraManager.instance.InitViewInfoCollection(m_Player.transform, m_Collection);

        InputManager.instance.gameObject.GetComponent<DragRecognizer>().OnGesture += OnDrag;

        CameraManager.instance.OnPostUpdate += UpdatePlayerRotation;
    }

    private void OnDestroy()
    {
        if(CameraManager.instance != null)
            CameraManager.instance.OnPostUpdate -= UpdatePlayerRotation;
    }

    void UpdatePlayerRotation(bool isWatching)
    {
        if(m_Player != null && !isWatching)
        {
            Vector3 eulerAngles = CameraManager.instance.eulerAngles;
            eulerAngles.x = 0;
            m_Player.transform.eulerAngles = eulerAngles;
        }
    }

    private void OnDrag(DragGesture gesture)
    {
        if(!m_isWatching)
        {
            return;
        }

        switch(gesture.Phase)
        {
            case ContinuousGesturePhase.Started:
            case ContinuousGesturePhase.Updated:
                CameraManager.instance.ProcessInputWhenWatching(gesture.DeltaMove);
                break;
            case ContinuousGesturePhase.Ended:
                CameraManager.instance.EndWatching(0.2f);
                break;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(m_Player != null)
        {
            // update position
            Vector3 dir = Vector3.zero;
            if (InputManager.instance.JoystickXYUse)
            {
                dir += InputManager.instance.JoystickXYAngle.y * m_Player.transform.forward;
                dir += InputManager.instance.JoystickXYAngle.x * m_Player.transform.right;
            }

            m_Player.transform.position += dir * 5 * Time.deltaTime;
        }
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width - 240, 100, 80, 80), "Watching"))
        {
            m_isWatching = true;
            CameraManager.instance.BeginWatching();
            Debug.LogFormat("m_isWatching: {0}", m_isWatching);
        }

        if(GUI.Button(new Rect(Screen.width - 120, 260, 80, 80), "Aiming"))
        {
            m_isAiming = !m_isAiming;
            Debug.LogFormat("m_isAiming: {0}", m_isAiming);
        }
    }
    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Walk, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Run, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Sprint, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Squat, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Roll, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Jump, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CameraManager.instance.SetCharacterView(CameraViewInfoCollection.CharacterView.Fly, m_isAiming);
        }
    }
}
