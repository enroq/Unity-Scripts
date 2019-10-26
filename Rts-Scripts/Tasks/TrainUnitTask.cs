using UnityEngine;

public class TrainUnitTask : ITaskable
{
    private int m_TeamHash;
    private BaseUnit m_UnitCache;
    private GameObject m_UnitObject;
    
    internal GameObject UnitObject
    {
        get { return m_UnitObject; }
    }

    internal BaseUnit UnitCache
    {
        get { return m_UnitCache; }
    }

    public Sprite TaskIcon { get { return m_UnitCache.EntityIcon; } }

    public TaskStatus TaskStatus { get; set; }
    public int TaskProgressLevel { get; set; }
    public int MaxProgressLevel  { get; set; }
        
    public TrainUnitTask(GameObject unit, GameObject parent)
    {
        m_UnitObject = unit;
        m_UnitObject.SetActive(false);

        m_UnitObject.transform.position = parent.transform.position;

        m_UnitCache = m_UnitObject.GetComponent<BaseUnit>();

        MaxProgressLevel = m_UnitCache.UnitTrainTime;

        m_TeamHash = parent.GetComponent<BaseBuilding>().Team;
        m_UnitCache.Team = m_TeamHash;
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

        else if (TaskProgressLevel == MaxProgressLevel)
            TaskStatus = TaskStatus.Completed;
    }
}
