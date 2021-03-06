﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class BuildingCombatState : KaryonBehaviour, ICombatant
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
    private int m_AttackDamage;
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

    List<Collider> m_EngagmentColliders = new List<Collider>();
    List<Collider> m_CombatantColliders = new List<Collider>();

    Collider[] m_EngagementColliderCache;

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

    void Start()
    {
        GameEngine.EngagementHandler.AttachCombatState(this);

        m_CurrentAttackCooldown = m_AttackCooldown;
        m_CurrentResponseCooldown = m_ResponseTime;

        if ((m_EntityRelative = gameObject.GetComponent<BaseEntity>()) == null)
            throw new MissingComponentException("Object with combat state missing BaseEntity component.");

        if (m_AttackType == CombatType.Ranged || m_AttackType == CombatType.Siege)
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
    }

    public void EngageEntity(BaseEntity entity)
    {
        if (entity != null)
        {
            m_TargetEntity = entity;
            m_CurrentTarget = entity.gameObject;
            foreach (Collider collider in m_CurrentTarget.GetComponents<Collider>())
            {
                if (!collider.isTrigger) { m_TargetCollider = collider; break; }
            }
        }
    }

    public void ProcessCombatState(float iterationRate)
    {
        ProcessExpiredColliders();
        ProcessResponseTime(iterationRate);
        ProcessCooldown(iterationRate);
    }

    public virtual void UpdateCombatState(float deltaTime) { }

    internal void ProcessExpiredColliders()
    {
        m_EngagementColliderCache = m_EngagmentColliders.ToArray();

        for(int i = m_EngagementColliderCache.Length -1; i >= 0; i--)
        {
            if (m_EngagementColliderCache[i] == null)
                m_EngagmentColliders.Remove(m_EngagementColliderCache[i]);
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
        if (ShouldSeekCombatants)
        {
            if (m_CurrentResponseCooldown > 0)
                m_CurrentResponseCooldown -= f;

            if (m_CurrentResponseCooldown <= 0)
            {
                m_CurrentResponseCooldown = m_ResponseTime;
                EngageEntity(QueryClosestEntity());
            }
        }
    }

    private void ProcessAttack()
    {
        if (m_CurrentTarget == null || !ShouldSeekCombatants)
            return;

        if (m_TargetEntity.CurrentHealth <= 0)
        {
            ClearEngagement();
            if (GameEngine.DebugMode || m_DebugMode)
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
        if (AttackType == CombatType.Ranged)
        {
            if (m_CurrentTarget != null)
            {
                PlayAttackAnimation();
                m_DelayProjectileRoutine =
                    StartCoroutine(DelayProjectile());
            }
        }

        else if (AttackType == CombatType.Melee)
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
        if (m_CurrentTarget != null)
        {
            m_CurrentTarget.GetComponent<BaseEntity>().OnHit(m_AttackDamage);
            m_EntityRelative.PlayAttackSound();
        }
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

        if (GameEngine.DebugMode || m_DebugMode)
            Debug.Log(string.Format("{0} Launching Projectile: [{1}]", gameObject.name, m_ProjectileObjectCache.name));

        if (m_ProjectileObjectCache != null && m_CurrentTarget != null)
        {
            m_ProjectileObjectCache.transform.LookAt(m_CurrentTarget.transform);

            m_ProjectileComponentCache = m_ProjectileObjectCache.GetComponent<BaseProjectile>();
            m_ProjectileComponentCache.SetTarget(m_CurrentTarget);
            m_ProjectileComponentCache.InitializeProjectileState
                (m_AttackDamage, m_HasAreaEffect, m_AreaOfEffect, m_AreaEffectFactor);
            m_EntityRelative.PlayAttackSound();
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
        if (m_CurrentTarget != null)
        {
            if (m_EngagmentColliders.Contains(m_TargetCollider))
                return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!m_EngagmentColliders.Contains(other))
            m_EngagmentColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_EngagmentColliders.Contains(other))
            m_EngagmentColliders.Remove(other);
    }

    private BaseEntity QueryClosestEntity()
    {
        m_CombatantColliders.Clear();
        for (int i = m_EngagmentColliders.Count - 1; i >= 0; i--)
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
                (m_CombatantColliders.ToArray(), transform.position);

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
