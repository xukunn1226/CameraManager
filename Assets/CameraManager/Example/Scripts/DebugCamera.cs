using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DebugCamera : MonoBehaviour
{
    public GameObject m_Player;
    public float m_Step;

    // Update is called once per frame
    void Update()
    {
        if(m_Player != null)
        {
            m_Player.transform.position += GetMoveForward() * m_Step * Time.deltaTime;
        }
    }

    /// <summary>
    /// 获得角色移动朝向（世界坐标，已归一化）
    /// </summary>
    /// <returns></returns>
    Vector3 GetMoveForward()
    {
        //float angle = Camera.eulerAngles.y;
        float angle = InputManager.instance.JoystickAngle * Mathf.Deg2Rad;

        return InputManager.instance.JoystickUse ? new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) : Vector3.zero;      // normalized
    }
}
