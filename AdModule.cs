using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdModule : MonoBehaviour 
{
    public enum AdRewardType
    {
        None,
        ExtraLives,
        DarkMatter
    }

    public enum AdErrorType
    {
        Generic,
        Failed,
        Unsupported
    }

    string[] m_ErrorMessages = new string[]
    {
        "There has been an error loading advertisement.",
        "Check internet connection and restart application.",
        "Advertisement is unsupported and failed to load."
    };

    [SerializeField]
    private string m_AndroidGameId = "";

    [SerializeField]
    private bool m_TestModeEnabled = false;

    [SerializeField]
    private string m_RewardVideoString = "rewardedVideo";

    float m_AdDisplayTimeOut = 5.0f;
    float m_CurrentDisplayTimeOut = 0.0f;
    float m_TimeOutInterval = 0.5f;

    bool m_HasTimedOut = false;

    ShowOptions m_ShowOptions;

    TitleSceneHandler m_CurrentTitleSceneHandler;

    void Start()
    {
        m_ShowOptions = new ShowOptions();
    }

    public void ShowAd(AdRewardType type, TitleSceneHandler handler)
    {
        m_CurrentTitleSceneHandler = handler;
        if (Advertisement.isSupported)
        {
            switch (type)
            {
                case AdRewardType.ExtraLives:
                    {
                        StartCoroutine(ShowRewardAd_Lives());
                        break;
                    }
                case AdRewardType.DarkMatter:
                    {
                        StartCoroutine(ShowRewardAd_DarkMatter());
                        break;
                    }
                case AdRewardType.None:
                    {
                        break;
                    }
                default: break;
            }
        }

        else DisplayAdError(AdErrorType.Unsupported);
    }

    private void DisplayAdError(AdErrorType type)
    {
        m_CurrentTitleSceneHandler.DisplayAdError(m_ErrorMessages[(int)type]);
    }

    private IEnumerator ShowRewardAd_Lives()
    {
        Advertisement.Initialize(m_AndroidGameId, m_TestModeEnabled);

        while (!Advertisement.IsReady(m_RewardVideoString) && !m_HasTimedOut)
        {
            yield return new WaitForSecondsRealtime(m_TimeOutInterval);
            if (QueryAdTimeOut())
            {
                RewardAdCallback_ExtraLives(ShowResult.Failed);
                m_HasTimedOut = true;
            }
        }

        if (!m_HasTimedOut)
        {
            m_ShowOptions.resultCallback = RewardAdCallback_ExtraLives;
            Advertisement.Show(m_RewardVideoString, m_ShowOptions);
        }

        m_HasTimedOut = false;
    }

    private IEnumerator ShowRewardAd_DarkMatter()
    {
        Advertisement.Initialize(m_AndroidGameId, m_TestModeEnabled);

        while (!Advertisement.IsReady(m_RewardVideoString) && !m_HasTimedOut)
        {
            yield return new WaitForSecondsRealtime(m_TimeOutInterval);
            if (QueryAdTimeOut())
            {
                RewardAdCallback_DarkMatter(ShowResult.Failed);
                m_HasTimedOut = true;
            }
        }

        if (!m_HasTimedOut)
        {
            m_ShowOptions.resultCallback = RewardAdCallback_DarkMatter;
            Advertisement.Show(m_RewardVideoString, m_ShowOptions);
        }

        m_HasTimedOut = false;
    }

    bool QueryAdTimeOut()
    {
        if (m_CurrentDisplayTimeOut < m_AdDisplayTimeOut)
            m_CurrentDisplayTimeOut += m_TimeOutInterval;

        if (m_CurrentDisplayTimeOut >= m_AdDisplayTimeOut)
        {
            m_CurrentDisplayTimeOut = 0;
            return true;
        }
        
        return false;
    }

    private void RewardAdCallback_ExtraLives(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Failed:
                {
                    DisplayAdError(AdErrorType.Failed);
                    break;
                }
            case ShowResult.Finished:
                {
                    m_CurrentTitleSceneHandler.RewardPlayerForAd_ExtraLives();
                    break;
                }
            case ShowResult.Skipped:
                {
                    Debug.Log("Reward Callback For Extra Lives: Skipped");
                    break;
                }
            default: break;
        }
    }

    private void RewardAdCallback_DarkMatter(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Failed:
                {
                    DisplayAdError(AdErrorType.Failed);
                    break;
                }
            case ShowResult.Finished:
                {
                    m_CurrentTitleSceneHandler.RewardPlayerForAd_DarkMatter();
                    break;
                }
            case ShowResult.Skipped:
                {
                    Debug.Log("Reward Callback For Dark Matter: Skipped");
                    break;
                }
            default: break;
        }
    }
}
