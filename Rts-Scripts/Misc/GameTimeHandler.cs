using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class GameTimeHandler : MonoBehaviour
{
    [SerializeField]
    private Text m_GameTimerDisplay;

    int m_GameSeconds;
    TimeSpan m_GameTime;
    Coroutine m_GameTimerRoutine;

    private void Start()
    {
        m_GameTimerRoutine = StartCoroutine(ProcessGameTime());
    }

    IEnumerator ProcessGameTime()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        m_GameSeconds += 1;
        m_GameTime = TimeSpan.FromSeconds(m_GameSeconds);

        if (m_GameTimerDisplay != null)
            m_GameTimerDisplay.text = m_GameTime.ToString();

        m_GameTimerRoutine = StartCoroutine(ProcessGameTime());
    }
}
