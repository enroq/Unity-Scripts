using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUpgradeTask : ITaskable
{
    BaseBuilding m_ParentBuilding;
    ActionButton m_CorrespondingButton;
    ConstructionUpgrade m_Upgrade;

    public ConstructionUpgrade TechComponent
    {
        get { return m_Upgrade; }
    }

    public Sprite TaskIcon
    {
        get { return m_CorrespondingButton.ButtonImage; }
    }

    public TaskStatus TaskStatus { get; set; }
    public int TaskProgressLevel { get; set; }
    public int MaxProgressLevel { get; set; }

    public BuildingUpgradeTask(BaseBuilding parent, ConstructionUpgrade upgrade, ActionButton button)
    {
        m_CorrespondingButton = button;
        m_CorrespondingButton.DisableButton();

        m_ParentBuilding = parent;
        m_Upgrade = upgrade;

        MaxProgressLevel = m_Upgrade.UpgradeTime;
        m_Upgrade.UpgradeInProgress = true;
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
        {
            TaskStatus = TaskStatus.Completed;
            m_Upgrade.UpgradeInProgress = false;

            if (m_ParentBuilding.CanLevelUp)
                m_CorrespondingButton.EnableButton();
        }
    }
}
