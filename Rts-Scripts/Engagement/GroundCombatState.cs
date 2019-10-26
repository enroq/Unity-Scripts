using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class GroundCombatState : KaryonBehaviour, ICombatant
{
    [SerializeField]
    private bool m_DebugMode;
    [SerializeField]
    private CombatType m_AttackType;
    [SerializeField]
    private List<CombatantTargetType> m_TargetTypesAllowed;
    [SerializeField]
    private bool m_HasAreaEffect;
    [SerializeField]
    private float m_AreaOfEffect;
    [SerializeField]
    private float m_AreaEffectFactor;
    [SerializeField]
    private float m_AttackCooldown;
    [SerializeField]
    private float m_AttackRange;
    [SerializeField]
    private int m_AttackDamage;
    [SerializeField]
    private float m_ResponseRange;
    [SerializeField]
    private float m_AttackStopBuffer = 1.0f;
    [SerializeField]
    private float m_ResponseTime;
    [SerializeField]
    private float m_DamageDelayTime;
    [SerializeField]
    private GameObject m_ProjectileObject;
    [SerializeField]
    private GameObject m_ProjectileOrigin;
    [SerializeField]
    private float m_ProjectileDelayTime;

    private GameObject m_CurrentTarget;
    private Collider m_TargetCollider;
    private BaseEntity m_TargetEntity;

    private Vector3 m_TargetPositionCache;
    private Vector3 m_PositionRelativeToTarget;
    private Vector3 m_DeltaRotation;
    private Vector3 m_TargetDeltaRotation;
    private Vector3 m_FeelerRayOrigin;

    private bool m_NavigatingToTarget = false;
    private bool m_CurrentlyEngaged = false;

    BaseEntity m_EntityRelative;

    private float m_CurrentAttackCooldown;
    private float m_CurrentResponseCooldown;

    private float m_TurnSpeed = 3.5f;

    Coroutine m_DelayDamageRoutine;
    Coroutine m_DelayProjectileRoutine;

    RaycastHit[] m_FeelerCache;
    Ray m_FeelerRay = new Ray();

    RaycastHit[] m_EngagementRayCache;
    Ray m_EngagementRay = new Ray();

    Collider[] m_EngagmentColliders;
    List<Collider> m_CombatantColliders = new List<Collider>();
    BaseEntity m_EngagementEntityCache;

    GameObject m_ProjectileObjectCache;
    BaseProjectile m_ProjectileComponentCache;

    public CombatType AttackType
    {
        get { return m_AttackType; }
    }

    public CombatantTargetType TargetType
    {
        get { return CombatantTargetType.Ground; }
    }

    public int AttackRating
    {
        get { return m_AttackDamage; }
        set { m_AttackDamage = value; }
    }

    void Start ()
    {
        GameEngine.EngagementHandler.AttachCombatState(this);

        m_CurrentAttackCooldown = m_AttackCooldown;
        m_CurrentResponseCooldown = m_ResponseTime;

        if ((m_EntityRelative = gameObject.GetComponent<BaseEntity>()) == null)
            throw new MissingComponentException("Object with combat state missing BaseEntity component.");

        if(m_AttackType == CombatType.Ranged || m_AttackType == CombatType.Siege)
        {
            if (m_ProjectileObject == null)
                throw new MissingComponentException(string.Format("{0}'s Combat State Is Missing Projectile Object.", gameObject));
            if (m_ProjectileOrigin == null)
                throw new MissingComponentException(string.Format("{0}'s Combat State Is Missing Projectile Origin.", gameObject));
        }
	}

    internal int Team
    {
        get { return m_EntityRelative.Team; }
    }

    internal bool IsUnit
    {
        get { return gameObject.GetComponent<BaseUnit>() != null; }
    }

    internal bool ShouldSeekCombatants
    {
        get
        {
            switch (m_EntityRelative.CurrentCommand)
            {
                case CommandType.None: return true;
                case CommandType.Attack: return true;

                default:
                    return false;
            }
        }
    }

    public void ClearEngagement()
    {
        if (GameEngine.DebugMode || m_DebugMode)
            Debug.Log("Clearing Current Engagment For: " + gameObject.name);

        m_TargetEntity = null;
        m_CurrentTarget = null;
        m_TargetCollider = null;

        m_CurrentlyEngaged = false;
        m_NavigatingToTarget = false;
    }

    public void EngageEntity(BaseEntity entity)
    {
        if (entity != null)
        {
            m_CurrentlyEngaged = true;

            m_TargetEntity = entity;
            m_CurrentTarget = entity.gameObject;
            foreach(Collider collider in m_CurrentTarget.GetComponents<Collider>())
            {
                if (!collider.isTrigger) { m_TargetCollider = collider; break; }
            }
        }
    }

    public void ProcessCombatState(float iterationRate)
    {
        ProcessResponseTime(iterationRate);
        ProcessPursuit(iterationRate);
        ProcessCooldown(iterationRate);
    }

    public void UpdateCombatState(float deltaTime)
    {
        if(m_EntityRelative is BaseUnit)
            TurnTowardsEnemy(deltaTime);
    }

    internal void ProcessPursuit(float f)
    {
        if (!ShouldSeekCombatants)
            return;

        if (m_EntityRelative is BaseUnit && m_CurrentTarget != null)
        {
            QueryPursuitStatus();
        }
    }

    internal void ProcessCooldown(float f)
    {
        if (m_CurrentAttackCooldown > 0)
            m_CurrentAttackCooldown -= f;

        if (m_CurrentAttackCooldown <= 0)
            ProcessAttack();
    }

    internal void ProcessResponseTime(float f)
    { 
        if (ShouldSeekCombatants && !TargetInResponseRange() && !m_NavigatingToTarget)
        {
            if(m_CurrentResponseCooldown > 0)
                m_CurrentResponseCooldown -= f;

            if (m_CurrentResponseCooldown <= 0)
            {
                m_CurrentResponseCooldown = m_ResponseTime;
                EngageEntity(QueryClosestEntity());
            }
        }
    }

    internal void TurnTowardsEnemy(float deltaTime)
    {
        if (m_CurrentTarget != null)
        {
            m_PositionRelativeToTarget = m_CurrentTarget.transform.position - transform.position;
            m_DeltaRotation = Quaternion.LookRotation(m_PositionRelativeToTarget).eulerAngles;
            m_DeltaRotation.x = 0;
            transform.rotation = Quaternion.Slerp
                (transform.rotation, Quaternion.Euler(m_DeltaRotation), deltaTime * m_TurnSpeed);
        }
    }

    internal void QueryPursuitStatus()
    {
        if (m_CurrentTarget == null)
            return;

        if (!IsInStopRange())
        {
            if (!m_NavigatingToTarget)
            {
                m_NavigatingToTarget = true;

                if (m_TargetEntity is BaseBuilding)
                    m_TargetPositionCache = ((BaseBuilding)m_TargetEntity).
                        DetermineApproach(m_EntityRelative).Value;

                else m_TargetPositionCache = m_CurrentTarget.transform.position;

                ((BaseUnit)m_EntityRelative).GoToPosition
                    (m_TargetPositionCache, m_AttackRange - (m_AttackStopBuffer * 2));

            }
            else if(m_TargetPositionCache != m_CurrentTarget.transform.position && m_TargetEntity is BaseUnit)
            {
                m_TargetPositionCache = m_CurrentTarget.transform.position;
                ((BaseUnit)m_EntityRelative).UpdateNavDestination
                    (m_TargetPositionCache, m_AttackRange - (m_AttackStopBuffer * 2));
            }
        }

        else if (m_NavigatingToTarget && IsInStopRange())
        {
            m_NavigatingToTarget = false;
            ((BaseUnit)m_EntityRelative).HaltNavigation();
        }
    }

    private void ProcessAttack()
    {
        if (m_CurrentTarget == null && m_CurrentlyEngaged)
            ClearEngagement();

        if (m_CurrentTarget == null || !ShouldSeekCombatants || m_NavigatingToTarget)
            return;

        if (m_TargetEntity.CurrentHealth <= 0)
        {
            ClearEngagement();
            if (GameEngine.DebugMode && m_DebugMode)
                Debug.Log("Target Entity Health At Zero, Abandoning Engagement.");
        }

        if (IsInAttackRange())
        {
            LaunchAttack();
            m_CurrentAttackCooldown = m_AttackCooldown;
        }
    }

    private void LaunchAttack()
    {
        if(AttackType == CombatType.Ranged)
        {
            if (m_CurrentTarget != null)
            {
                PlayAttackAnimation();
                m_DelayProjectileRoutine = 
                    StartCoroutine(DelayProjectile());
            }
        }

        else if(AttackType == CombatType.Melee)
        {
            if (m_CurrentTarget != null)
            {
                PlayAttackAnimation();
                m_DelayDamageRoutine = 
                    StartCoroutine(DelayDamage());
            }
        }
    }

    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(m_DamageDelayTime);
        if(m_CurrentTarget != null)
            m_CurrentTarget.GetComponent
                <BaseEntity>().OnHit(m_AttackDamage);
    }

    IEnumerator DelayProjectile()
    {
        yield return new 
            WaitForSeconds(m_ProjectileDelayTime);

        if (m_CurrentTarget != null)
            LaunchProjectile();
    }

    private void LaunchProjectile()
    {
        m_ProjectileObjectCache = GameEngine.ObjectPoolHandler.ExtractObject
            (m_ProjectileObject, m_ProjectileOrigin.transform.position);

        if (GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("{0} Launching Projectile: [{1}]", gameObject.name, m_ProjectileObjectCache.name));

        if (m_ProjectileObjectCache != null)
        {
            m_ProjectileObjectCache.transform.LookAt(m_CurrentTarget.transform);

            m_ProjectileComponentCache = m_ProjectileObjectCache.GetComponent<BaseProjectile>();
            m_ProjectileComponentCache.SetTarget(m_CurrentTarget);
            m_ProjectileComponentCache.InitializeProjectileState
                (m_AttackDamage, m_HasAreaEffect, m_AreaOfEffect, m_AreaEffectFactor);
        }

        else
            throw new UnityException("Unable To Extract Object From Pool.");
    }

    private void PlayAttackAnimation()
    {
        m_EntityRelative.PlayAttackAnimation();
    }

    private bool IsInAttackRange()
    {
        if(m_CurrentTarget != null)
        {
            m_FeelerRay.direction = m_TargetEntity.Origin - m_EntityRelative.Origin;
            m_FeelerRay.origin = m_EntityRelative.Origin;
            m_FeelerCache = Physics.SphereCastAll(m_FeelerRay, 0.5f, m_AttackRange * 1.5f);

            if(GameEngine.DebugMode && m_DebugMode)
                Debug.Log(string.Format("{0} Feeler Contacts {1} Object(s)", gameObject.name, m_FeelerCache.Length));

            for(int i = m_FeelerCache.Length -1; i >= 0; i--)
            {
                if (m_FeelerCache[i].collider == m_TargetCollider)
                {
                    if (m_FeelerCache[i].distance <= m_AttackRange)
                        return true;

                    else if(GameEngine.DebugMode && m_DebugMode)
                        Debug.Log(string.Format("{0}'s Feeler Ray Out Of Range [{1} : {2}]",
                            gameObject.name, m_FeelerCache[i].distance, m_AttackRange));
                }

                else if(GameEngine.DebugMode && m_DebugMode)
                    Debug.Log(string.Format("{0} Not Equal To Target Collider: {1}", 
                        m_FeelerCache[i].collider, m_TargetCollider));
            }
        }

        return false;
    }

    private bool IsInStopRange()
    {
        if (m_CurrentTarget != null)
        {
            m_FeelerRay.direction = m_TargetEntity.Origin - m_EntityRelative.Origin;
            m_FeelerRay.origin = m_EntityRelative.Origin;
            m_FeelerCache = Physics.SphereCastAll(m_FeelerRay, 0.5f, m_AttackRange * 1.5f);

            if (GameEngine.DebugMode && m_DebugMode)
                Debug.Log(string.Format("{0} Feeler Contacts {1} Object(s)", gameObject.name, m_FeelerCache.Length));

            for (int i = m_FeelerCache.Length - 1; i >= 0; i--)
            {
                if (m_FeelerCache[i].collider == m_TargetCollider)
                {
                    if (m_FeelerCache[i].distance <= m_AttackRange - m_AttackStopBuffer)
                        return true;

                    else if (GameEngine.DebugMode && m_DebugMode)
                        Debug.Log(string.Format("{0}'s Feeler Ray Out Of Range [{1} : {2}]",
                            gameObject.name, m_FeelerCache[i].distance, m_AttackRange));
                }

                else if (GameEngine.DebugMode && m_DebugMode)
                    Debug.Log(string.Format("{0} Not Equal To Target Collider: {1}",
                        m_FeelerCache[i].collider, m_TargetCollider));
            }
        }

        return false;
    }

    private bool TargetInResponseRange()
    {
        if (m_EntityRelative.CurrentCommand == CommandType.Attack && m_TargetEntity != null)
            return true;

        if (m_CurrentTarget != null)
        {
            float distance = 
                (m_CurrentTarget.transform.position - transform.position).sqrMagnitude;

            if (distance <= Mathf.Pow(m_ResponseRange, 2))
                return true;
            else
            {
                if(GameEngine.DebugMode && m_DebugMode)
                    Debug.Log(string.Format
                        ("{0}'s Distance ({1}) To Current Target Greater Than Response Range.", 
                            gameObject.name, distance, m_ResponseRange));

                ClearEngagement(); return false;
            }
        }

        else
            return false;
    }

    private BaseEntity QueryClosestEntity()
    {
        m_EngagmentColliders = Physics.OverlapSphere(transform.position, m_ResponseRange);
        m_CombatantColliders.Clear();

        for (int i = m_EngagmentColliders.Length - 1; i >= 0; i--)
        {
            if (m_EngagmentColliders[i].isTrigger == false)
            {
                if ((m_EngagementEntityCache = m_EngagmentColliders[i].gameObject.GetComponent<BaseEntity>()) != null)
                {
                    if (m_EngagementEntityCache.Team != Team && m_EngagementEntityCache.Team != -1)
                    {
                        if (m_EngagementEntityCache.GetComponent<ICombatant>() != null)
                        {
                            if (m_TargetTypesAllowed.Contains(m_EngagementEntityCache.GetComponent<ICombatant>().TargetType))
                                m_CombatantColliders.Add(m_EngagmentColliders[i]);
                        }

                        else
                            m_CombatantColliders.Add(m_EngagmentColliders[i]);
                    }
                }
            }
        }

        if (m_CombatantColliders.Count > 0)
        {
            m_EngagmentColliders = OrderCollidersByDistance
                (m_CombatantColliders.ToArray(), transform.position).ToArray();

            return m_EngagmentColliders[0].GetComponent<BaseEntity>();
        }

        else
            return null;
    }

    internal void OnDrawGizmos()
    {
        if (m_DebugMode && m_FeelerCache != null)
        {
            var line = GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.yellow };

                line.startWidth = 0.15f;
                line.endWidth = 0.15f;

                line.startColor = Color.yellow;
                line.endColor = Color.yellow;
            }

            line.positionCount = m_FeelerCache.Length;

            for (int i = 0; i < m_FeelerCache.Length; i++)
                line.SetPosition(i, m_FeelerCache[i].point);
        }
    }
}

/*Notes
 * Need to look into the feasibility of using coroutines for attacking in the name of less resource usage.
 */