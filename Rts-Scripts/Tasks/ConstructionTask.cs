using System;
using UnityEngine;

public class ConstructionTask : WorkerTask
{
    BaseBuilding m_BuildingRelative;

    Vector3? m_TaskPositionCache;

    internal BaseBuilding TargetBuilding
    {
        get { return m_BuildingRelative; }
    }

    public TaskStatus TaskStatus { get; set; }

    public Sprite TaskIcon
    {
        get
        {
            return m_BuildingRelative.GetComponent<BaseEntity>().EntityIcon;
        }
    }

    public int TaskProgressLevel
    {
        get { return m_BuildingRelative.CurrentHealth; }
        set 
        { 
            m_BuildingRelative.CurrentHealth = value;
            m_BuildingRelative.SyncFoundationHealth();
        }
    }

    public int MaxProgressLevel
    {
        get { return m_BuildingRelative.MaxHealth; }
        set { /*Inaccessible, Derived From Max Health*/ }
    }

    public SelectableObject Selectable
    {
        get { return m_BuildingRelative.CurrentFoundationObject.GetComponent<SelectableObject>(); }
    }

    public Sprite BuildingSprite
    {
        get { return m_BuildingRelative.EntityIcon; }
    }

    public int NumberOfWorkersAssigned { get; set; }

    public ConstructionTask(GameObject buildingObject, int teamHash)
    {
        buildingObject.SetActive(false);

        m_BuildingRelative 
            = buildingObject.GetComponent<BaseBuilding>();
        m_BuildingRelative.CurrentHealth = 0;
        m_BuildingRelative.ConstructionTask = this;
        m_BuildingRelative.Team = teamHash;

        GameEngine.PlayerStateHandler.
            GetStateByIndex(teamHash).AddBuildingToState(m_BuildingRelative);
    }

    internal void CancelTask()
    {
        m_BuildingRelative.CancelConstruction();
        m_BuildingRelative = null;
    }

    public void FurtherTaskProgress(int i)
    {
        if (TaskProgressLevel + i <= MaxProgressLevel)
            TaskProgressLevel += i;

        else TaskProgressLevel = MaxProgressLevel;

        UpdateTaskStatus();
        UpdateBuildingFoundation();

        if (TaskStatus == TaskStatus.Completed)
            FinishBuildProgress();
    }

    public void UpdateTaskStatus()
    {
        if (TaskProgressLevel < MaxProgressLevel)
            TaskStatus = TaskStatus.Incomplete;

        else if (TaskProgressLevel == MaxProgressLevel)
            TaskStatus = TaskStatus.Completed;
    }

    void UpdateBuildingFoundation()
    {
        m_BuildingRelative.UpdateFoundationStep();
    }

    public float DistanceFromTask(BaseEntity entity)
    {
        return Vector3.Distance(TaskPosition(entity), entity.Origin);
    }

    public Vector3 TaskPosition(BaseEntity entity)
    {
        if(m_TaskPositionCache == null)
            m_TaskPositionCache = m_BuildingRelative.CurrentFoundationObject.GetComponent
                <FoundationBehavior>().DetermineApproach(entity).Value;
        return
            m_TaskPositionCache.Value;
    }

    public float ArrivalOffset
    {
        get { return m_BuildingRelative.ArrivalOffset; }
    }

    internal void InitializeBuild()
    {
        m_BuildingRelative.InitializeBuild();
    }

    internal void EnableFoundationObstacle()
    {
        m_BuildingRelative.EnableFoundationObstacle();
    }

    internal bool WithoutCollision
    {
        get { return m_BuildingRelative.WithoutCollision; }
    }

    internal void UpdateBuildStatusPlaneMaterial(Material material)
    {
        m_BuildingRelative.UpdateBuildStatusPlaneMaterial(material);
    }

    internal void FinishBuildProgress()
    {
        m_BuildingRelative.CompleteConstructionPhase();
    }

    internal void InstantiateFoundation(Vector3 vector)
    {
        m_BuildingRelative.InstantiateBuildingFoundation(vector);
    }

    internal void UpdateFoundationPosition(Vector3 vector)
    {
        m_BuildingRelative.UpdateFoundationPosition(vector);
    }
}
