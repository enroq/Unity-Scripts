using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class HarvesterState : KaryonBehaviour
{
    [SerializeField]
    bool m_DebugMode;
    [SerializeField]
    float m_HarvestSpeed = 1.0f;
    [SerializeField]
    int m_HarvestCapacity = 8;
    [SerializeField]
    int m_AmountPerHarvest = 1;
    [SerializeField]
    float m_HarvestRange = 2.0f;
    [SerializeField]
    float m_ArrivalOffsetDistance = 2.5f;
    [SerializeField]
    float m_NodeRecognitionRange = 25.0f;
    [SerializeField]
    float m_RecognitionSpeed = 1.0f;
    [SerializeField]
    LayerMask m_ResourceNodeLayer;
    [SerializeField]
    int m_CurrentResourceLoad;
    [SerializeField]
    GameObject m_ResourcePrefab;

    private float m_CurrentHarvestDelay;
    private float m_CurrentRecognitionDelay;
    private float m_RangeBuffer = 1.0f;
    private float m_TurnSpeed = 8.5f;
    private float m_DistanceToCapitol;

    private bool m_NavigatingToNode;
    private bool m_ReturningToCapitol;
    private bool m_InRangeOfNode;

    private Vector3 m_PositionRelativeToTarget;
    private Vector3 m_DeltaRotation;
    private Vector3 m_TargetPositionCache;

    private ResourceNode m_TargetNode;
    private ResourceNode m_DeltaNode;
    private Collider m_TargetCollider;
    private BaseEntity m_EntityRelative;
    private PlayerState m_PlayerRelative;

    BaseCapitol m_ClosestCapitolBuilding;
    List<GameObject> m_PlayerCapitols = new List<GameObject>();

    Coroutine m_ReturnResourceRoutine;

    Collider[] m_ColliderSearchCache;
    List<Collider> m_ValidResourceColliders = new List<Collider>();

    RaycastHit[] m_FeelerCache; Ray m_FeelerRay = new Ray();

    private void Start()
    {
        if ((m_EntityRelative = gameObject.GetComponent<BaseEntity>()) == null)
            throw new MissingComponentException("Harvester State Attached To Object Without Base Entity Component.");

        m_CurrentResourceLoad = 0;

        ResetHarvestDelay();
        GameEngine.HarvestHandler.AttachState(this);

        m_PlayerRelative = GameEngine.
            PlayerStateHandler.GetStateByIndex(m_EntityRelative.Team);
    }

    internal GameObject FindClosestCapitol()
    {
        m_PlayerCapitols.Clear();
        for(int i = m_PlayerRelative.CurrentStructures.Length -1; i >= 0; i--)
            if (m_PlayerRelative.CurrentStructures[i] is BaseCapitol)
                m_PlayerCapitols.Add(m_PlayerRelative.CurrentStructures[i].gameObject);

        if (m_PlayerCapitols.Count == 1)
            return m_PlayerCapitols[0];

        if(m_PlayerCapitols.Count > 1)
            return GetClosestGameObject(m_PlayerCapitols.ToArray(), transform.position);

        return null;
    }

    internal bool ShouldSeekResources
    {
        get
        {
            switch (m_EntityRelative.CurrentCommand)
            {
                case CommandType.None: return true;

                default:
                    return false;
            }
        }
    }

    internal void SetTargetNode(ResourceNode node, Collider collider)
    {
        m_TargetNode = node;
        m_TargetCollider = collider;

        m_TargetPositionCache = m_TargetCollider.transform.position;

        m_TargetNode.AttachWorker();
    }

    internal void ClearTargetNode()
    {
        ClearNavState();
        m_TargetNode.DetatchWorker();

        m_TargetNode = null;
        m_TargetCollider = null;
        m_TargetPositionCache = Vector3.zero;

        StopAllCoroutines();
    }

    internal void ClearNavState()
    {
        m_ReturningToCapitol = false;
        m_NavigatingToNode = false;
    }

    void ResetHarvestDelay()
    {
        m_CurrentHarvestDelay = m_HarvestSpeed;
    }

    internal void UpdateHarvesterState(float deltaTime)
    {
        if (m_TargetNode != null && m_NavigatingToNode)
            TurnTowardsResource(deltaTime);
    }

    internal void ProcessHarvesterState(float interval)
    {
        if (!m_ReturningToCapitol && ShouldSeekResources)
        {
            ProcessRecognitionCooldown(interval);
            ProcessHarvestCooldown(interval);
        }
    }

    internal void ProcessHarvestCooldown(float interval)
    {
        if (m_CurrentHarvestDelay - interval > 0)
            m_CurrentHarvestDelay -= interval;

        else
        {
            ProcessHarvest();
        }
    }

    internal void ProcessRecognitionCooldown(float interval)
    {
        if (m_CurrentRecognitionDelay - interval > 0)
            m_CurrentHarvestDelay -= interval;

        else
        {
            QueryCurrentTargetNode();
        }
    }

    internal void QueryCurrentTargetNode()
    {
        if (m_TargetNode == null && ShouldSeekResources)
            FindClosestResourceNode();

        else
        {
            QueryGatherState();
        }
    }

    internal void QueryGatherState()
    {
        if (m_TargetNode == null)
            return;

        if (!IsInStopRange())
        {
            if (!m_NavigatingToNode)
            {
                m_NavigatingToNode = true;
                ((BaseWorker)m_EntityRelative).GoToPosition
                    (m_TargetNode.DetermineApproach(m_EntityRelative).Value);
            }
        }

        else if (m_NavigatingToNode && IsInStopRange())
        {
            m_NavigatingToNode = false;
            ((BaseWorker)m_EntityRelative).HaltNavigation();
        }
    }

    internal void ProcessHarvest()
    {
        if (m_TargetNode == null || !ShouldSeekResources || m_NavigatingToNode)
            return;

        if (m_TargetNode.CurrentResourceValue <= 0)
            ClearTargetNode();

        if(IsInHarvestRange())
        {
            HarvestFromNode();
            ResetHarvestDelay();
        }
    }

    internal void HarvestFromNode()
    {
        if (m_TargetNode != null)
        {
            m_EntityRelative.PlayHarvestAnimation();
            m_EntityRelative.PlayHarvestSound();
            m_TargetNode.ExtractResource(m_AmountPerHarvest);
            m_CurrentResourceLoad += m_AmountPerHarvest;
        }

        if(m_CurrentResourceLoad >= m_HarvestCapacity)
        {
            if (m_CurrentResourceLoad > m_HarvestCapacity)
                m_CurrentResourceLoad = m_HarvestCapacity;

            if (m_ResourcePrefab != null)
                m_ResourcePrefab.SetActive(true);

            ReturnToCapitol();
        }
    }

    internal void ReturnToCapitol()
    {
        if (FindClosestCapitol() == null)
            return;

        m_ClosestCapitolBuilding 
            = FindClosestCapitol().GetComponent<BaseCapitol>();

        if (m_ClosestCapitolBuilding != null)
        {
            m_ReturningToCapitol = true;
            m_NavigatingToNode = false;
            ((BaseUnit)m_EntityRelative).GoToPosition
                (m_ClosestCapitolBuilding.DetermineApproach(m_EntityRelative).Value);

            m_ReturnResourceRoutine = StartCoroutine(AwaitReturnToCapitol());
        }
    }

    IEnumerator AwaitReturnToCapitol()
    {
        yield return new WaitUntil(() => InRangeOfCapitol());

        GameEngine.ResourceManager.StoreResource
            (m_EntityRelative.Team, m_TargetNode.ResourceType, m_CurrentResourceLoad);

        m_CurrentResourceLoad = 0;
        m_ReturningToCapitol = false;

        if (m_ResourcePrefab != null)
            m_ResourcePrefab.SetActive(false);
    }

    internal bool InRangeOfCapitol()
    {
        if (m_ClosestCapitolBuilding != null)
        {
            m_DistanceToCapitol =
                (m_ClosestCapitolBuilding.transform.position - transform.position).sqrMagnitude;
            return (m_DistanceToCapitol <= Mathf.Pow
                ((m_ClosestCapitolBuilding.ArrivalOffset + m_ArrivalOffsetDistance), 2));
        }

        return false;
    }

    internal bool IsInStopRange()
    {
        if (m_TargetNode != null)
        {
            m_FeelerRay.direction = transform.forward;
            m_FeelerRay.origin = transform.position;
            m_FeelerCache = Physics.SphereCastAll
                (m_FeelerRay, 0.5f, m_HarvestRange * 1.5f, m_ResourceNodeLayer);

            for (int i = m_FeelerCache.Length - 1; i >= 0; i--)
            {
                if (m_FeelerCache[i].collider == m_TargetCollider)
                {
                    if (m_FeelerCache[i].distance <= m_HarvestRange - m_RangeBuffer)
                        return true;
                }
            }
        }

        return false;
    }

    internal bool IsInHarvestRange()
    {
        if (m_TargetNode != null)
        {
            m_FeelerRay.direction = transform.forward;
            m_FeelerRay.origin = transform.position;
            m_FeelerCache = Physics.SphereCastAll
                (m_FeelerRay, 0.5f, m_HarvestRange * 1.5f, m_ResourceNodeLayer);

            for (int i = m_FeelerCache.Length - 1; i >= 0; i--)
            {
                if (m_FeelerCache[i].collider == m_TargetCollider)
                {
                    if (m_FeelerCache[i].distance <= m_HarvestRange)
                    {
                        m_InRangeOfNode = true;
                        return true;
                    }
                }
            }
        }

        m_InRangeOfNode = false;
        return false;
    }

    internal void FindClosestResourceNode()
    {
        m_ColliderSearchCache = Physics.OverlapSphere
            (transform.position, m_NodeRecognitionRange, m_ResourceNodeLayer);

        m_ValidResourceColliders.Clear();
        for(int i = m_ColliderSearchCache.Length -1; i >= 0; i--)
        {
            if((m_DeltaNode = m_ColliderSearchCache[i].GetComponent<ResourceNode>()) != null)
            {
                if (m_DeltaNode.CanAccomodateNewWorker())
                {
                    if(GameEngine.DebugMode || m_DebugMode)
                        Debug.LogFormat("Resource Node Can Accomodate Worker: {0}/{1}",
                            m_DeltaNode.CurrentWorkersAttached, m_DeltaNode.MaxWorkerCapacity);

                    m_ValidResourceColliders.Add(m_ColliderSearchCache[i]);
                }
            }
        }

        if (m_ColliderSearchCache.Length > 0)
        {
            m_TargetCollider = GetClosestCollider
                (m_ValidResourceColliders.ToArray(), transform.position);

            SetTargetNode(m_TargetCollider.GetComponent<ResourceNode>(), m_TargetCollider);
        }
    }

    internal void TurnTowardsResource(float deltaTime)
    {
        if (m_TargetCollider != null && ShouldSeekResources && (m_NavigatingToNode || m_InRangeOfNode))
        {
            m_PositionRelativeToTarget = m_TargetCollider.transform.position - transform.position;
            m_DeltaRotation = Quaternion.LookRotation(m_PositionRelativeToTarget).eulerAngles;
            m_DeltaRotation.x = 0;
            transform.rotation = Quaternion.Slerp
                (transform.rotation, Quaternion.Euler(m_DeltaRotation), deltaTime * m_TurnSpeed);
        }
    }
}
