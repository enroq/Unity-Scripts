using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    internal class Sound
    {
        [SerializeField]
        string m_SoundId;
        [SerializeField]
        AudioClip m_SoundData;

        internal string SoundId { get { return m_SoundId; } }
        internal AudioClip SoundData { get { return m_SoundData; } }
    }

    [SerializeField]
    AudioSource m_Source;

    [SerializeField]
    List<Sound>
       m_Sounds = new List<Sound>();

    private SoundManager m_Instance;
    public SoundManager Instance
    {
        get { return m_Instance; }
    }

    internal AudioSource Source { get { return m_Source; } }

    Dictionary<string, AudioClip> 
        m_AudioDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (m_Instance != null)
            m_Instance = this;
        else
            throw new UnityException("You can not have more than one sound manager in the scene at a time.");

        if(m_Source == null)
        {
            Debug.LogErrorFormat("The Sound Manager Does Not Appear To Have A Sound Source");
        }

        for(int i = m_Sounds.Count; i >= 0; i--)
        {
            if(!m_AudioDictionary.ContainsKey(m_Sounds[i].SoundId))
            {
                m_AudioDictionary.Add
                    (m_Sounds[i].SoundId, m_Sounds[i].SoundData);
            }
        }
    }

    public void PlaySound(string id)
    {
        if (m_AudioDictionary.ContainsKey(id))
            m_Source.PlayOneShot(m_AudioDictionary[id]);

        else
            Debug.LogError("The Sound Manager Does Not Contain Corresponding Sound Identity..");
    }

    /// <summary>
    /// Usage Would Go Something Like.. SoundManager.Instance.PlaySound("id");
    /// </summary>
    public void PlaySound(string id, float volume)
    {
        if (m_AudioDictionary.ContainsKey(id))
            m_Source.PlayOneShot(m_AudioDictionary[id], volum);

        else
            Debug.LogError("The Sound Manager Does Not Contain Corresponding Sound Identity..");
    }
}
