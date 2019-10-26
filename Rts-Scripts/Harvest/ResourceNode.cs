using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class ResourceNode : MonoBehaviour
{
    [SerializeField]
    int m_MaxResourceValue = 3000;
    [SerializeField]
    ResourceType m_ResourceType = ResourceType.None;
    [SerializeField]
    bool m_IsExhuastable = true;
    [SerializeField]
    int m_MaxWorkerCapacity = 4;

    int m_CurrentResourceValue;
    int m_CurrentWorkersAttached;

    Vector3? m_DestinationCache = Vector3.zero;
    Vector3 m_DestinationOffset = Vector3.zero;

    Ray m_DestinationRay;
    RaycastHit[] m_DestinationRayHits = new RaycastHit[128];

    Collider[] m_DeltaColliders;

    int m_DestinationHitIndex = -1;
    int m_DestinationRayHitCount = -1;

    BaseEntity m_ApproachingEntity;

    public int CurrentResourceValue { get { return m_CurrentResourceValue; } }
    public ResourceType ResourceType { get { return m_ResourceType; } }

    public int CurrentWorkersAttached { get { return m_CurrentWorkersAttached; } }
    public int MaxWorkerCapacity { get { return m_MaxWorkerCapacity; } }

    private void Start()
    {
        m_CurrentResourceValue = m_MaxResourceValue;
        if (gameObject.layer != GameEngine.HarvestHandler.ResourceLayer)
            throw new UnityException("Resource Node Component Attached To Object Whose Layer Is Not Resource.");
    }

    internal void DetatchWorker()
    {
        m_CurrentWorkersAttached--;
    }

    internal void AttachWorker()
    {
        m_CurrentWorkersAttached++;
    }

    internal bool CanAccomodateNewWorker()
    {
        return m_CurrentWorkersAttached + 1 <= m_MaxWorkerCapacity;
    }

    internal void ExtractResource(int amt)
    {
        if (m_IsExhuastable)
        {
            if (m_CurrentResourceValue - amt > 0)
                m_CurrentResourceValue -= amt;
            else
                ExhaustResource();
        }
    }

    private void ExhaustResource()
    {
        gameObject.SetActive(false);
    }

    internal Vector3? DetermineApproach(BaseEntity entity)
    {
        m_DestinationRay = new Ray(entity.Origin, transform.position - entity.Origin);
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
                        m_DestinationOffset = (transform.position - entity.Origin).normalized;
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
}
