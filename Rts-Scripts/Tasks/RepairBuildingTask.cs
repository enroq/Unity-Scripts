using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairBuildingTask : WorkerTask
{
    private BaseBuilding m_TargetBuilding;

    Vector3? m_TaskPositionCache;

    internal BaseBuilding TargetBuilding
    {
        get { return m_TargetBuilding; }
    }

    public RepairBuildingTask(BaseBuilding building)
    {
        SetTargetBuilding(building);
    }

    public Sprite TaskIcon
    {
        get;
        set;
    }

    public TaskStatus TaskStatus { get; set; }

    public int MaxProgressLevel
    {
        get { return m_TargetBuilding.MaxHealth; }
        set { /*Inaccessible, Derived From Max Health*/ }
    }

    public int TaskProgressLevel
    {
        get { return m_TargetBuilding.CurrentHealth; }
        set { m_TargetBuilding.CurrentHealth = value; }
    }

    public int NumberOfWorkersAssigned { get; set; }

    public float ArrivalOffset
    {
        get
        {
            return m_TargetBuilding.ArrivalOffset;
        }
    }

    public float DistanceFromTask(BaseEntity entity)
    {
        return Vector3.Distance
            (TaskPosition(entity), entity.Origin);
    }

    public Vector3 TaskPosition(BaseEntity entity)
    {
        if(m_TaskPositionCache == null)
            m_TaskPositionCache = m_TargetBuilding.
                DetermineApproach(entity).Value;

        return m_TaskPositionCache.Value;
    }

    public void FurtherTaskProgress(int i)
    {
        if (TaskProgressLevel + i <= MaxProgressLevel)
            TaskProgressLevel += i;

        else TaskProgressLevel = MaxProgressLevel;

        UpdateTaskStatus();
    }

    public void UpdateTaskStatus()
    {
        if (TaskProgressLevel < MaxProgressLevel)
            TaskStatus = TaskStatus.Incomplete;

        else 
            if (TaskProgressLevel == MaxProgressLevel)
                TaskStatus = TaskStatus.Completed;
    }

    internal void SetTargetBuilding(BaseBuilding building)
    {
        if(building != null)
            m_TargetBuilding = building;
    }
}
