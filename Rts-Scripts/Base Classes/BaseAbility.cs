using System.Collections.Generic;
using UnityEngine;

#pragma warning disable

public enum AbilityEffectType
{
    None,
    DirectDamage,
    DirectHeal,
    DirectBuff,
    DirectDebuff,
    AreaDamage,
    AreaHeal,
    AreaBuff,
    AreaDebuff,
    Summon
}

public enum AbilityActivationType
{
    None,
    Instant,
    Delayed,
    Arrival,
    Interval,
    Passive,
    Duration,
    Channel
}

public enum AbilityTargetType
{
    None,
    Location,
    Entity,
    Chain
}

public enum AbilityTargetTeam
{
    None,
    Allies,
    Enemy
}

public class BaseAbility : MonoBehaviour
{
    [SerializeField]
    string m_AbilityName;
    [SerializeField]
    AbilityEffectType m_PrimaryEffectType;
    [SerializeField]
    List<AbilityEffectType> m_SecondaryEffectTypes;
    [SerializeField]
    AbilityActivationType m_ActivationType;
    [SerializeField]
    AbilityTargetType m_TargetType;
    [SerializeField]
    AbilityTargetTeam m_TargetTeam;
    [SerializeField]
    int m_EffectRating;
    [SerializeField]
    float m_AbilityCooldown;
    [SerializeField]
    float m_AbilityTargetRange;
    [SerializeField]
    Collider m_AreaEffectCollider;
    [SerializeField]
    float m_EffectInterval;
    [SerializeField]
    float m_EffectDuration;
    [SerializeField]
    float m_CastDuration;
    [SerializeField]
    float m_EffectDelay;
    [SerializeField]
    int m_AbilityCost;
    [SerializeField]
    GameObject m_EffectObject;
    [SerializeField]
    Vector3 m_EffectOffset = Vector3.zero;
    [SerializeField]
    int m_AnimationIndex;
    [SerializeField]
    AudioSource m_AbilitySound;

    List<GameObject> m_ObjectsInRange = new List<GameObject>();

    private void Start()
    {
        m_AreaEffectCollider.transform.parent = null;
    }

    internal void AddObjectInRange(GameObject obj)
    {
        if (!m_ObjectsInRange.Contains(obj) && obj.GetComponent<BaseEntity>())
        {
            m_ObjectsInRange.Add(obj);
            if(GameEngine.DebugMode)
                Debug.LogFormat("Adding {0} To Area Of Effect..", obj);
        }
    }

    internal void RemoveObjectInRange(GameObject obj)
    {
        if (m_ObjectsInRange.Contains(obj))
            m_ObjectsInRange.Remove(obj);
    }

    internal GameObject[] ObjectsInRange
    {
        get { return m_ObjectsInRange.ToArray(); }
    }

    public string AbilityName
    {
        get { return m_AbilityName; }
    }

    public AbilityEffectType PrimaryEffect
    {
        get { return m_PrimaryEffectType; }
    }

    public List<AbilityEffectType> SecondaryEffects
    {
        get { return m_SecondaryEffectTypes; }
    }

    public AbilityActivationType ActivationType
    {
        get { return m_ActivationType; }
    }

    public AbilityTargetType TargetType
    {
        get { return m_TargetType; }   
    }

    public int EffectRating
    {
        get { return m_EffectRating; }
    }

    public float AbilityCooldown
    {
        get { return m_AbilityCooldown; }
    }

    public float EffectInterval
    {
        get { return m_EffectInterval; }
    }

    public float EffectDuration
    {
        get { return m_EffectDuration; }
    }

    public float CastDuration
    {
        get { return m_CastDuration; }
    }

    public float EffectDelay
    {
        get { return m_EffectDelay; }
    }

    public int AbilityCost
    {
        get { return m_AbilityCost; }
    }

    public GameObject EffectObject
    {
        get { return m_EffectObject; }
    }

    public Vector3 EffectOffset
    {
        get { return m_EffectOffset; }
    }

    public int AnimationIndex
    {
        get { return m_AnimationIndex; }
    }

    public Collider AreaEffectCollider
    {
        get { return m_AreaEffectCollider; }
    }

    public AudioSource AbilitySound
    {
        get { return m_AbilitySound; }
    }

    public AbilityTargetTeam TargetTeam
    {
        get { return m_TargetTeam; }
    }

    public float AbilityTargetRange
    {
        get { return m_AbilityTargetRange; }
    }

    internal void EnableAreaEffectCollider()
    {
        m_ObjectsInRange.Clear();
        m_AreaEffectCollider.gameObject.SetActive(true);
    }

    internal void DisableAreaEffectCollider()
    {
        m_AreaEffectCollider.gameObject.SetActive(false);
        m_EffectObject.transform.position = gameObject.transform.position;
    }
}
