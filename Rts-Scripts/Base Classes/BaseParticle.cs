using UnityEngine;

public class BaseParticle : MonoBehaviour, IPoolable
{
    public int ParentInstanceId { get; set; }

    [SerializeField]
    private float m_DecayTime = 1.0f;

    private float m_CurrentDecayTime;

    internal void ProcessParticleDecay(float deltaTime)
    {
        if (m_CurrentDecayTime > 0)
            m_CurrentDecayTime -= deltaTime;

        if(m_CurrentDecayTime <= 0)
            GameEngine.ObjectPoolHandler.ReclaimObject(ParentInstanceId, gameObject);
    }

    public void OnExtraction()
    {
        m_CurrentDecayTime = m_DecayTime; 
        GameEngine.ParticleHandler.AddParticleToCache(this);
    }
}
