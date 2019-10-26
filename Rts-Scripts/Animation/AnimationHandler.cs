using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class AnimationHandler : MonoBehaviour
{
    Animator m_Animator;

    [SerializeField]
    private string m_WalkStateParameter = "IsWalking";

    [SerializeField]
    private bool m_HasAttackAnimation;
    [SerializeField]
    private bool m_HasIdleAnimation;
    [SerializeField]
    private bool m_HasWalkAnimation;
    [SerializeField]
    private bool m_HasDeathAnimation;
    [SerializeField]
    private bool m_HasHarvestAnimation;
    [SerializeField]
    private bool m_HasAbilityAnimation;

    [SerializeField]
    private string m_AttackAnimationName;
    [SerializeField]
    private string m_IdleAnimationName;
    [SerializeField]
    private string m_WalkAnimationName;
    [SerializeField]
    private string m_DeathAnimationName;
    [SerializeField]
    private string m_HarvestAnimationName;

    [SerializeField]
    private List<string> m_AbilityAnimationNames;

    private int m_AttackAnimationHash;
    private int m_IdleAnimationHash;
    private int m_WalkAnimationHash;
    private int m_DeathAnimationHash;
    private int m_HarvestAnimationHash;

    private List<int> m_AbilityAnimationHashes = new List<int>();

    private int m_IsWalkingHash;

	void Start ()
    {
        if ((m_Animator = gameObject.GetComponent<Animator>()) == null)
            throw new MissingComponentException("Animation Handler Missing Animator Component.");

        if (string.IsNullOrEmpty(m_AttackAnimationName) && m_HasAttackAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Attack Animation Name.", gameObject));
        if (string.IsNullOrEmpty(m_IdleAnimationName) && m_HasIdleAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Idle Animation Name.", gameObject));
        if (string.IsNullOrEmpty(m_WalkAnimationName) && m_HasWalkAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Walk Animation Name.", gameObject));
        if (string.IsNullOrEmpty(m_DeathAnimationName) && m_HasDeathAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Death Animation Name.", gameObject));
        if (string.IsNullOrEmpty(m_WalkStateParameter) && m_HasWalkAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Walk State Parameter.", gameObject));
        if (string.IsNullOrEmpty(m_HarvestAnimationName) && m_HasHarvestAnimation)
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Harvest Animation Name.", gameObject));

        if(m_HasAbilityAnimation && !(m_AbilityAnimationNames.Count > 0))
            throw new UnityException(string.Format("{0}'s Animation Handler Is Missing Ability Animation Name(s).", gameObject));

        if (m_HasAttackAnimation)
            m_AttackAnimationHash = Animator.StringToHash(m_AttackAnimationName);

        if(m_HasIdleAnimation)
            m_IdleAnimationHash = Animator.StringToHash(m_IdleAnimationName);

        if(m_HasWalkAnimation)
            m_WalkAnimationHash = Animator.StringToHash(m_WalkAnimationName);

        if(m_HasDeathAnimation)
            m_DeathAnimationHash = Animator.StringToHash(m_DeathAnimationName);

        if (m_HasHarvestAnimation)
            m_HarvestAnimationHash = Animator.StringToHash(m_HarvestAnimationName);

        if (m_HasAbilityAnimation)
        {
            for (int i = 0; i < m_AbilityAnimationNames.Count; i++)
                m_AbilityAnimationHashes.Add(Animator.StringToHash(m_AbilityAnimationNames[i]));
        }

        if(m_HasWalkAnimation)
            m_IsWalkingHash = Animator.StringToHash(m_WalkStateParameter);
    }

    internal void PlayAttackAnimation()
    {
        if(m_HasAttackAnimation)
            m_Animator.Play(m_AttackAnimationHash, 0);
    }

    internal void PlayDeathAnimation()
    {
        if(m_HasDeathAnimation)
            m_Animator.Play(m_DeathAnimationHash, 0);
    }

    internal void PlayHarvestAnimation()
    {
        if(m_HasHarvestAnimation)
            m_Animator.Play(m_HarvestAnimationHash, 0);
    }

    internal void PlayAbilityAnimation(int index)
    {
        if (m_HasAbilityAnimation)
            m_Animator.Play(m_AbilityAnimationHashes[index], 0);
    }

    internal void EnableWalkAnimation()
    {
        if(m_HasWalkAnimation)
            m_Animator.SetBool(m_IsWalkingHash, true);
    }

    internal void DisableWalkAnimation()
    {
        if(m_HasWalkAnimation)
            m_Animator.SetBool(m_IsWalkingHash, false);
    }
}
