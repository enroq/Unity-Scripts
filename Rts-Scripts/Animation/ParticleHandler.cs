using UnityEngine;
using System.Collections.Generic;

#pragma warning disable
public class ParticleHandler : MonoBehaviour
{
    [SerializeField]
    private bool m_DebugMode;

    static List<BaseParticle> m_ParticleCache = new List<BaseParticle>();
    float deltaTime;

    BaseParticle[] m_UpdateCache;

    void Update()
    {
        deltaTime = Time.deltaTime;

        m_UpdateCache = m_ParticleCache.ToArray();
        for (int i = m_UpdateCache.Length - 1; i >= 0; i--)
        {
            if (m_UpdateCache[i] != null && m_UpdateCache[i].gameObject.activeInHierarchy)
                m_UpdateCache[i].ProcessParticleDecay(deltaTime);
            else
            {
                if (GameEngine.DebugMode && m_DebugMode)
                    Debug.Log(string.Format("Removing {0} From Particle Cache.", m_UpdateCache[i]));

                m_ParticleCache.Remove(m_UpdateCache[i]);
            }
        }
    }

    internal void AddParticleToCache(BaseParticle particle)
    {
        if (particle != null && !m_ParticleCache.Contains(particle))
            m_ParticleCache.Add(particle);
    }
}
