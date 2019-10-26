using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable
public class FoundationBehavior : BaseEntity
{
    List<Collider> m_CurrentCollisions = new List<Collider>();

    [SerializeField]
    private GameObject m_BuildStatusPlane;
    [SerializeField]
    private GameObject m_PlacementObject;
    [SerializeField]
    private List<GameObject> m_FoundationStates;

    int m_CurrentStepIndex = 0;

    int m_StepProgressThreshold;
    int m_StepIntervalAmount;

    NavMeshObstacle m_NavObstacle;
    Coroutine m_QueryCollisionRoutine;
    Collider[] m_CollisionCache;

    Vector3? m_DestinationCache = Vector3.zero;
    Vector3 m_DestinationOffset = Vector3.zero;

    Collider[] m_DeltaColliders;

    Ray m_DestinationRay;
    RaycastHit[] m_DestinationRayHits = new RaycastHit[128];

    int m_DestinationHitIndex = -1;
    int m_DestinationRayHitCount = -1;

    BaseEntity m_ApproachingEntity;

    internal NavMeshObstacle NavObstacle
    {
        get { return m_NavObstacle; }
    }

    internal GameObject BuildStatusPlane
    {
        get { return m_BuildStatusPlane; }
        set { m_BuildStatusPlane = value; }
    }

    internal bool WithoutCollision
    {
        get { return m_CurrentCollisions.Count == 0; }
    }

    internal BaseBuilding ParentBuilding { get; set; }

    internal WorkerTask ConstructionTask
    {
        get { return ParentBuilding.ConstructionTask; }
    }

    internal int DetermineStepInterval()
    {
        if (m_FoundationStates.Count > 0)
            return ConstructionTask.MaxProgressLevel / m_FoundationStates.Count;
        else
            return 0;
    }

    internal void ProcessCurrentStep()
    {
        m_StepIntervalAmount = DetermineStepInterval();

        if (m_StepIntervalAmount != 0)
        {
            m_StepProgressThreshold = m_CurrentStepIndex > 0 ?
                m_CurrentStepIndex * m_StepIntervalAmount : m_StepIntervalAmount;

            if (ConstructionTask.TaskProgressLevel >= m_StepProgressThreshold)
            {
                m_CurrentStepIndex++;
                m_FoundationStates[m_CurrentStepIndex - 1].SetActive(false);
                m_FoundationStates[m_CurrentStepIndex].SetActive(true);
            }
        }
    }

    void Start()
    {
        if (gameObject.GetComponent<Rigidbody>() == null)
            throw new MissingComponentException("Building Foundation Missing Rigidbody Component.");

        if ((m_NavObstacle = gameObject.GetComponent<NavMeshObstacle>()) == null)
            throw new MissingComponentException("Building Foundation Missing Nav Obstacle Component");

        if (m_BuildStatusPlane == null)
            throw new MissingComponentException("Building Foundation Missing Build Status Plane.");

        m_QueryCollisionRoutine = StartCoroutine(QueryCollisions());
    }

    internal override void OnDamage(int damage)
    {
        ParentBuilding.OnDamage(damage);
        base.OnDamage(damage);
    }

    internal override void OnDeath()
    {
        Destroy(ParentBuilding.gameObject);
        base.OnDeath();
    }

    IEnumerator QueryCollisions()
    {
        yield return new WaitForSeconds(0.5f); 
        m_CollisionCache = m_CurrentCollisions.ToArray();

        for(int i = m_CollisionCache.Length -1; i >= 0; i--)
            if(m_CollisionCache[i] == null)
                m_CurrentCollisions.Remove(m_CollisionCache[i]);  
        
        m_QueryCollisionRoutine = StartCoroutine(QueryCollisions());
    }

    internal void UpdateBuildStatusPlaneMaterial(Material material)
    {
        m_BuildStatusPlane.GetComponent<Renderer>().material = material;
    }

    internal void InitializeBuild()
    {
        DisableStatusPlane();

        if (m_FoundationStates.Count > 0)
        {
            DisablePlacementObject();
            InitializeFirstStep();
        }
    }

    internal void DisableStatusPlane()
    {
        m_BuildStatusPlane.SetActive(false);
    }

    internal void DisablePlacementObject()
    {
        if(m_PlacementObject != null)
            m_PlacementObject.SetActive(false);
    }

    internal void InitializeFirstStep()
    {
        if (m_FoundationStates.Count > 0)
            m_FoundationStates[0].SetActive(true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BaseUnit>() == null
                && collision.gameObject.layer != LayerMask.NameToLayer("Terrain"))
        {
            m_CurrentCollisions.Add(collision.collider);
            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Registered Trigger Enter Of {1}", gameObject, collision.gameObject));
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.GetComponent<BaseUnit>() == null 
                && collision.gameObject.layer != LayerMask.NameToLayer("Terrain"))
        {
            m_CurrentCollisions.Remove(collision.collider);
            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Registered Trigger Exit Of {1}", gameObject, collision.gameObject));
        }
    }

    internal void DisableNavObstacle()
    {
        if(m_NavObstacle != null)
            m_NavObstacle.enabled = false;
    }

    internal void EnableNavObstacle()
    {
        if (m_NavObstacle != null)
            m_NavObstacle.enabled = true;
    }


    internal Vector3? DetermineApproach(BaseEntity entity)
    {
        m_DestinationRay = new Ray(entity.Origin, Origin - entity.Origin);
        m_DestinationRayHitCount = Physics.SphereCastNonAlloc(m_DestinationRay, 0.25f, m_DestinationRayHits);
        m_DeltaColliders = GetComponents<Collider>();

        if (m_DestinationRayHits != null && m_DestinationRayHitCount > 0)
        {
            for (int i = m_DestinationRayHitCount - 1; i >= 0; i--)
            {
                for (int j = m_DeltaColliders.Length - 1; j >= 0; j--)
                {
                    if (m_DestinationRayHits[i].collider == m_DeltaColliders[j] && !m_DeltaColliders[j].isTrigger)
                    {
                        m_DestinationOffset = (Origin - entity.Origin).normalized;
                        m_DestinationCache = m_DestinationRayHits[i].point - m_DestinationOffset;

                        m_DestinationHitIndex = i;
                        m_ApproachingEntity = entity;

                        return m_DestinationCache;
                    }
                }
            }
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (m_DestinationCache != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_DestinationCache.Value, 0.5f);

            if (m_DestinationHitIndex != -1)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(m_DestinationRayHits[m_DestinationHitIndex].point, 0.5f);
                Gizmos.DrawLine(m_ApproachingEntity.Origin, m_DestinationCache.Value);
            }
        }
    }
}
