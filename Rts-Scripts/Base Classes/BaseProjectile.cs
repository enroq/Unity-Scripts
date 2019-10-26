using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class BaseProjectile : MonoBehaviour, IPoolable
{
    public int ParentInstanceId { get; set; }

    [SerializeField]
    float m_ProjectileSpeed;
    [SerializeField]
    List<GameObject> m_ProjectileParticles;
    [SerializeField]
    GameObject m_CollisionParticle;
    [SerializeField]
    bool m_ProducesSound;

    private GameObject m_TargetObject;
    private GameObject m_DeltaParticle;
    private Collider m_TargetCollider;

    private ProjectileSoundController m_SoundController;

    private List<Collider> m_Colliders = new List<Collider>();
    private List<GameObject> m_AttachedParticles = new List<GameObject>();

    private float m_DeltaSpeed;
    private bool m_TargetSet;

    private int m_ProjectileDamage;
    private bool m_HasAreaEffect;
    private float m_AreaOfEffect;
    private float m_AreaEffectFactor;

    private Vector3 m_PositionRelativeToTarget;
    private Vector3 m_DeltaRotation;
    private Vector3 m_CollisionPoisiton;
    private Vector3 m_CollisonRotation;

    public GameObject TargetObject
    {
        get { return m_TargetObject; }
    }

    public int ProjectileDamage
    {
        get { return m_ProjectileDamage; }
    }

    public bool HasAreaEffect
    {
        get { return m_HasAreaEffect; }
    }

    public float AreaOfEffect
    {
        get { return m_AreaOfEffect; }
    }

    public float AreaEffectFactor
    {
        get { return m_AreaEffectFactor; }
    }

    private void Start()
    {
        if (m_ProducesSound)
        {
            m_SoundController = gameObject.GetComponent<ProjectileSoundController>();
            if (m_SoundController == null)
                throw new UnityException(string.Format("{0} Missing Projectile Sound Controller!", gameObject));
        }

    }

    public void OnExtraction()
    {
        GameEngine.ProjectileHandler.AddProjectileToCache(this);
        for (int i = 0; i < m_ProjectileParticles.Count; i++)
        {
            m_DeltaParticle = GameEngine.ObjectPoolHandler.ExtractObject
                (m_ProjectileParticles[i], transform.position);
            m_DeltaParticle.transform.parent = transform;
            m_AttachedParticles.Add(m_DeltaParticle);
        }
    }

    internal void ResetTargets()
    {
        m_TargetSet = false;
        m_TargetObject = null;
        m_TargetCollider = null;

        m_Colliders = new List<Collider>();
    }

    internal void SetTarget(GameObject target)
    {
        m_TargetObject = target;

        if (m_TargetObject != null)
            m_TargetCollider = m_TargetObject.GetComponent<Collider>();

        m_TargetSet = true;
    }

    internal void InitializeProjectileState
        (int damage, bool hasAreaEffect, float areaOfEffect, float areaEffectFactor)
    {
        m_ProjectileDamage = damage;
        m_HasAreaEffect = hasAreaEffect;
        m_AreaOfEffect = areaOfEffect;
        m_AreaEffectFactor = areaEffectFactor;
    }

    internal void ProcessProjectileMovement(float deltaTime)
    {
        if (m_TargetObject == null && m_TargetSet)
            GameEngine.ObjectPoolHandler.ReclaimObject(ParentInstanceId, gameObject);

        else
        {
            m_DeltaSpeed = m_ProjectileSpeed * deltaTime;
            transform.position = Vector3.MoveTowards
                (transform.position, m_TargetObject.GetComponent<BaseEntity>().Origin, m_DeltaSpeed);

            TurnTowardsTarget(deltaTime);

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                if (hit.collider == m_TargetCollider)
                    m_CollisionPoisiton = hit.point;
            }

            if (QueryCollisionWithTarget())
                OnImpact();
        }
    }

    private void TurnTowardsTarget(float deltaTime)
    {
        m_PositionRelativeToTarget = m_TargetObject.transform.position - transform.position;
        if (m_PositionRelativeToTarget != Vector3.zero)
        {
            m_DeltaRotation = Quaternion.LookRotation(m_PositionRelativeToTarget).eulerAngles;
            transform.rotation = Quaternion.Slerp
                (transform.rotation, Quaternion.Euler(m_DeltaRotation), deltaTime * m_ProjectileSpeed);
        }
    }

    internal bool QueryCollisionWithTarget()
    {
        if (m_Colliders.Contains(m_TargetCollider))
        {
            m_CollisonRotation = -gameObject.transform.rotation.eulerAngles;
            return true;
        }
        else
            return false;
    }


    internal void OnImpact()
    {
        if (GameEngine.DebugMode)
            Debug.Log(string.Format("{0} Impacted {1}", gameObject.name, m_TargetCollider.name));

        for (int i = 0; i < m_AttachedParticles.Count; i++)
            m_AttachedParticles[i].transform.parent = null;

        m_AttachedParticles.Clear();

        GameEngine.ObjectPoolHandler.ReclaimObject(ParentInstanceId, gameObject);
        if (m_TargetObject != null)
        {
            m_TargetObject.GetComponent<BaseEntity>().OnHit(m_ProjectileDamage);
            if (m_CollisionParticle != null)
                InitializeCollisionParticle();

            ResetTargets();
        }
    }

    internal void InitializeCollisionParticle()
    {
       GameObject particle = GameEngine.ObjectPoolHandler
            .ExtractObject(m_CollisionParticle, m_CollisionPoisiton);

        if(particle != null)
            particle.transform.rotation = Quaternion.Euler(m_CollisonRotation);
        else
            throw new UnityException("Unable To Extract Object From Pool.");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!m_Colliders.Contains(other))
            m_Colliders.Add(other);
    }

    public void OnTriggerExit(Collider other)
    {
        if (m_Colliders.Contains(other))
            m_Colliders.Remove(other);
    }
}
