using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable
public class TabInputHandler : MonoBehaviour
{
    [System.Serializable]
    internal class SelectableOnEnterRelationship
    {
        /// <summary>
        /// Selectable That User Must Focus/Highlight In Order To Activate Button On Enter.
        /// </summary>
        [Tooltip("Selectable That User Must Focus/Highlight In Order To Activate Button On Enter.")]
        [SerializeField]
        Selectable m_Selectable;

        /// <summary>
        /// Button To Be Activated When Selectable Relative Is Focused On And User Presses Enter.
        /// </summary>
        [Tooltip("Button To Be Activated When Selectable Relative Is Focused On And User Presses Enter.")]
        [SerializeField]
        Button m_Button;

        /// <summary>
        /// Determines Whether Or Not The Relative Selectable Should Be Reselected After Invoking Button Press.
        /// </summary>
        [Tooltip("Determines Whether Or Not The Relative Selectable Should Be Reselected After Invoking Button Press.")]
        [SerializeField]
        bool m_ReselectOnEnter;

        internal Button Button { get { return m_Button; } }
        internal Selectable Selectable { get { return m_Selectable; } }
        internal bool ReselectOnEnter { get { return m_ReselectOnEnter; } }
    }

    /// <summary>
    /// Color Used For Highlighting Selectables If Color Override Enabled.
    /// </summary>
    [Tooltip("Color Used For Highlighting Selectables If Color Override Enabled.")]
    [SerializeField]
    Color m_DefaultSelectionColor;

    /// <summary>
    /// Allows Developer To Enabled Or Disable Color Override.
    /// </summary>
    [Tooltip("Allows Developer To Enabled Or Disable Color Override.")]
    [SerializeField]
    bool m_OverrideSelectionColor;

    /// <summary>
    /// Enabling This Will Remove The Need For Transition Events Except In Edge Cases.
    /// Use Transition Events Instead Of Auto Transition For Preformance Gains When Displaying High Quantities Of Selectables.
    /// </summary>
    [Tooltip("Enabling This Will Remove The Need For Transition Events Except In Edge Cases.")]
    [SerializeField]
    bool m_AutoTransitionMode;

    /// <summary>
    /// Allows Users To Tab Backwards By Holding Left-Control When Tabbing (Only Works In Build, Editor Bug As Of 2017.4).
    /// </summary>
    [Tooltip("Allows Users To Tab Backwards By Holding Left-Control When Tabbing (Only Works In Build, Editor Bug).")]
    [SerializeField]
    bool m_AllowReverseTab;

    /// <summary>
    /// Allows Developer To Enable Or Disable Selectable Relationship Functions.
    /// </summary>
    [Tooltip("Allows Developer To Enable Or Disable Selectable Relationship Functions.")]
    [SerializeField]
    bool m_SelectableRelationshipsEnabled;

    /// <summary>
    /// Allows Selectables That Would Not Normally Respond To The Enter Button To Be Activated By Pressing Enter.
    /// </summary>
    [Tooltip("Allows Selectables That Would Not Normally Respond To The Enter Button To Be Activated By Pressing Enter.")]
    [SerializeField]
    bool m_OverrideNavigationEvents;

    /// <summary>
    /// Determines Whether Or Not The First Selectable Should Be Focused On When Starting Game/Scene.
    /// </summary>
    [Tooltip("Determines Whether Or Not The First Selectable Should Be Focused On When Starting Game/Scene.")]
    [SerializeField]
    bool m_AutoSelectFirstSelectable;

    /// <summary>
    /// Allows Developers To Specify Containers (Objects With Selectable Children) Should Be Ignored When Tabbing.
    /// </summary>
    [Tooltip("Allows Developers To Specify Containers (Objects With Selectable Children) Should Be Ignored When Tabbing.")]
    [SerializeField]
    List<GameObject> m_OmittedContainers;

    /// <summary>
    /// On Enter Relationships Allow The User To Activate A Button When Pressing Enter While Corresponding Selectable Is Selected.
    /// For Example: Pressing Enter While Chat Text Input Object Is Selected Will Activate The Send Button.
    /// </summary>
    [Tooltip("On Enter Relationships Allow The User To Activate A Button When Pressing Enter While Corresponding Selectable Is Selected.")]
    [SerializeField]
    List<SelectableOnEnterRelationship> m_OnEnterRelationships;

    EventSystem m_EventSystem;

    Selectable m_CurrentSelectable = null;

    List<Selectable> 
        m_Selectables = new List<Selectable>();

    Dictionary<Selectable, Tuple<Button, bool>> 
        m_SelectableRelationshipDictionary = new Dictionary<Selectable, Tuple<Button, bool>>();

    public delegate void GuiTransitionEventHandler();

    /// <summary>
    /// GuiTransitionEvents Are Used To Clear Current Selectables When Switching Between Gui Panels.
    /// </summary>
    public static event GuiTransitionEventHandler GuiTransitionEvent;

    public static void InvokeTransitionEvent()
    {
        GuiTransitionEvent?.Invoke();
    }

    void TabInputHandler_GuiTransitionEvent()
    {
        ClearSelectables();
        GenerateSelectables();
    }

    void Start()
    {
        m_EventSystem = EventSystem.current;

        m_EventSystem.sendNavigationEvents = !m_OverrideNavigationEvents;

        GuiTransitionEvent += TabInputHandler_GuiTransitionEvent;

        for (int i = m_OnEnterRelationships.Count - 1; i >= 0; i--)
        {
            SelectableOnEnterRelationship r = m_OnEnterRelationships[i];
            if(r.Selectable && r.Button)
                m_SelectableRelationshipDictionary.Add
                    (r.Selectable, new Tuple<Button, bool>(r.Button, r.ReselectOnEnter));
        }

        GenerateSelectables();

        if (m_AutoSelectFirstSelectable)
            SelectNext();
    }

    void Update()
    {
        if (QueryRelationshipInvoke())
            QueryOnEnterRelationships();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (m_AllowReverseTab && Input.GetKey(KeyCode.LeftControl))
                CycleSelectablesInReverse();
            else
                CycleSelectablesInOrder();
        }
    }

    void ClearSelectables()
    {
        m_Selectables.Clear();

        m_CurrentSelectable = null;
    }

    bool QueryRelationshipInvoke()
    {
        return (Input.GetKeyDown(KeyCode.Return) && m_SelectableRelationshipsEnabled);
    }

    void QueryOnEnterRelationships()
    {
        QueryCurrentSelection();

        if(m_OverrideNavigationEvents)
        {
            Button button; Toggle toggle;

            if ((button = m_EventSystem.currentSelectedGameObject.GetComponent<Button>()) != null)
            {
                if(button.IsActive())
                    button.onClick.Invoke();
            }

            if ((toggle = m_EventSystem.currentSelectedGameObject.GetComponent<Toggle>()) != null)
            {
                if(toggle.IsActive())
                    toggle.OnPointerClick(new PointerEventData(m_EventSystem));
            }
        }

        if (m_SelectableRelationshipDictionary.ContainsKey(m_CurrentSelectable))
        {
            var tuple = m_SelectableRelationshipDictionary[m_CurrentSelectable];

            if (tuple.Item1.IsActive())
            {
                tuple.Item1.onClick.Invoke();
                if (tuple.Item2)
                    SelectObject(m_CurrentSelectable);
            }
        }
    }

    void CycleSelectablesInOrder()
    {
        if (m_AutoTransitionMode && ShouldResetSelectables())
            ClearSelectables();

        if (m_Selectables.Count == 0)
            GenerateSelectables();

        if (m_Selectables.Count > 0)
        {
            QueryCurrentSelection();
            SelectNext();
        }
    }

    void CycleSelectablesInReverse()
    {
        if (m_AutoTransitionMode && ShouldResetSelectables())
            ClearSelectables();

        if (m_Selectables.Count == 0)
            GenerateSelectables();

        if (m_Selectables.Count > 0)
        {
            QueryCurrentSelection();
            SelectPrevious();
        }
    }

    void SelectNext()
    {
        if (m_Selectables.Count < 1)
            return;

        if (m_CurrentSelectable == null)
            m_CurrentSelectable = m_Selectables[0];

        else
            m_CurrentSelectable = GetNextSelectable();

        SelectObject(m_CurrentSelectable);
    }

    void SelectPrevious()
    {
        if (m_CurrentSelectable == null)
            m_CurrentSelectable = m_Selectables[m_Selectables.Count -1];

        else
            m_CurrentSelectable = GetPreviousSelectable();

        SelectObject(m_CurrentSelectable);
    }

    Selectable GetNextSelectable()
    {
        int currentIndex = m_Selectables.IndexOf(m_CurrentSelectable);

        if (currentIndex + 1 < m_Selectables.Count)
            return m_Selectables[currentIndex + 1];

        else return m_Selectables[0];
    }

    Selectable GetPreviousSelectable()
    {
        int currentIndex = m_Selectables.IndexOf(m_CurrentSelectable);

        if (currentIndex - 1 >= 0)
            return m_Selectables[currentIndex - 1];

        else return m_Selectables[m_Selectables.Count -1];
    }

    void GenerateSelectables()
    {
        ClearSelectables();

        for (int i = Selectable.allSelectables.Count - 1; i >= 0; i--)
        {
            Selectable s = Selectable.allSelectables[i];

            if (m_OverrideSelectionColor)
                SetSelectedColor(s);

            if (s.IsActive() && s.interactable && !m_OmittedContainers.Contains(s.transform.parent.gameObject))
            {
                m_Selectables.Add(s);
            }
        }

        m_Selectables = m_Selectables.OrderByDescending
            (s => s.gameObject.transform.position.y).ThenBy(s => s.gameObject.transform.position.x).ToList();
    }

    void SelectObject(Selectable selectable)
    {
        InputField input = selectable.GetComponent<InputField>();
        if (input)
            input.OnPointerClick(new PointerEventData(m_EventSystem));

        m_EventSystem.SetSelectedGameObject
            (selectable.gameObject, new BaseEventData(m_EventSystem));
    }

    bool ShouldResetSelectables()
    {
        if (m_Selectables.Count != Selectable.allSelectables.Count)
            return true;

        for (int i = m_Selectables.Count - 1; i >= 0; i--)
            if (!m_Selectables[i].IsActive())
                return true;

        return false;
    }

    void SetSelectedColor(Selectable selectable)
    {
        ColorBlock cBlock = selectable.colors;

        cBlock.highlightedColor = m_DefaultSelectionColor;

        selectable.colors = cBlock;
    }

    void QueryCurrentSelection()
    {
        if (m_EventSystem.currentSelectedGameObject == null)
            return;

        if (m_CurrentSelectable == null)
            SyncCurrentSelection();

        if (m_CurrentSelectable == null)
            return;

        if (m_EventSystem.currentSelectedGameObject != m_CurrentSelectable.gameObject)
                SyncCurrentSelection();
    }

    void SyncCurrentSelection()
    {
        Selectable current = m_EventSystem.currentSelectedGameObject.GetComponent<Selectable>();

        if (current != null && m_Selectables.Contains(current))
            m_CurrentSelectable = m_Selectables[m_Selectables.IndexOf(current)];
    }
}
