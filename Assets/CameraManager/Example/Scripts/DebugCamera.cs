using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DebugCamera : MonoBehaviour
{
    public GameObject m_Player;
    public CameraViewInfoCollection m_Collection;

    private bool m_isAiming;

    private void Start()
    {
        CameraManager.instance.InitViewInfoCollection(m_Player.transform, m_Collection);

        CameraManager.instance.OnPostUpdate += UpdatePlayerRotation;
    }

    private void OnDestroy()
    {
        if(CameraManager.instance != null)
            CameraManager.instance.OnPostUpdate -= UpdatePlayerRotation;
    }

    void UpdatePlayerRotation()
    {
        if(m_Player != null)
        {
            Vector3 eulerAngles = CameraManager.instance.eulerAngles;
            eulerAngles.x = 0;
            m_Player.transform.eulerAngles = eulerAngles;
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

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Walk, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Run, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Sprint, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Squat, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Roll, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Jump, m_isAiming);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CameraManager.instance.SetCharacterView(CameraManager.CharacterView.Fly, m_isAiming);
        }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return))
        {
            m_isAiming = !m_isAiming;
            Debug.LogFormat("m_isAiming: {0}", m_isAiming);
        }
    }
}
