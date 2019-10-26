using System.Collections.Generic;
using UnityEngine;

#pragma warning disable

public enum TechType
{
    None,
    Ability,
    AttackDamage,
    AttackRange,
    AttackSpeed,
    AttackTargets,
    CargoCapacity,
    Defense,
    Hitpoints,
    MovementSpeed,
    RangeOfSight,
    StealthDetectionChance,
    StealthDetectionRange
}

public class ResearchHandler : MonoBehaviour
{
    [SerializeField]
    List<string> m_TechStateEntities;

    private Dictionary<string, TechnologyState> 
        m_TechStates = new Dictionary<string, TechnologyState>();

    private TechnologyState m_DeltaState;
    private BaseTechnology m_DeltaTech;

    private BaseEntity[] m_EntityCache;

    private void Awake()
    {
        for(int i = m_TechStateEntities.Count -1; i >= 0; i--)
        {
            if (!m_TechStates.ContainsKey(m_TechStateEntities[i]))
                m_TechStates.Add(m_TechStateEntities[i], new TechnologyState(m_TechStateEntities[i]));
            else
                Debug.LogWarningFormat
                    ("Research Handler Attempted To Add Duplicate State [{0}]", m_TechStateEntities[i]);
        }
    }

    internal void AddTechToState(BaseTechnology tech)
    {
        if (m_TechStates.ContainsKey(tech.CorrespondingEntity))
            m_TechStates[tech.CorrespondingEntity].AddTechToState(tech);

        else Debug.LogWarning
                ("Research Handler Attempting To Add Tech To Non-Existant State.");
    }

    internal void UpgradeTechState(string entityName, string techName)
    {
        if(m_TechStates.ContainsKey(entityName))
        {
            m_DeltaState = m_TechStates[entityName];
            m_DeltaTech = m_DeltaState.GetTechByName(techName);

            if(m_DeltaTech != null && m_DeltaTech.CanBeUpgraded)
            {
                m_DeltaTech.UpgradeTech();
                ApplyTechToUnits(m_DeltaTech);
            }

            m_DeltaState = null; m_DeltaTech = null;
        }
    }

    internal void SyncTechState(BaseEntity entity)
    {
        if(m_TechStates.ContainsKey(entity.EntityName))
        {
            m_DeltaState = m_TechStates[entity.EntityName];
            for(int i = m_DeltaState.Technologies.Length -1; i >= 0; i--)
            {
                SyncTechWithEntity(m_DeltaState.Technologies[i], entity);
            }
        }
    }

    internal void ApplyTechToUnits(BaseTechnology tech)
    {
        m_EntityCache = GameEngine.PlayerStateHandler.GetControllingPlayer().CurrentUnits;
        for (int i = m_EntityCache.Length - 1; i >= 0; i--)
            tech.ApplyEffectToEntity(m_EntityCache[i]);
    }

    internal void SyncTechWithEntity(BaseTechnology tech, BaseEntity entity)
    {
        tech.SyncTechWithEntity(entity);
    }
}
