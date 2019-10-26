using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAreaController : MonoBehaviour {

    [SerializeField]
    BaseAbility m_ParentAbility;

    void Start()
    {

        if(m_ParentAbility == null)
            throw new UnityException(string.Format("Ability Area Controller Attached To Objects ({0}) Without Ability", gameObject));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (m_ParentAbility != null)
            m_ParentAbility.AddObjectInRange(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        if (m_ParentAbility != null)
            m_ParentAbility.RemoveObjectInRange(other.gameObject);
    }
}
