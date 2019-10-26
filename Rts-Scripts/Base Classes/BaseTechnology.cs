using UnityEngine;

#pragma warning disable
public class BaseTechnology : MonoBehaviour
{
    [SerializeField]
    string m_CorrespondingEntity;
    [SerializeField]
    string m_TechName;
    [SerializeField]
    TechType m_TechType;
    [SerializeField]
    int m_MaxLevel;
    [SerializeField]
    int m_EffectRating;
    [SerializeField]
    int m_BaseResearchTime;

    private int m_CurrentLevel;
    private ICombatant m_CombatantCache;

    internal bool IsBeingResearched { get; set; }

    internal int CurrentLevel
    {
        get { return m_CurrentLevel; }
    }

    internal string TechName
    {
        get { return m_TechName; }
    }

    internal string CorrespondingEntity
    {
        get { return m_CorrespondingEntity; }
    }

    internal int ResearchTime
    {
        get
        {
            return m_BaseResearchTime * (m_CurrentLevel + 1);
        }
    }

    private void Start()
    {
        GameEngine.ResearchHandler.AddTechToState(this);
    }

    internal bool CanBeUpgraded
    {
        get { return (m_CurrentLevel + 1 <= m_MaxLevel); }
    }

    internal void UpgradeTech()
    {
        if (CanBeUpgraded)
            m_CurrentLevel++;
    }

    internal void ApplyEffectToEntity(BaseEntity entity)
    {
        switch(m_TechType)
        {
            case TechType.AttackDamage:
                {
                    ApplyDamageUpgrade(entity);
                    break;
                }

            default:break;
        }
    }

    internal void SyncTechWithEntity(BaseEntity entity)
    {
        switch (m_TechType)
        {
            case TechType.AttackDamage:
                {
                    SyncDamageUpgrade(entity);
                    break;
                }

            default: break;
        }
    }

    internal void ApplyDamageUpgrade(BaseEntity entity)
    {
        if((m_CombatantCache = entity.gameObject.GetComponent<ICombatant>()) != null)
        {
            m_CombatantCache.AttackRating += m_EffectRating;
        }
    }

    internal void SyncDamageUpgrade(BaseEntity entity)
    {
        if ((m_CombatantCache = entity.gameObject.GetComponent<ICombatant>()) != null)
        {
            m_CombatantCache.AttackRating += m_EffectRating * m_CurrentLevel;
        }
    }
}
