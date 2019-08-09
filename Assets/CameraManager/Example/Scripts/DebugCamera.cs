using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DebugCamera : MonoBehaviour
{
    public GameObject m_Player;
    public CameraViewInfoCollection m_Collection;

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
        if(Input.GetKey(KeyCode.Alpha1))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Walk);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Run);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Sprint);
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Squat);
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Roll);
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Jump);
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            CameraManager.instance.ChangeConventionalViewInfo(CameraManager.ConventionalView.Fly);
        }
    }
}
