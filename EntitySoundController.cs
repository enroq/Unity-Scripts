using UnityEngine;

#pragma warning disable
public class EntitySoundController : MonoBehaviour
{
    [SerializeField]
    string m_AttackSoundId;
    [SerializeField]
    string m_SelectionSoundId;
    [SerializeField]
    string m_EntityInitializeSoundId;
    [SerializeField]
    string m_TakeHitSoundId;
    [SerializeField]
    string m_TakeHeavyHitSoundId;
    [SerializeField]
    string m_HarvestSoundId;
    [SerializeField]
    string m_DeathSoundId;
    [SerializeField]
    string m_AttackConfirmSoundId;
    [SerializeField]
    string m_MovementConfirmSoundId;

    [SerializeField]
    bool m_HasAttackSound;
    [SerializeField]
    bool m_HasSelectionSound;
    [SerializeField]
    bool m_HasMoveSound;
    [SerializeField]
    bool m_HasTrainCompleteSound;
    [SerializeField]
    bool m_HasTakeHitSound;
    [SerializeField]
    bool m_HasTakeHeavyHitSound;
    [SerializeField]
    bool m_HasHarvestSound;
    [SerializeField]
    bool m_HasDeathSound;
    [SerializeField]
    bool m_HasAttackConfirmSound;
    [SerializeField]
    bool m_HasMovementConfirmSound;

	void Start ()
    {
        if (string.IsNullOrEmpty(m_AttackSoundId) && m_HasAttackSound)
            throw new UnityException(string.Format("{0} Is Missing Attack Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_SelectionSoundId) && m_HasSelectionSound)
            throw new UnityException(string.Format("{0} Is Missing Select Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_EntityInitializeSoundId) && m_HasTrainCompleteSound)
            throw new UnityException(string.Format("{0} Is Missing Train Completion Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_TakeHitSoundId) && m_HasTakeHitSound)
            throw new UnityException(string.Format("{0} Is Missing Take Hit Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_TakeHeavyHitSoundId) && m_HasTakeHeavyHitSound)
            throw new UnityException(string.Format("{0} Is Missing Take Hit Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_HarvestSoundId) && m_HasHarvestSound)
            throw new UnityException(string.Format("{0} Is Missing Harvest Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_DeathSoundId) && m_HasDeathSound)
            throw new UnityException(string.Format("{0} Is Missing Death Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_AttackConfirmSoundId) && m_HasAttackConfirmSound)
            throw new UnityException(string.Format("{0} Is Missing Attack Confirm Sound Id!", gameObject));
        if (string.IsNullOrEmpty(m_MovementConfirmSoundId) && m_HasMovementConfirmSound)
            throw new UnityException(string.Format("{0} Is Missing Harvest Sound Id!", gameObject));

    }

    string FormatEvent(string id)
    {
        return "event:/" + id;
    }

    internal void InvokeAttackSound()
    {
        if(!string.IsNullOrEmpty(m_AttackSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_AttackSoundId), gameObject);
    }

    internal void InvokeSelectionSound()
    {
        if (!string.IsNullOrEmpty(m_SelectionSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_SelectionSoundId), gameObject);
    }

    internal void InvokeInitilaizationSound()
    {
        if (!string.IsNullOrEmpty(m_EntityInitializeSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_EntityInitializeSoundId), gameObject);
    }

    internal void InvokeTakeHitSound()
    {
        if (!string.IsNullOrEmpty(m_TakeHitSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_TakeHitSoundId), gameObject);
    }

    internal void InvokeTakeHeavyHitSound()
    {
        if (!string.IsNullOrEmpty(m_TakeHeavyHitSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_TakeHeavyHitSoundId), gameObject);
    }

    internal void InvokeHarvestSound()
    {
        if (!string.IsNullOrEmpty(m_HarvestSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_HarvestSoundId), gameObject);
    }

    internal void InvokeDeathSound()
    {
        if (!string.IsNullOrEmpty(m_DeathSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_DeathSoundId), gameObject);
    }

    internal void InvokeAttackConfirmSound()
    {
        if (!string.IsNullOrEmpty(m_AttackConfirmSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_AttackConfirmSoundId), gameObject);
    }

    internal void InvokeMovementConfirmSound()
    {
        if (!string.IsNullOrEmpty(m_MovementConfirmSoundId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_MovementConfirmSoundId), gameObject);
    }
}
