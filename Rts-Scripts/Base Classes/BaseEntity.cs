using System.Collections;
using UnityEngine;

#pragma warning disable
public class BaseEntity : KaryonBehaviour
{
    [SerializeField]
    internal bool m_DebugMode;
    [SerializeField]
    private string m_EntityName = string.Empty;
    [SerializeField]
    private GameObject m_Origin;
    [SerializeField]
    private bool m_IsAnimated = false;
    [SerializeField]
    private bool m_ProducesSound = false;
    [SerializeField]
    private int m_MaxHealth;
    [SerializeField]
    private int m_CurrentHealth;
    [SerializeField]
    private Sprite m_EntityIcon;             
    [SerializeField]
    private float m_RegenRate = 1.0f;        
    [SerializeField]
    private int m_RegenValue = 1;            
    [SerializeField]
    private bool m_IsRegenerating = false;
    [SerializeField]
    private bool m_CanRegenerate = false;
    [SerializeField]
    private int m_DefenseRating = 1;
    [SerializeField]
    private bool m_Invulnerable = false;
    [SerializeField]
    private int m_EntityTeam = -1;
    [SerializeField]
    private GameObject m_CorpseObject;
    [SerializeField]
    private float m_DeathTime = 1.0f;
    [SerializeField]
    private bool m_AnimatedDeath = false;

    Coroutine m_RegenRoutine;
    Coroutine m_DeathRoutine;

    AnimationHandler m_AnimationHandler;
    EntitySoundController m_SoundController;

    private CommandType m_CurrentCommand;

    public string EntityName
    {
        get { return m_EntityName; }
    }

    public Sprite EntityIcon
    {
        get { return m_EntityIcon; }
        set { m_EntityIcon = value; }
    }

    public int CurrentHealth
    {
        get { return m_CurrentHealth; }
        set { m_CurrentHealth = value; }
    }

    public int MaxHealth
    {
        get { return m_MaxHealth; }
        set { m_MaxHealth = value; }
    }

    public int Team
    {
        get { return m_EntityTeam; }
        set { m_EntityTeam = value; }
    }

    public int DefenseRating
    {
        get { return m_DefenseRating; }
        set { m_DefenseRating = value; }
    } 

    public CommandType CurrentCommand
    {
        get { return m_CurrentCommand; }
    }

    public Vector3 Origin
    {
        get
        {
            if (m_Origin != null)
                return m_Origin.transform.position;
            else
                return transform.position;
        }
    }

    public virtual void Start()
    {
        if(m_CanRegenerate)
            m_RegenRoutine = StartCoroutine(RegenHealth());

        if(GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("{0} Initialized Base Entity", gameObject));

        if (m_EntityName.Equals(string.Empty))
            throw new UnityException(string.Format("{0} Base Unit Component Missing Name.", gameObject));

        if(m_IsAnimated)
            if ((m_AnimationHandler = gameObject.GetComponent<AnimationHandler>()) == null)
                throw new MissingComponentException("Base Entity Missing Animation Handler Component");

        if(m_ProducesSound)
            if ((m_SoundController = gameObject.GetComponent<EntitySoundController>()) == null)
                throw new MissingComponentException("Base Entity Missing Entity Sound Controller.");
    }

    internal virtual void UpdateCommandState(CommandType command)
    {
        m_CurrentCommand = command;
    }

    internal void IncreaseHealth(int val)
    {
        if (CurrentHealth + val > MaxHealth)
            CurrentHealth = MaxHealth;
        else
            CurrentHealth += val;
    }

    internal void DecreaseHealth(int val)
    {
        if (m_CurrentHealth - val > 0)
            m_CurrentHealth -= val;
        else
            OnDeath();
    }

    internal virtual void OnDeath()
    {
        if (GetComponent<ICombatant>() != null)
            GameEngine.EngagementHandler.DetatchCombatState(GetComponent<ICombatant>());

        if (m_CorpseObject != null)
        {
            Instantiate
                (m_CorpseObject, gameObject.transform.position, transform.rotation);
        }

        if (gameObject.GetComponent<SelectableObject>().IsSelected)
            GameEngine.SelectionHandler.DeselectObject(gameObject.GetComponent<SelectableObject>());

        if (m_AnimatedDeath)
        {
            PlayDeathAnimation();
            m_DeathRoutine = 
                StartCoroutine(DelayDestroyAfterDeath());
        }

        else
            Destroy(gameObject);
    }

    internal virtual void OnHit(int damage)
    {
        PlayTakeHitSound();
        OnDamage(damage);
    }

    internal virtual void OnDamage(int damage)
    {
        if (damage - DefenseRating > 0)
            DecreaseHealth(damage - DefenseRating);
        else
            DecreaseHealth(1);
    }

    IEnumerator DelayDestroyAfterDeath()
    {
        yield return new 
            WaitForSeconds(m_DeathTime);
        if(gameObject != null)
            Destroy(gameObject);
    }

    IEnumerator RegenHealth()
    {
        yield return new WaitForSeconds(m_RegenRate);

        if (m_CanRegenerate)
        {
            if (CurrentHealth >= MaxHealth)
                m_IsRegenerating = false;
            else
                m_IsRegenerating = true;

            if (m_IsRegenerating)
                IncreaseHealth(m_RegenValue);
        }
            
        m_RegenRoutine = StartCoroutine(RegenHealth());
    }

    internal virtual void OnSelect()   { }
    internal virtual void OnDeselect() { }

    internal void PlayAttackAnimation()
    {
        if(m_IsAnimated)
            m_AnimationHandler.PlayAttackAnimation();
    }

    internal void PlayDeathAnimation()
    {
        if (m_IsAnimated)
            m_AnimationHandler.PlayDeathAnimation();
    }

    internal void EnableWalkAnimation()
    {
        if (m_IsAnimated)
            m_AnimationHandler.EnableWalkAnimation();
    }

    internal void DisableWalkAnimation()
    {
        if (m_IsAnimated)
            m_AnimationHandler.DisableWalkAnimation();
    }

    internal void PlayHarvestAnimation()
    {
        if (m_IsAnimated)
            m_AnimationHandler.PlayHarvestAnimation();
    }

    internal void PlayAbilityAnimation(int index)
    {
        if (m_IsAnimated)
            m_AnimationHandler.PlayAbilityAnimation(index);
    }

    internal void PlayAttackSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeAttackSound();
    }

    internal void PlaySelectionSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeSelectionSound();
    }

    internal void PlayInitializationSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeInitilaizationSound();
    }

    internal void PlayTakeHitSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeTakeHitSound();
    }

    internal void PlayHarvestSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeHarvestSound();
    }

    internal void PlayDeathSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeDeathSound();
    }

    internal void PlayMovementConfirmSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeMovementConfirmSound();
    }

    internal void PlayAttackConfirmSound()
    {
        if (m_SoundController != null)
            m_SoundController.InvokeAttackConfirmSound();
    }
}
