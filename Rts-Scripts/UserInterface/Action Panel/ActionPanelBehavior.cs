using System.Collections.Generic;
using UnityEngine;

public class ActionPanelBehavior : MonoBehaviour
{
    public List<UnityEngine.UI.Button> 
        m_PanelButtons = new List<UnityEngine.UI.Button>();

    public delegate void ActionButtonEventHandler(ActionButtonEventArgs args);
    public static event ActionButtonEventHandler ButtonAction;

    ActionButtonEventArgs m_ButtonArgs = new ActionButtonEventArgs(-1);

    private ActionButtonState m_CurrentActionState;
    private ButtonStateHandler m_CurrentStateHandler;
    private ButtonStateHandler m_MultiInvokeHandlerCache;

    Dictionary<int, ActionButton> 
        m_ActionButtonsCache = new Dictionary<int, ActionButton>();

    private Sprite m_SpriteCache;

    public static void InvokeActionButtonEvent(ActionButtonEventArgs args)
    {
        if (ButtonAction != null)
            ButtonAction(args);
    }

    internal List<SelectableObject> SelectedObjects
    {
       get { return GameEngine.SelectionHandler.SelectedObjects; }
    }

    void Awake()
    {
        GameEngine.AttachActionPanel(gameObject);

        m_SpriteCache = m_PanelButtons[0].image.sprite;
        ButtonAction += ActionPanelBehavior_ButtonAction;
    }

    void Start()
    {
        for(int i = 0; i < m_PanelButtons.Count; i++)
        {
            int index = i;

            m_PanelButtons[index].GetComponentInChildren<UnityEngine.UI.Text>().text = string.Empty;
            m_PanelButtons[index].onClick.AddListener(() => OnActionButtonClick(index));
        }      
    }

    public void SetCurrentHandler(ButtonStateHandler handler)
    {
        if(handler != null)
        {
            m_CurrentStateHandler = handler;
            SetCurrentState
                (m_CurrentStateHandler.PrimaryState);
        }
    }

    public void EngagePrimaryState()
    {
        if (m_CurrentStateHandler != null)
            SetCurrentState(m_CurrentStateHandler.PrimaryState);
    }

    public void EngageSubstateStateByType(ActionButtonState.StateType type)
    {
        if (m_CurrentStateHandler != null)
        {
            if (m_CurrentStateHandler.GetInnerStateByType(type) != null)
                SetCurrentState(m_CurrentStateHandler.GetInnerStateByType(type));
        }
    }

    public void SetCurrentState(ActionButtonState state)
    {
        if (state != null)
        {
            m_CurrentActionState = state;
            m_ActionButtonsCache.Clear();

            for (int i = 0; i < m_CurrentActionState.ActionButtons.Count; i++)
            {
                m_ActionButtonsCache.Add
                    (m_CurrentActionState.ActionButtons[i].ButtonIndex, 
                        m_CurrentActionState.ActionButtons[i]);
            }

            for(int i = 0; i < m_PanelButtons.Count; i++)
            {
                if (m_ActionButtonsCache.ContainsKey(i))
                {
                    m_PanelButtons[i].image.sprite = m_ActionButtonsCache[i].ButtonImage;
                    m_PanelButtons[i].interactable = true;

                    if(m_ActionButtonsCache[i].IsResearchButton)
                    {
                        if (m_ActionButtonsCache[i].TechComponent.IsBeingResearched
                            || !m_ActionButtonsCache[i].TechComponent.CanBeUpgraded)
                        {
                            m_PanelButtons[i].interactable = false;
                        }
                    }

                    else if(m_ActionButtonsCache[i].IsUpgradeButton)
                    {
                        if (m_ActionButtonsCache[i].Upgrade.UpgradeInProgress
                            || !m_ActionButtonsCache[i].Upgrade.CanBeUpgraded)
                        {
                            m_PanelButtons[i].interactable = false;
                        }
                    }
                }

                else
                {
                    m_PanelButtons[i].image.sprite = m_SpriteCache;
                    m_PanelButtons[i].interactable = false;
                }
            }
        }
    }

    public void ResetCurrentState()
    {
        m_CurrentActionState = null;

        for (int i = 0; i < m_PanelButtons.Count; i++)
        {
            m_PanelButtons[i].image.sprite = m_SpriteCache;
            m_PanelButtons[i].interactable = true;
        }
    }

    internal void OnActionButtonClick(int index)
    {
        m_ButtonArgs.ButtonIndex = index;
        InvokeActionButtonEvent(m_ButtonArgs);
    } 

    private void ActionPanelBehavior_ButtonAction(ActionButtonEventArgs args)
    {
        if(GameEngine.DebugMode)
            Debug.Log(string.Format("Action Button Clicked: ({0})", args.ButtonIndex));

        if (m_CurrentActionState != null)
            m_CurrentActionState.InvokeButtonAtIndex(args.ButtonIndex);

        if(SelectedObjects.Count > 1)
            QueryMultipleInvokes(args.ButtonIndex);
    }

    private void QueryMultipleInvokes(int index)
    {
        for(int i = 0; i < SelectedObjects.Count; i++)
        {
            if ((m_MultiInvokeHandlerCache = SelectedObjects[i].gameObject.GetComponent<ButtonStateHandler>()) != null)
            {
                if(m_CurrentActionState.ActionStateIdentity == m_MultiInvokeHandlerCache.PrimaryState.ActionStateIdentity)
                {
                    if (m_MultiInvokeHandlerCache != m_CurrentStateHandler && ButtonTypeCanBeInvokedMultipleTimes(index))
                    {
                        m_MultiInvokeHandlerCache.PrimaryState.InvokeButtonAtIndex(index);
                    }
                }
            }
        }
    }

    private bool ButtonTypeCanBeInvokedMultipleTimes(int index)
    {
        return m_MultiInvokeHandlerCache.PrimaryState.GetButtonByIndex(index).ButtonType != ActionButton.ActionType.Building;
    }

    internal void DisableButton(int index)
    {
        m_PanelButtons[index].interactable = false;
    }

    internal void EnableButton(int index)
    {
        m_PanelButtons[index].interactable = true;
    }
}

public class ActionButtonEventArgs
{
    private int m_ButtonIndex = -1; /// Initialize as non-existent index value.

    internal int ButtonIndex
    {
        get { return m_ButtonIndex; }
        set { m_ButtonIndex = value; }
    }

    public ActionButtonEventArgs(int index)
    {
        m_ButtonIndex = index;
    }
}