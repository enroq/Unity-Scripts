using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityState : MonoBehaviour
{
    [SerializeField]
    List<BaseAbility> m_Abilities;
    [SerializeField]
    bool m_AutoPopulateAbilities = true;

    BaseEntity m_EntityRelative;

    internal BaseEntity EntityRelative
    {
        get { return m_EntityRelative; }
    }

    internal void Start()
    {
        if ((m_EntityRelative = gameObject.GetComponent<BaseEntity>()) == null)
            throw new MissingComponentException("Ability State Attached To Object Without Entity Component.");
        if(m_AutoPopulateAbilities)
        {
            foreach(BaseAbility ability in GetComponentsInChildren<BaseAbility>())
                m_Abilities.Add(ability);
        }
    }

    internal void ProcessPassiveAbilities()
    {
        for(int i = m_Abilities.Count -1; i >= 0; i--)
        {
            if (m_Abilities[i].ActivationType == AbilityActivationType.Passive)
                ProcessPassiveAbility(m_Abilities[i]);
        }
    }

    internal void InitializeActiveAbility(BaseAbility ability)
    {
        GameEngine.AbilityTargetHandler.SetAbilityQueue(ability, this);
    }

    internal void ActivateAbility(BaseAbility ability, Vector3 target)
    {
        switch (ability.ActivationType)
        {
            case AbilityActivationType.Instant:
                {
                    ProcessInstantAbility(ability, target);
                    break;
                }
            case AbilityActivationType.Delayed:
                {
                    ProcessDelayedAbility(ability, target);
                    break;
                }
            case AbilityActivationType.Interval:
                {
                    break;
                }
            case AbilityActivationType.Duration:
                {
                    break;
                }
            default:break;
        }
    }

    internal void ActivateAbility(BaseAbility ability, BaseEntity target)
    {
        switch (ability.ActivationType)
        {
            case AbilityActivationType.Instant:
                {
                    ProcessInstantAbility(ability, target);
                    break;
                }
            case AbilityActivationType.Delayed:
                {
                    ProcessDelayedAbility(ability, target);
                    break;
                }
            case AbilityActivationType.Interval:
                {
                    break;
                }
            case AbilityActivationType.Duration:
                {
                    break;
                }
            default: break;
        }
    }

    internal void ProcessPassiveAbility(BaseAbility ability)
    {

    }

    internal void ProcessInstantAbility(BaseAbility ability, BaseEntity target)
    {
        switch (ability.PrimaryEffect)
        {
            case AbilityEffectType.None:
                {
                    if (GameEngine.DebugMode)
                        throw new UnityException(string.Format
                            ("{0}'s Ability State Attempting To Process Effect Type Of None.", gameObject));
                    break;
                }
            case AbilityEffectType.DirectDamage:
                {
                    InvokeInstantDirectDamage(ability, target);
                    break;
                }
            case AbilityEffectType.DirectHeal:
                {
                    break;
                }
            case AbilityEffectType.DirectBuff:
                {
                    break;
                }
            case AbilityEffectType.DirectDebuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDamage:
                {
                    break;
                }
            case AbilityEffectType.AreaHeal:
                {
                    break;
                }
            case AbilityEffectType.AreaBuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDebuff:
                {
                    break;
                }
            default: goto case AbilityEffectType.None;
        }
    }

    internal void ProcessInstantAbility(BaseAbility ability, Vector3 target)
    {
        switch (ability.PrimaryEffect)
        {
            case AbilityEffectType.None:
                {
                    if (GameEngine.DebugMode)
                        throw new UnityException(string.Format
                            ("{0}'s Ability State Attempting To Process Effect Type Of None.", gameObject));
                    break;
                }
            case AbilityEffectType.AreaDamage:
                {
                    break;
                }
            case AbilityEffectType.AreaHeal:
                {
                    break;
                }
            case AbilityEffectType.AreaBuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDebuff:
                {
                    break;
                }
            default: goto case AbilityEffectType.None;
        }
    }

    internal void ProcessDelayedAbility(BaseAbility ability, Vector3 target)
    {
        switch (ability.PrimaryEffect)
        {
            case AbilityEffectType.None:
                {
                    if (GameEngine.DebugMode)
                        throw new UnityException(string.Format
                            ("{0}'s Ability State Attempting To Process Effect Type Of None.", gameObject));
                    break;
                }
            case AbilityEffectType.AreaDamage:
                {
                    InvokeDelayedAreaDamage(ability, target);
                    break;
                }
            case AbilityEffectType.AreaHeal:
                {
                    break;
                }
            case AbilityEffectType.AreaBuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDebuff:
                {
                    break;
                }
            default: goto case AbilityEffectType.None;
        }
    }

    internal void ProcessDelayedAbility(BaseAbility ability, BaseEntity target)
    {
        switch (ability.PrimaryEffect)
        {
            case AbilityEffectType.None:
                {
                    if (GameEngine.DebugMode)
                        throw new UnityException(string.Format
                            ("{0}'s Ability State Attempting To Process Effect Type Of None.", gameObject));
                    break;
                }
            case AbilityEffectType.DirectDamage:
                {
                    InvokeDelayedDirectDamage(ability, target);
                    break;
                }
            case AbilityEffectType.DirectHeal:
                {
                    break;
                }
            case AbilityEffectType.DirectBuff:
                {
                    break;
                }
            case AbilityEffectType.DirectDebuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDamage:
                {
                    break;
                }
            case AbilityEffectType.AreaHeal:
                {
                    break;
                }
            case AbilityEffectType.AreaBuff:
                {
                    break;
                }
            case AbilityEffectType.AreaDebuff:
                {
                    break;
                }
            default: goto case AbilityEffectType.None;
        }
    }

    internal void InvokeInstantDirectDamage(BaseAbility ability, BaseEntity entity)
    {
        if (entity != null)
        {
            entity.OnDamage(ability.EffectRating);
            GameEngine.ObjectPoolHandler.ExtractObject(ability.EffectObject, entity.Origin);
            m_EntityRelative.PlayAbilityAnimation(ability.AnimationIndex);
        }
    }

    internal void InvokeDelayedDirectDamage(BaseAbility ability, BaseEntity entity)
    {
        m_EntityRelative.PlayAbilityAnimation(ability.AnimationIndex);
        StartCoroutine(DelayDirectDamage(ability, entity));
    }

    IEnumerator DelayDirectDamage(BaseAbility ability, BaseEntity entity)
    {
        yield return new WaitForSeconds(ability.EffectDelay);

        if (entity != null)
        {
            entity.OnDamage(ability.EffectRating);
            GameEngine.ObjectPoolHandler.ExtractObject(ability.EffectObject, entity.Origin);
        }
    }

    internal void InvokeDelayedAreaDamage(BaseAbility ability, Vector3 target)
    {
        m_EntityRelative.PlayAbilityAnimation(ability.AnimationIndex);
        ability.AreaEffectCollider.transform.position = target;
        ability.EnableAreaEffectCollider();

        GameEngine.ObjectPoolHandler.ExtractObject(ability.EffectObject, target + ability.EffectOffset);

        StartCoroutine(DelayAreaDamage(ability, target));
    }

    IEnumerator DelayAreaDamage(BaseAbility ability, Vector3 target)
    {
        yield return new WaitForSeconds(ability.EffectDelay);

        for(int i = ability.ObjectsInRange.Length -1; i >= 0; i--)
        {
            if (ability.ObjectsInRange[i] != null && !GameEngine.PlayerStateHandler.IsTeamMember(ability.ObjectsInRange[i])) 
                ability.ObjectsInRange[i].GetComponent<BaseEntity>().OnDamage(ability.EffectRating);
        }

        ability.DisableAreaEffectCollider();
    }
}