using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingSwitch : MonoBehaviour
{
    [SerializeField]
    GameObject m_ButtonObject;
    [SerializeField]
    Vector3 m_RotationAxis = Vector3.forward;
    [SerializeField]
    float m_OffRotation = 90f;
    [SerializeField]
    float m_OnRotation = 0f;
    [SerializeField]
    float m_RotationSpeed = 3f;
    [SerializeField]
    bool m_StartInOnPosition = true;
    
    /// <summary>
    /// Object that will be activated or deactivated depending on switch position.
    /// </summary>
    [SerializeField]
    GameObject m_TargetObject;

    bool m_On;

    Coroutine m_RotationRoutine;

    RectTransform m_ButtonTransform;

    private void Start()
    {
        if ((m_ButtonTransform = gameObject.GetComponent<RectTransform>()) == null)
        {
            Debug.LogAssertionFormat("{0} Is Attached To An Object ({1}) With No RectTransform Component..", name, gameObject);
        }

        if (m_StartInOnPosition)
            m_On = true;
    }

    public void Switch()
    {
        if (m_RotationRoutine != null)
            StopCoroutine(m_RotationRoutine);

        if(m_On)
        {
            m_On = false;
            m_TargetObject.SetActive(false);
            m_RotationRoutine = StartCoroutine
                (RotateButton(m_OffRotation * m_RotationAxis));
        }

        else if (!m_On)
        {
            m_On = true;
            m_TargetObject.SetActive(true);
            m_RotationRoutine = StartCoroutine
                (RotateButton(m_OnRotation * m_RotationAxis));
        }
    }

    IEnumerator RotateButton(Vector3 rotation)
    {
        yield return new WaitForEndOfFrame();

        m_ButtonTransform.rotation = Quaternion.Euler(Vector3.SlerpUnclamped
        (
            transform.rotation.eulerAngles,
            rotation,
            (Time.smoothDeltaTime * m_RotationSpeed) / Mathf.PI
        ));

        if (m_ButtonTransform.rotation.eulerAngles != rotation)
            m_RotationRoutine = StartCoroutine(RotateButton(rotation));
    }
}
