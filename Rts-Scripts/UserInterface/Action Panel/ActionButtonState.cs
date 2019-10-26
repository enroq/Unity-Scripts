using System.Collections.Generic;
using UnityEngine;

public class ActionButtonState : KaryonBehaviour
{
    public enum StateType
    {
        None,
        Primary,
        Constructions,
        Units           
    }

    [SerializeField]
    private StateType m_StateType = StateType.None;

    [SerializeField]
    private List<ActionButton> m_ActionButtons = new List<ActionButton>();

    [SerializeField]
    private bool m_AutoPopulateButtons = true;

    [SerializeField]
    private string m_ActionStateIdentity = string.Empty;

    public StateType Type
    {
        get { return m_StateType; }
        set { m_StateType = value; }
    }

    public List<ActionButton> ActionButtons
    {
        get { return m_ActionButtons; }
        set { m_ActionButtons = value; }
    }

    public string ActionStateIdentity
    {
        get { return m_ActionStateIdentity; }
    }

    ActionButton[] ChildActionButtons;

    void Start()
    {
        if (m_AutoPopulateButtons)
        {
            ChildActionButtons = GetComponentsInChildren<ActionButton>();
            for (int i = 0; i < ChildActionButtons.Length; i++)
            {
                m_ActionButtons.Add(ChildActionButtons[i]);
            }
        }
    }

    public void InvokeButtonAtIndex(int index)
    {
        for(int i = 0; i < m_ActionButtons.Count; i++)
        {
            if (m_ActionButtons[i].ButtonIndex == index)
                HandleActionRequest(m_ActionButtons[i]);
        }
    }

    public ActionButton GetButtonByIndex(int index)
    {
        for(int i = m_ActionButtons.Count -1; i >= 0; i--)
        {
            if (m_ActionButtons[i].ButtonIndex == index) {
                return m_ActionButtons[i];
            }
        }

        return null;
    }

    public GameObject GetParentObject()
    {
        return gameObject.transform.parent.gameObject;
    }

    public void HandleActionRequest(ActionButton actionButton)
    {
        switch(actionButton.ButtonType)
        {
            case ActionButton.ActionType.Construct:
                {
                    HandleConstructAction(actionButton);
                    break;
                }
            case ActionButton.ActionType.Building:
                {
                    HandleBuildAction(actionButton, GetParentObject());
                    break;
                }
            case ActionButton.ActionType.Unit:
                {
                    HandleTrainUnitAction(actionButton, GetParentObject());
                    break;
                }
            case ActionButton.ActionType.Ability:
                {
                    HandleAbilityAction(actionButton, GetParentObject());
                    break;
                }
            case ActionButton.ActionType.Research:
                {
                    HandleResearchAction(actionButton, GetParentObject());
                    break;
                }
            case ActionButton.ActionType.Upgrade:
                {
                    HandleUpgradeAction(actionButton, GetParentObject());
                    break;
                }
            default:break;
        }
    }

    private void HandleTrainUnitAction(ActionButton actionButton, GameObject parent)
    {
        if (GameEngine.PlayerStateHandler.GetStateByIndex
            (parent.GetComponent<BaseEntity>().Team).IsWithinUnitLimit)
        {
            if (parent.GetComponent<BaseBuilding>() != null)
                parent.GetComponent<BaseBuilding>().AddTaskToQueue
                    (new TrainUnitTask(Instantiate(actionButton.ObjectRelative), parent));

            else
                throw new UnityException("Parent Object Missing Base Building Component.");
        }

        else
        {
            /*Todo: Add Notification For Player When At Unit Limit*/
        }

    }

    private void HandleConstructAction(ActionButton actionButton)
    {
        GameEngine.ActionPanelBehavior
            .EngageSubstateStateByType(StateType.Constructions);
    }

    private void HandleBuildAction(ActionButton actionButton, GameObject parent)
    {
        if(!GameEngine.ConstructionHandler.IsBuilding)
            GameEngine.ConstructionHandler.SetCurrentBuildingQueue
                (new ConstructionTask
                    (Materialize(actionButton.ObjectRelative), parent.GetComponent<BaseEntity>().Team));

        GameEngine.ActionPanelBehavior.EngagePrimaryState();
    }

    private void HandleAbilityAction(ActionButton actionButton, GameObject parent)
    {
        parent.GetComponent<AbilityState>().InitializeActiveAbility
            (actionButton.ObjectRelative.GetComponent<BaseAbility>());
    }

    private void HandleResearchAction(ActionButton actionButton, GameObject parent)
    {
        if (parent.GetComponent<BaseBuilding>() != null)
        {
            if (!actionButton.ObjectRelative.GetComponent<BaseTechnology>().IsBeingResearched)
            {
                parent.GetComponent<BaseBuilding>().AddTaskToQueue
                    (new ResearchTask
                        (actionButton.ObjectRelative.GetComponent<BaseTechnology>(), actionButton));
            }
        }

        else throw new UnityException("Parent Object Missing Base Building Component.");
    }

    private void HandleUpgradeAction(ActionButton actionButton, GameObject parent)
    {
        if(!actionButton.ObjectRelative.GetComponent<ConstructionUpgrade>().UpgradeInProgress
            && parent.GetComponent<BaseBuilding>().CanLevelUp)
        {
            parent.GetComponent<BaseBuilding>().AddTaskToQueue
                (new BuildingUpgradeTask(parent.GetComponent<BaseBuilding>(), 
                    actionButton.ObjectRelative.GetComponent<ConstructionUpgrade>(), actionButton));
        }
    }
}
