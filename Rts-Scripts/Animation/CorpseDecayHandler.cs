using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class CorpseDecayHandler : MonoBehaviour
{
    [SerializeField]
    private float m_DecayTimeInterval = 1.0f;

    Coroutine m_CorpseDecayRoutine;
    List<BaseCorpse> m_CorpseCache = new List<BaseCorpse>();

	void Start ()
    {
        m_CorpseDecayRoutine = StartCoroutine(CorpseDecayRoutine());
	}

    internal void AttachCorpse(BaseCorpse corpse)
    {
        if (corpse != null && !m_CorpseCache.Contains(corpse))
            m_CorpseCache.Add(corpse);
    }

    IEnumerator CorpseDecayRoutine()
    {
        yield return new 
            WaitForSecondsRealtime(m_DecayTimeInterval);

        ProcessCache();

        m_CorpseDecayRoutine = 
            StartCoroutine(CorpseDecayRoutine());
    }

    private void ProcessCache()
    {
        BaseCorpse[] corpses = m_CorpseCache.ToArray();
        for(int i = corpses.Length -1; i >= 0; i--)
        {
            if(corpses[i] != null)
                corpses[i].ProcessDecay(m_DecayTimeInterval);
        }

        for(int i = m_CorpseCache.Count -1; i >= 0; i--)
        {
            if (m_CorpseCache[i] == null)
                m_CorpseCache.RemoveAt(i);
        }
    }
}
