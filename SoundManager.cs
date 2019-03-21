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
        [SerializeField]
        float m_VolumeModifier = 1.0f;

        internal string SoundId { get { return m_SoundId; } }
        internal AudioClip SoundData { get { return m_SoundData; } }
        internal float VolumeModifier { get { return m_VolumeModifier; } }
    }

    [SerializeField]
    private AudioClip m_BackgroundMusic;

    [SerializeField]
    AudioSource m_MainAudioSource;

    [SerializeField]
    AudioSource m_BackgroundAudioSource;

    [SerializeField]
    bool m_PlayBackgroundMusic = true;

    [SerializeField]
    List<Sound>
       m_Sounds = new List<Sound>();

    internal AudioSource MainAudioSource { get { return m_MainAudioSource; } }

    Dictionary<string, System.Tuple<AudioClip, float>> 
        m_AudioDictionary = new Dictionary<string, System.Tuple<AudioClip, float>>();

    void Awake()
    {
        if (m_MainAudioSource == null)
        {
            Debug.LogErrorFormat
                ("The Sound Manager On <{0}> Does Not Appear To Have A Sound Source; Adding Source..", gameObject.name);

            m_MainAudioSource = gameObject.AddComponent<AudioSource>();
        }

        for (int i = m_Sounds.Count - 1; i >= 0; i--)
        {
            if (!m_AudioDictionary.ContainsKey(m_Sounds[i].SoundId))
            {
                m_AudioDictionary.Add
                    (m_Sounds[i].SoundId, System.Tuple.Create(m_Sounds[i].SoundData, m_Sounds[i].VolumeModifier));
            }
        }
    }

    private void Start()
    {
        if (m_PlayBackgroundMusic)
            InitializeBackgroundMusic();
    }

    void InitializeBackgroundMusic()
    {
        if (m_BackgroundAudioSource != null && m_BackgroundMusic != null)
        {
            m_BackgroundAudioSource.loop = true;
            m_BackgroundAudioSource.clip = m_BackgroundMusic;
            m_BackgroundAudioSource.Play();
        }
    }

    internal bool ContainsSoundId(string id)
    {
        if (m_AudioDictionary.Count > 0)
            return m_AudioDictionary.ContainsKey(id);
        else
            return false;
    }

    public void PlaySound(string id)
    {
        if (m_AudioDictionary.ContainsKey(id))
        {
            m_MainAudioSource.clip = m_AudioDictionary[id].Item1;
            m_MainAudioSource.volume = m_AudioDictionary[id].Item2;

            m_MainAudioSource.Play();
        }

        else
            Debug.LogError("The Sound Manager Does Not Contain Corresponding Sound Identity..");
    }


    public void PlaySoundOnce(string id, float volumeMod = 1.0f)
    {
        if (m_AudioDictionary.ContainsKey(id))
            m_MainAudioSource.PlayOneShot
                (m_AudioDictionary[id].Item1, m_AudioDictionary[id].Item2 * volumeMod);

        else
            Debug.LogError("The Sound Manager Does Not Contain Corresponding Sound Identity..");
    }

    public void LoopSound(string id)
    {
        if (m_AudioDictionary.ContainsKey(id))
        {
            m_MainAudioSource.clip = m_AudioDictionary[id].Item1;
            m_MainAudioSource.volume = m_AudioDictionary[id].Item2;

            m_MainAudioSource.loop = true;
            m_MainAudioSource.Play();
        }

        else
            Debug.LogError("The Sound Manager Does Not Contain Corresponding Sound Identity..");
    }

    public void StopLoop()
    {
        m_MainAudioSource.Stop();

        m_MainAudioSource.clip = null;
        m_MainAudioSource.loop = false;
        m_MainAudioSource.volume = 1.0f;
    }
}
