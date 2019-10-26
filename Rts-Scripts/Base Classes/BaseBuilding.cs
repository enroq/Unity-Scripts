using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

#pragma warning disable
public class BaseBuilding : BaseEntity, ITasker
{
    [SerializeField]
    private GameObject m_FoundationPrefab;
    [SerializeField]
    private GameObject m_SpawnPositionObject;
    [SerializeField]
    private GameObject m_RallyPointObject;
    [SerializeField]
    private bool m_HasRallyPoint = false;
    [SerializeField]
    private float m_ArrivalOffset = 2.5f;
    [SerializeField]
    private float m_BuildHeightOffset = 0f;
    [SerializeField]
    private bool m_IsControlBuilding;
    [SerializeField]
    private float m_ControlRange;
    [SerializeField]
    private int m_BuildingLimit = 0;
    [SerializeField]
    private GameObject m_InitialState = null;
    [SerializeField]
    private List<ConstructionUpgrade> m_UpgradeStates;

    public Coroutine TaskRoutine { get; set; }

    public ITaskable CurrentTask { get; set; }

    public Queue<ITaskable> TaskQueue { get; set; }

    private BaseUnit m_DeltaUnit;
    private Vector3 m_DeltaPosition;
    private Vector3 m_UnitRallyPoint;
    private BaseEntity m_RallyEntity;
    private GameObject m_CurrentFoundationObject;
    private FoundationBehavior m_Foundation;
    private WorkerTask m_ConstructionTask;

    private int m_CurrentUpgradeLevel = 0;
    private ConstructionUpgrade m_UpgradeDelta = null;

    Vector3? m_DestinationCache = Vector3.zero;
    Vector3 m_DestinationOffset = Vector3.zero;

    Ray m_DestinationRay;
    RaycastHit[] m_DestinationRayHits = new RaycastHit[128];

    Collider[] m_DeltaColliders;

    int m_DestinationHitIndex = -1;
    int m_DestinationRayHitCount = -1;
    BaseEntity m_ApproachingEntity;

    internal bool CanLevelUp
    {
        get { return m_CurrentUpgradeLevel +1 <= MaxUpgradeLevel; }
    }

    internal int MaxUpgradeLevel
    {
        get { return m_UpgradeStates.Count; }
    }

    internal GameObject CurrentFoundationObject
    {
        get { return m_CurrentFoundationObject; }
    }

    internal bool WithoutCollision
    {
        get { return m_Foundation.WithoutCollision; }
    }

    internal WorkerTask ConstructionTask
    {
        get { return m_ConstructionTask; }
        set { m_ConstructionTask = value; }
    }

    internal GameObject FoundationPrefab
    {
        get { return m_FoundationPrefab; }
        set { m_FoundationPrefab = value; }
    }

    internal GameObject SpawnPositionObject
    {
        get { return m_SpawnPositionObject; }
        set { m_SpawnPositionObject = value; }
    }

    internal float ArrivalOffset
    {
        get { return m_ArrivalOffset; }
        set { m_ArrivalOffset = value; }
    }

    internal bool IsControlBuilding
    {
        get { return m_IsControlBuilding; }
        set { m_IsControlBuilding = value; }
    }

    public override void Start()
    {
        if (FoundationPrefab == null)
            throw new MissingComponentException("Base Building Missing Foundation Prefab.");

        TaskQueue = new Queue<ITaskable>();

        if(Team != -1)
            GameEngine.PlayerStateHandler.
                GetStateByIndex(Team).AddBuildingToState(this);
	}

    internal override void OnDeath()
    {
        GameEngine.PlayerStateHandler.GetStateByIndex
            (Team).RemoveBuildingFromState(this);

        base.OnDeath();
    }

    internal override void OnSelect()
    {
        if(m_RallyPointObject != null)
            m_RallyPointObject.SetActive(true);
    }

    internal override void OnDeselect()
    {
        if (m_RallyPointObject != null)
            m_RallyPointObject.SetActive(false);
    }

    internal void Upgrade()
    {
        if (CanLevelUp)
            m_CurrentUpgradeLevel++;
    }

    internal void SetCurrentTask(ITaskable task)
    {
        CurrentTask = task;
    }

    internal void SetRallyPoint(Vector3 rallyPoint)
    {
        m_HasRallyPoint = true;
        m_UnitRallyPoint = rallyPoint;

        if (m_RallyPointObject != null)
        {
            m_RallyPointObject.transform.position = m_UnitRallyPoint;
        }
    }

    internal void SetRallyEntity(BaseEntity entity)
    {
        m_RallyEntity = entity;
    }

    internal void SyncFoundationHealth()
    {
        if (m_Foundation != null)
            m_Foundation.CurrentHealth = CurrentHealth;
    }

    internal void UpdateFoundationStep()
    {
        m_Foundation.ProcessCurrentStep();
    }

    internal void EnableFoundationObstacle()
    {
        m_Foundation.EnableNavObstacle();
    }

    internal void DisableFoundationObstacle()
    {
        m_Foundation.DisableNavObstacle();
    }

    internal void AddTaskToQueue(ITaskable task)
    {
        if(CurrentTask == null)
        {
            SetCurrentTask(task);
            TaskRoutine = StartCoroutine(HandleTaskProgression());

            if (GameEngine.SelectionHandler.IsFocusObject(gameObject))
            {
                GameEngine.InfoBoxBehavior.SetTaskFocus(CurrentTask);
                GameEngine.InfoBoxBehavior.UpdateTaskQueueIcons(TaskQueue.ToArray());
            }
        }

        else if(TaskQueue.Count < 7)
        {
            TaskQueue.Enqueue(task);
            if (GameEngine.SelectionHandler.IsFocusObject(gameObject))
                GameEngine.InfoBoxBehavior.UpdateTaskQueueIcons(TaskQueue.ToArray());
        }
    }

    IEnumerator HandleTaskProgression()
    {
        yield return new WaitForSeconds(1.0f);

        CurrentTask.FurtherTaskProgress(1);

        if (CurrentTask.TaskStatus == TaskStatus.Completed)
            HandleTaskCompletion();
        else
            TaskRoutine = StartCoroutine(HandleTaskProgression());
    }

    void HandleTaskCompletion()
    {
        if(CurrentTask is TrainUnitTask)
        {
            HandleUnitCompletion((TrainUnitTask)CurrentTask);
            CurrentTask = null;
        }

        else if(CurrentTask is ResearchTask)
        {
            HandleResearchTaskCompletion((ResearchTask)CurrentTask);
            CurrentTask = null;
        }

        else if(CurrentTask is BuildingUpgradeTask)
        {
            HandleUpgradeCompletion();
            CurrentTask = null;
        }

        if(CurrentTask == null && TaskQueue.Count > 0)
        {
            CurrentTask = TaskQueue.Dequeue();
            TaskRoutine = StartCoroutine(HandleTaskProgression());

            if (GameEngine.SelectionHandler.IsFocusObject(gameObject))
            {
                GameEngine.InfoBoxBehavior.SetTaskFocus(CurrentTask);
                GameEngine.InfoBoxBehavior.UpdateTaskQueueIcons(TaskQueue.ToArray());
            }
        }
    }

    void HandleResearchTaskCompletion(ResearchTask research)
    {
        GameEngine.ResearchHandler.UpgradeTechState
            (research.TechComponent.CorrespondingEntity, research.TechComponent.TechName);
    }

    void HandleUnitCompletion(TrainUnitTask unitTask)
    {
        unitTask.UnitObject.transform.position = SpawnPositionObject.transform.position;
        unitTask.UnitObject.transform.rotation = SpawnPositionObject.transform.rotation;
        unitTask.UnitObject.SetActive(true);

        if ((m_DeltaUnit = unitTask.UnitObject.GetComponent<BaseUnit>()) != null)
        {
            GameEngine.ResearchHandler.SyncTechState(m_DeltaUnit);

            if (m_DeltaUnit.UnitType == UnitType.Air)
            {
                m_DeltaPosition = SpawnPositionObject.transform.position;
                m_DeltaPosition.y = GameEngine.AirUnitPlane.transform.position.y;
                unitTask.UnitObject.transform.position = m_DeltaPosition;

                m_DeltaUnit.GetComponent<NavMeshAgent>().enabled = true;
            }

            m_DeltaUnit.PlayInitializationSound();

            if (m_HasRallyPoint)
                StartCoroutine(DelayedGoToRallyPoint(m_DeltaUnit));
        }
    }

    void HandleUpgradeCompletion()
    {
        if (m_CurrentUpgradeLevel == 0)
        {
            if (m_InitialState == null)
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            else
                m_InitialState.SetActive(false);
        }

        if (m_UpgradeDelta != null)
            m_UpgradeDelta.DisableUpgradeObject();

        Upgrade();

        m_UpgradeDelta = m_UpgradeStates[m_CurrentUpgradeLevel -1];
        m_UpgradeDelta.EnableUpgradeObject();
        m_UpgradeDelta.SyncUpgrade(this);

        if (gameObject.GetComponent<SelectableObject>().IsSelected)
        {
            GameEngine.SelectionHandler.DeselectObject
                (gameObject.GetComponent<SelectableObject>());

            StartCoroutine(DelayedSelection());
        }
    }

    IEnumerator DelayedSelection()
    {
        yield return new WaitForSeconds(0.15f);

        GameEngine.SelectionHandler.SelectObject
            (gameObject.GetComponent<SelectableObject>());
    }

    IEnumerator DelayedGoToRallyPoint(BaseUnit unit)
    {
        yield return new 
            WaitUntil(() => unit.NavHandlerInitialized);

        unit.GoToPosition(m_RallyPointObject.transform.position);
    }

    internal void InitializeFoundation()
    {
        if ((m_Foundation = m_CurrentFoundationObject.GetComponent<FoundationBehavior>()) == null)
            throw new MissingComponentException("Building Foundation Missing Collision Handler Component.");

        SyncFoundation();
    }

    internal void SyncFoundation()
    {
        m_Foundation.ParentBuilding = this;
        m_Foundation.Team = Team;
        m_Foundation.CurrentHealth = CurrentHealth;
        m_Foundation.MaxHealth = MaxHealth;
    }

    internal void InitializeBuild()
    {
        m_Foundation.InitializeBuild();
    }

    internal void UpdateBuildStatusPlaneMaterial(Material material)
    {
        m_Foundation.UpdateBuildStatusPlaneMaterial(material);
    }

    internal void CancelConstruction()
    {
        Destroy(m_CurrentFoundationObject);
        Destroy(gameObject);
    }

    internal void CompleteConstructionPhase()
    {
        if (m_CurrentFoundationObject != null)
        {
            m_CurrentFoundationObject.SetActive(false);
            ActivateBuildingObject
                (m_CurrentFoundationObject.transform.position);
            PlayInitializationSound();

            ConstructionTask = null;

            if (m_CurrentFoundationObject.GetComponent<SelectableObject>().IsSelected)
            {
                if (gameObject.GetComponent<SelectableObject>() != null)
                    GameEngine.SelectionHandler.HandleObjectSelection
                        (gameObject.GetComponent<SelectableObject>(), false);
            }

            Destroy(m_CurrentFoundationObject);
        }
    }

    internal void InstantiateBuildingFoundation(Vector3 vector)
    {
        m_CurrentFoundationObject = Instantiate
            (FoundationPrefab, vector, FoundationPrefab.transform.rotation);
        m_CurrentFoundationObject.SetActive(false);

        InitializeFoundation();
    }

    internal void ActivateBuildingObject(Vector3 vector)
    {
        vector.y += m_BuildHeightOffset;
        gameObject.transform.position = vector;
        gameObject.SetActive(true);
    }

    internal void UpdateFoundationPosition(Vector3 vector)
    {
        if (m_CurrentFoundationObject != null)
        { 
            if(!m_CurrentFoundationObject.activeInHierarchy)
                m_CurrentFoundationObject.SetActive(true);

            vector.y += m_BuildHeightOffset;

            m_CurrentFoundationObject.transform.position = vector;
        }
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
                Gizmos.DrawLine(m_ApproachingEntity.Origin, m_DestinationRayHits[m_DestinationHitIndex].point);
            }
        }
    }
}
