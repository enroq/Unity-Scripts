using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchTask : ITaskable
{
    BaseTechnology m_BaseTech;
    ActionButton m_CorrespondingButton;

    public BaseTechnology TechComponent
    {
        get { return m_BaseTech; }
    }

    public Sprite TaskIcon
    {
        get { return m_CorrespondingButton.ButtonImage; }
    }

    public TaskStatus TaskStatus { get; set; }
    public int TaskProgressLevel { get; set; }
    public int MaxProgressLevel  { get; set; }

    public ResearchTask(BaseTechnology tech, ActionButton button)
    {
        m_CorrespondingButton = button;
        m_CorrespondingButton.DisableButton();

        m_BaseTech = tech;
        m_BaseTech.IsBeingResearched = true;
        MaxProgressLevel = m_BaseTech.ResearchTime;
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
            m_BaseTech.IsBeingResearched = false;

            if (m_BaseTech.CanBeUpgraded)
                m_CorrespondingButton.EnableButton();
        }
    }
}
