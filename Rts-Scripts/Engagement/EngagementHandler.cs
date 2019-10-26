using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable
public class EngagementHandler : MonoBehaviour
{
    [SerializeField]
    float m_IterationInterval = 0.1f;

    List<ICombatant> m_CombatStateCache = new List<ICombatant>();

    ICombatant[] m_ProcessCache;
    ICombatant[] m_UpdateCache;

    float m_TimeDelta;

    void Start()
    {
        InvokeRepeating("ProcessCombatStates", m_IterationInterval, m_IterationInterval);
    }

    private void Update()
    {
        m_UpdateCache = m_CombatStateCache.ToArray();
        m_TimeDelta = Time.deltaTime;

        for (int i = m_UpdateCache.Length - 1; i >= 0; i--)
        {
            if (m_UpdateCache[i] != null)
            {
                if(!(m_UpdateCache[i] is BuildingCombatState))
                    m_UpdateCache[i].UpdateCombatState(m_TimeDelta);
            }

            else
                m_CombatStateCache.Remove(m_UpdateCache[i]);
        }
    }

    void ProcessCombatStates()
    {
        m_ProcessCache = m_CombatStateCache.ToArray();

        for (int i = m_ProcessCache.Length - 1; i >= 0; i--)
        {
            if (m_ProcessCache[i] != null)
                m_ProcessCache[i].ProcessCombatState(m_IterationInterval);

            else
                m_CombatStateCache.Remove(m_ProcessCache[i]);
        }
    }

    internal void AttachCombatState(ICombatant combatant)
    {
        if (!m_CombatStateCache.Contains(combatant) 
            && combatant.AttackType != CombatType.None)
                m_CombatStateCache.Add(combatant);
    }

    internal void DetatchCombatState(ICombatant combatant)
    {
        if (m_CombatStateCache.Contains(combatant))
            m_CombatStateCache.Remove(combatant);
    }
}
