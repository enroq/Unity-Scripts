using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum FadeState
{
    None,
    Increase,
    Decrease
}

public class FadeUIAlpha : MonoBehaviour
{
    [SerializeField]
    Image m_TargetImage;

    [SerializeField]
    FadeState m_StartingState = FadeState.Increase;

    [SerializeField]
    float m_FadeSpeed = 1.0f;

    [SerializeField]
    float m_MinimumAlpha = 0.1f;

    [SerializeField]
    float m_MaximumAlpha = 0.75f;

    Color m_DeltaColor;

    FadeState m_FadeState;

    private void Start()
    {
        if(m_TargetImage != null)
            m_DeltaColor = m_TargetImage.color;

        m_FadeState = m_StartingState;
    }

    private void FixedUpdate()
    {
        if (m_FadeState == FadeState.None)
            return;

        if (m_TargetImage != null)
        {
            if(m_FadeState == FadeState.Increase)
            {
                if (m_TargetImage.color.a < m_MaximumAlpha)
                {
                    m_DeltaColor.a += Time.deltaTime * m_FadeSpeed;
                    m_TargetImage.color = m_DeltaColor;
                }

                else
                    m_FadeState = FadeState.Decrease;
            }

            else if (m_FadeState == FadeState.Decrease)
            {
                if (m_TargetImage.color.a > m_MinimumAlpha)
                {
                    m_DeltaColor.a -= Time.deltaTime * m_FadeSpeed;
                    m_TargetImage.color = m_DeltaColor;
                }

                else
                    m_FadeState = FadeState.Increase;
            }
        }
    }

}
