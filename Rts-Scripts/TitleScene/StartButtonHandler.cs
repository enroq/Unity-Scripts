using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonHandler : MonoBehaviour
{
    public enum TransitionState
    {
        Addative,
        Regressive
    }

    public float m_CurrentInstructionalTextAlpha = 1.0f;

    public TransitionState m_CurrentAlphaState = TransitionState.Regressive;

    public Color m_DeltaColor;

    public UnityEngine.UI.Text m_StartText;

    // Use this for initialization
    void Start ()
    {
        InvokeRepeating("ShiftOpacity", 0.1f, 0.1f);
	}

    public void OnStartClick()
    {
        CancelInvoke();

        m_DeltaColor.a = 1.0f;
        m_StartText.color = m_DeltaColor;

        m_StartText.text = "Loading..";

        SceneManager.LoadSceneAsync(1);
    }

    void ShiftOpacity()
    {
        if (m_CurrentAlphaState == TransitionState.Addative)
        {
            m_CurrentInstructionalTextAlpha += 0.1f;
            m_DeltaColor.a = m_CurrentInstructionalTextAlpha;

            m_StartText.color = m_DeltaColor;

            if (m_CurrentInstructionalTextAlpha >= 1.0f)
                m_CurrentAlphaState = TransitionState.Regressive;
        }

        else if (m_CurrentAlphaState == TransitionState.Regressive)
        {
            m_CurrentInstructionalTextAlpha -= 0.1f;
            m_DeltaColor.a = m_CurrentInstructionalTextAlpha;

            m_StartText.color = m_DeltaColor;

            if (m_CurrentInstructionalTextAlpha <= 0.0f)
                m_CurrentAlphaState = TransitionState.Addative;
        }
    }
}
