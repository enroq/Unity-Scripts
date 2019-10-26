using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionHandler : KaryonBehaviour
{
    SelectionBoxHandler m_SelectionBoxHandler;

    int m_LeftMouseIndex = 0;

    RaycastHit[] m_RaycastHits;

    Ray m_RayOut;

    float m_RayOutDistance = 150.0f;
    float m_DoubleClickDecayTime = 0.35f;
    float m_DoubleClickSelectionRadius = 45.0f;

    bool m_MultiSelection = false;
    bool m_HasClickedOnce = false;
    bool m_OnSecondClick = false;

    Coroutine m_ClickDecayRoutine;
    Collider[] m_DoubleClickCollisions;

    SelectableObject m_SelectionCache;
    SelectableObject m_FocusCache;

    BaseEntity m_EntityCache;
    List<BaseEntity> m_EntitiesCache;

    ButtonStateHandler m_ButtonStateHandlerCache;

    List<SelectableObject> m_CurrentlySelectedObjects = new List<SelectableObject>();

    SelectableObject[] m_CurrentSelectionCache;

    int m_SelectionCountCache = 0;

    public GameObject FocusObject
    {
        get
        {
            if (m_CurrentlySelectedObjects.Count > 0 && m_CurrentlySelectedObjects[0] != null)
                return m_CurrentlySelectedObjects[0].gameObject;

            else
                return null;
        }
    }

    public bool IsFocusObject(GameObject gameObject)
    {
        if (FocusObject != null)
            return gameObject == FocusObject;
        else
            return false;
    }

    public bool UnitWithinSelectionBox(Vector2 screenPosition)
    {
        return m_SelectionBoxHandler.UnitWithinSelectionRegion(screenPosition);
    }

    public bool SelectionBoxActive
    {
        get { return m_SelectionBoxHandler.IsDraggingSelectionBox; }
    }

    public List<SelectableObject> SelectedObjects
    {
        get { return m_CurrentlySelectedObjects; }
    }

    void Awake()
    {
        if ((m_SelectionBoxHandler = GetComponent<SelectionBoxHandler>()) == null)
            throw new MissingComponentException("Selection Handler Missing Selection Box Handler");
    }

    void Update ()
    {
        if (GameEngine.ConstructionHandler.IsBuilding || GameEngine.AbilityTargetHandler.AbilityInQueue)
            return;

        if (m_CurrentlySelectedObjects.Count == 0 &&
                m_CurrentlySelectedObjects.Count != m_SelectionCountCache)
        {
            m_FocusCache = null;
            GameEngine.InfoBoxBehavior.HideMultiSelectionPanel();
            GameEngine.InfoBoxBehavior.HideSingleSelectionPanel();
            GameEngine.ActionPanelBehavior.ResetCurrentState();
        }

        else if (m_CurrentlySelectedObjects.Count == 1)
            QueryFocusChange();

        else if (m_CurrentlySelectedObjects.Count > 1 
            &&  m_CurrentlySelectedObjects.Count != m_SelectionCountCache)
        {
            UpdateMultiSelection();
        }

        if (m_CurrentlySelectedObjects.Count != m_SelectionCountCache)
            m_SelectionCountCache = m_CurrentlySelectedObjects.Count;

        if (Input.GetMouseButtonDown(m_LeftMouseIndex) && !EventSystem.current.IsPointerOverGameObject())
        {
            QueryDoubleClick();

            m_MultiSelection = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            m_RayOut = CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            m_RaycastHits = Physics.RaycastAll(m_RayOut, m_RayOutDistance);

            if (m_RaycastHits.Length > 0)
            {
                m_RaycastHits = OrderedRaycasts(m_RaycastHits);

                if(!RayHitsContainsSelectable())
                {
                    ClearCurrentSelections();
                    return;
                }

                for (int i = 0; i < m_RaycastHits.Length; i++)
                {
                    if ((m_SelectionCache = m_RaycastHits[i].collider.GetComponent<SelectableObject>()) != null)
                    {
                        if 
                            (m_OnSecondClick) HandleDoubleClick(m_SelectionCache);
                        else
                            HandleObjectSelection(m_SelectionCache, m_MultiSelection);
                        break;
                    }
                }
            }
        }
    }

    bool RayHitsContainsSelectable()
    {
        for (int i = 0; i < m_RaycastHits.Length; i++)
        {
            if ((m_SelectionCache = m_RaycastHits[i].collider.GetComponent<SelectableObject>()) != null)
            {
                return true;
            }
        }

        return false;
    }

    void HandleFocusChange()
    {
        if (GameEngine.PlayerStateHandler.IsTeamMember(m_FocusCache.gameObject))
        {
            if ((m_ButtonStateHandlerCache = m_FocusCache.gameObject.GetComponent<ButtonStateHandler>()) != null)
                GameEngine.ActionPanelBehavior.SetCurrentHandler(m_ButtonStateHandlerCache);
            else
                GameEngine.ActionPanelBehavior.ResetCurrentState();
        }

        else
            GameEngine.ActionPanelBehavior.ResetCurrentState();

        GameEngine.InfoBoxBehavior.SetFocusObject(FocusObject);
    }

    void QueryDoubleClick()
    {
        if (m_HasClickedOnce)
        {
            m_OnSecondClick = true;
            if (m_ClickDecayRoutine != null)
                StopCoroutine(m_ClickDecayRoutine);
        }

        else
        {
            m_HasClickedOnce = true;
            m_OnSecondClick = false;

            if (m_ClickDecayRoutine != null)
                StopCoroutine(m_ClickDecayRoutine);

            m_ClickDecayRoutine = StartCoroutine("DoubleClickDecay");
        }
    }

    void HandleDoubleClick(SelectableObject objectSelected)
    {
        m_HasClickedOnce = false;
        m_OnSecondClick = false;

        if (m_ClickDecayRoutine != null)
            StopCoroutine(m_ClickDecayRoutine);

        QueryUnitDoubleClick(objectSelected);
    }


    void QueryUnitDoubleClick(SelectableObject objectSelected)
    {     
        if (objectSelected.GetComponent<BaseEntity>() != null &&
            GameEngine.PlayerStateHandler.IsTeamMember(objectSelected.GetComponent<BaseEntity>()))
        {
            ClearCurrentSelections();
            SelectObject(objectSelected);
            SelectNearestMatchingUnits(objectSelected);
        }
    }

    void SelectNearestMatchingUnits(SelectableObject objectSelected)
    {
        m_DoubleClickCollisions = Physics.OverlapSphere
            (objectSelected.transform.position, m_DoubleClickSelectionRadius);

        m_DoubleClickCollisions = OrderCollidersByDistance
            (m_DoubleClickCollisions, objectSelected.transform.position).ToArray();

        for (int i = 0; i < m_DoubleClickCollisions.Length; i++)
        {
            if (MeetsAreaSelectionRequirements(m_DoubleClickCollisions[i], objectSelected))
                SelectObject(m_DoubleClickCollisions[i].GetComponent<SelectableObject>());
        }

        m_DoubleClickCollisions = null;
    }

    bool MeetsAreaSelectionRequirements(Collider collider, SelectableObject objectSelected)
    {
        if (collider.GetComponent<BaseEntity>() != null && collider.GetComponent<Renderer>().isVisible)
        {
            if((m_EntityCache = objectSelected.gameObject.GetComponent<BaseEntity>()) != null)
                return 
                    (collider.GetComponent<BaseEntity>().EntityName == m_EntityCache.EntityName
                        && GameEngine.PlayerStateHandler.IsTeamMember(collider.GetComponent<BaseEntity>()));
        }

        return false;
    }

    IEnumerator DoubleClickDecay()
    {
        yield return new 
            WaitForSecondsRealtime(m_DoubleClickDecayTime);

        m_HasClickedOnce = false;
        m_OnSecondClick = false;
    }

    internal void HandleObjectSelection(SelectableObject objectSelected, bool multiSelect)
    {
        if (multiSelect)
        {
            if (GameEngine.PlayerStateHandler.IsTeamMember(objectSelected.gameObject))
                ClearEnemiesSelected();
            else
                ClearAlliesSelected();

            SelectObject(objectSelected);
        }

        else
        {
            ClearCurrentSelections();
            SelectObject(objectSelected);
        }
    }

    void QueryFocusChange()
    {
        if (m_CurrentlySelectedObjects[0] != null 
            && m_FocusCache != m_CurrentlySelectedObjects[0])
        {
            m_FocusCache = m_CurrentlySelectedObjects[0];
            HandleFocusChange();
        }
    }

    void UpdateMultiSelection()
    {
        m_EntitiesCache = new List<BaseEntity>();
        for(int i = 0; i < m_CurrentlySelectedObjects.Count; i++)
        {
            if((m_EntityCache = m_CurrentlySelectedObjects[i].gameObject.GetComponent<BaseEntity>()) != null)
                m_EntitiesCache.Add(m_EntityCache);
        }

        GameEngine.InfoBoxBehavior.
            SetMultiEntityFocus(m_EntitiesCache.ToArray());

        m_EntitiesCache.Clear();
    }

    internal void SelectObject(SelectableObject objectSelected)
    {
        if (!m_CurrentlySelectedObjects.Contains(objectSelected))
        {
            m_CurrentlySelectedObjects.Add(objectSelected);
            objectSelected.Select();
        }
    }

    internal void DeselectObject(SelectableObject objectSelected)
    {
        if (m_CurrentlySelectedObjects.Contains(objectSelected))
        {
            m_CurrentlySelectedObjects.Remove(objectSelected);
            objectSelected.Deselect();
        }
    }

    internal void ClearCurrentSelections()
    {
        m_FocusCache = null;
        if (m_CurrentlySelectedObjects.Count > 0)
        {
            while (m_CurrentlySelectedObjects.Count > 0)
                DeselectObject(m_CurrentlySelectedObjects[0]);
        }
    }

    void ClearEnemiesSelected()
    {
        m_CurrentSelectionCache = m_CurrentlySelectedObjects.ToArray();
        for(int i = 0; i < m_CurrentSelectionCache.Length; i++)
        {
            if (!GameEngine.PlayerStateHandler.IsTeamMember(m_CurrentSelectionCache[i].gameObject))
                DeselectObject(m_CurrentSelectionCache[i]);
                
        }
    }

    void ClearAlliesSelected()
    {
        m_CurrentSelectionCache = m_CurrentlySelectedObjects.ToArray();
        for (int i = 0; i < m_CurrentSelectionCache.Length; i++)
        {
            if (GameEngine.PlayerStateHandler.IsTeamMember(m_CurrentSelectionCache[i].gameObject))
                DeselectObject(m_CurrentSelectionCache[i]);
        }
    }
}
