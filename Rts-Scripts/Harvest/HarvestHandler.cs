using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public enum ResourceType
{
    None,
    Crystal,
}

public class HarvestHandler : MonoBehaviour
{
    [SerializeField]
    float m_IterationInterval = 0.1f;
    [SerializeField]
    int m_MaxWorkersPerNode = 5;
    [SerializeField]
    string m_ResourceLayerName;

    List<HarvesterState> m_HarvesterStateCache = new List<HarvesterState>();

    HarvesterState[] m_ProcessCache;
    HarvesterState[] m_UpdateCache;

    internal LayerMask ResourceLayer { get { return LayerMask.NameToLayer(m_ResourceLayerName); } }

    float m_TimeDelta;

    void Start()
    {
        InvokeRepeating("ProcessHarvesterStates", m_IterationInterval, m_IterationInterval);
    }

    void Update()
    {
        m_TimeDelta = Time.deltaTime;
        m_UpdateCache = m_HarvesterStateCache.ToArray();
        for (int i = m_UpdateCache.Length - 1; i >= 0; i--)
        {
            if (m_UpdateCache[i] != null)
                m_UpdateCache[i].UpdateHarvesterState(m_TimeDelta);

            else
                m_HarvesterStateCache.Remove(m_UpdateCache[i]);
        }
    }

    void ProcessHarvesterStates()
    {
        m_ProcessCache = m_HarvesterStateCache.ToArray();
        for (int i = m_ProcessCache.Length - 1; i >= 0; i--)
        {
            if (m_ProcessCache[i] != null)
                m_ProcessCache[i].ProcessHarvesterState(m_IterationInterval);

            else
                m_HarvesterStateCache.Remove(m_ProcessCache[i]);
        }
    }

    internal void AttachState(HarvesterState state)
    {
        if (!m_HarvesterStateCache.Contains(state))
            m_HarvesterStateCache.Add(state);
    }
}
