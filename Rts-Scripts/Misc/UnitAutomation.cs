using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAutomation : MonoBehaviour
{
    [SerializeField]
    int m_AutoButtonIndex = 0;
    [SerializeField]
    float m_InvokeTime = 5.0f;

    ActionButtonState m_TargetButtonState;
	void Start ()
    {
        if ((m_TargetButtonState = gameObject.GetComponentInChildren<ActionButtonState>()) == null)
            throw new MissingComponentException("Object With Unit Automation Behavior Missing Action Button State.");

        InvokeRepeating("InvokeButtonIndex", m_InvokeTime, m_InvokeTime);
	}

    void InvokeButtonIndex()
    {
        m_TargetButtonState.InvokeButtonAtIndex(m_AutoButtonIndex);
    }
}
