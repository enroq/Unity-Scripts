using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class InfoBoxBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject m_SingleSelectionPanel;
    [SerializeField]
    private GameObject m_MultiSelectionPanel;

    [SerializeField]
    private Image m_SingleObjectIcon;
    [SerializeField]
    private Image m_StatusBarBack;
    [SerializeField]
    private Image m_StatusBarFront;
    [SerializeField]
    private Image m_TaskBarFront;
    [SerializeField]
    private Image m_TaskBarBack;
    [SerializeField]
    private Image m_TaskIcon;
    [SerializeField]
    private GameObject m_QueuePanel;
    [SerializeField]
    private GameObject m_TaskDisplayObject;
    [SerializeField]
    private GameObject m_StatusDisplayObject;
    [SerializeField]
    private Text m_AttackText;
    [SerializeField]
    private Text m_DefenseText;
    [SerializeField]
    private Text m_HealthText;

    [SerializeField]
    private List<Image> m_QueueImages = new List<Image>();
    [SerializeField]
    private List<Image> m_MultiObjectIcons = new List<Image>();
    [SerializeField]
    private List<Image> m_MultiObjectHealthBars = new List<Image>();
    [SerializeField]
    private List<Image> m_MultiObjectHealthBackers = new List<Image>();

    private Sprite m_ObjectSpriteCache;

    private BaseEntity m_CurrentFocusEntity;
    private ITaskable m_CurrentFocusTask;

    private BaseEntity[] m_MultipleEntityFocus;

    private Dictionary<string, int> 
        m_MultiUnitSortCache = new Dictionary<string, int>();

    List<BaseEntity> m_MultiSelectDisplayCache = new List<BaseEntity>();

    int m_CurrentMultiDisplayIndex = 1;
    int m_CurrentSelectStartIndex = 0;
    int m_CurrentSelectEndIndex = 0;
    int m_MultiSelectRange = 16;

    public void IncreaseMultiDisplayIndex()
    {
        if (GameEngine.SelectionHandler.SelectedObjects.Count > m_MultiSelectRange * m_CurrentMultiDisplayIndex)
        {
            m_CurrentMultiDisplayIndex++;
            m_CurrentSelectEndIndex = m_CurrentMultiDisplayIndex * m_MultiSelectRange;
            m_CurrentSelectStartIndex = m_CurrentSelectEndIndex - m_MultiSelectRange;

            m_MultiSelectDisplayCache.Clear();

            for(int i = m_CurrentSelectStartIndex; i < m_CurrentSelectEndIndex; i++)
            {
                if (i < GameEngine.SelectionHandler.SelectedObjects.Count)
                    m_MultiSelectDisplayCache.Add
                        (GameEngine.SelectionHandler.SelectedObjects[i].GetComponent<BaseEntity>());

            }

            SetMultiEntityFocus(m_MultiSelectDisplayCache.ToArray());
        }
    }

    public void DecreaseMultiDisplayIndex()
    {
        if (m_CurrentMultiDisplayIndex > 1)
        {
            m_CurrentMultiDisplayIndex--;
            m_CurrentSelectEndIndex = m_CurrentMultiDisplayIndex * m_MultiSelectRange;
            m_CurrentSelectStartIndex = m_CurrentSelectEndIndex - m_MultiSelectRange;

            m_MultiSelectDisplayCache.Clear();

            for (int i = m_CurrentSelectStartIndex; i < m_CurrentSelectEndIndex; i++)
            {
                if (i < GameEngine.SelectionHandler.SelectedObjects.Count)
                    m_MultiSelectDisplayCache.Add
                        (GameEngine.SelectionHandler.SelectedObjects[i].GetComponent<BaseEntity>());

            }

            SetMultiEntityFocus(m_MultiSelectDisplayCache.ToArray());
        }
    }

    internal BaseEntity FocusEntity
    {
        get { return m_CurrentFocusEntity; }
    }

    private void Awake()
    {
        GameEngine.AttachInfoBox(gameObject);
    }

    private void Start()
    {
        m_ObjectSpriteCache = m_SingleObjectIcon.sprite;
        
        HideTaskStatus();
        HideMultiSelectionPanel();
        HideSingleSelectionPanel();
    }

    private void Update()
    {
        if (m_CurrentFocusEntity != null)
            UpdateStatusDisplay(m_CurrentFocusEntity);

        if (m_CurrentFocusTask != null)
            UpdateTaskDisplayStatus(m_CurrentFocusTask);

        if (m_MultipleEntityFocus != null)
            UpdateMultiEntityStatus(m_MultipleEntityFocus);
    }

    internal void SetTaskFocus(ITaskable task)
    {
        m_CurrentFocusTask = task;
        SetTaskIcon(task.TaskIcon);
        DisplayTaskStatus();
    }

    internal void SetFocusObject(GameObject gameObject)
    {
        if(GameEngine.DebugMode)
            Debug.Log("Info Box Focus: " + gameObject);

        BaseEntity entityCache;
        FoundationBehavior foundationCache;

        HideTaskStatus();
        
        if ((entityCache = gameObject.GetComponent<BaseEntity>()) != null)
        {
            m_CurrentFocusEntity = entityCache;
            SetSingleObjectIcon(entityCache.EntityIcon);
            DisplaySingleSelectionPanel();

            if (GameEngine.PlayerStateHandler.IsTeamMember(entityCache))
            {
                if (entityCache is ITasker && !(entityCache is BaseWorker))
                {
                    if (((ITasker)entityCache).CurrentTask != null)
                    {
                        m_CurrentFocusTask = ((ITasker)entityCache).CurrentTask;
                        SetTaskIcon(m_CurrentFocusTask.TaskIcon);

                        UpdateTaskQueueIcons
                            (((ITasker)entityCache).TaskQueue.ToArray());
                        DisplayTaskStatus();
                    }

                    else UpdateStatDisplay(entityCache);
                }

                else UpdateStatDisplay(entityCache);
            }

            else UpdateStatDisplay(entityCache);
        }

        else if((foundationCache = gameObject.GetComponent<FoundationBehavior>()) != null)
        {
            SetSingleObjectIcon(foundationCache.ParentBuilding.EntityIcon);
            DisplaySingleSelectionPanel();

            m_CurrentFocusEntity = foundationCache.ParentBuilding;
        }

        else
        {
            ClearFocusObject();
        }
    }

    internal void ClearFocusObject()
    {
        m_CurrentFocusEntity = null;
        HideSingleSelectionPanel();
        ResetSingleObjectIcon();
    }

    internal void SetSingleObjectIcon(Sprite sprite)
    {
        m_SingleObjectIcon.sprite = sprite;
    }

    internal void SetTaskIcon(Sprite sprite)
    {
        m_TaskIcon.sprite = sprite;
    }

    internal void ResetSingleObjectIcon()
    {
        m_SingleObjectIcon.sprite = m_ObjectSpriteCache;
    }

    internal void ResetTaskIcon()
    {
        m_TaskIcon.sprite = m_ObjectSpriteCache;
    }

    internal void DisplaySingleSelectionPanel()
    {
        if (m_MultiSelectionPanel.activeInHierarchy)
            HideMultiSelectionPanel();

        m_SingleSelectionPanel.SetActive(true);
    }

    internal void HideSingleSelectionPanel()
    {
        if(m_SingleSelectionPanel.activeInHierarchy)
            m_SingleSelectionPanel.SetActive(false);
    }

    internal void DisplayMultiSelectionPanel()
    {
        if (m_SingleSelectionPanel.activeInHierarchy)
            HideSingleSelectionPanel();

        m_MultiSelectionPanel.SetActive(true);
    }

    internal void HideMultiSelectionPanel()
    {
        if(m_MultiSelectionPanel.activeInHierarchy)
            m_MultiSelectionPanel.SetActive(false);
    }

    internal void UpdateStatDisplay(BaseEntity entity)
    {
        if(entity != null)
        {
            m_HealthText.text = FormatHealthString(entity.MaxHealth);
            m_DefenseText.text = FormatDefenseString(entity.DefenseRating);

            if (entity.gameObject.GetComponent<ICombatant>() != null)
            {
                if (!m_AttackText.gameObject.activeInHierarchy)
                    m_AttackText.gameObject.SetActive(true);

                m_AttackText.text = FormatAttackString
                    (entity.gameObject.GetComponent<ICombatant>().AttackRating,
                        entity.gameObject.GetComponent<ICombatant>().AttackType.ToString());
            }

            else
                m_AttackText.gameObject.SetActive(false);

            if (!m_StatusDisplayObject.activeInHierarchy)
                m_StatusDisplayObject.SetActive(true);
        }
    }

    internal void DisableStatDisplay()
    {
        if (m_StatusDisplayObject.activeInHierarchy)
            m_StatusDisplayObject.SetActive(false);
    }

    internal void DisplayTaskStatus()
    {
        DisableStatDisplay();

        m_TaskDisplayObject.SetActive(true);

        m_TaskBarBack.gameObject.SetActive(true);
        m_TaskBarFront.gameObject.SetActive(true);
        m_TaskIcon.gameObject.SetActive(true);

        m_QueuePanel.SetActive(true);
    }

    internal void HideTaskStatus()
    {
        m_TaskDisplayObject.SetActive(false);

        m_TaskBarBack.gameObject.SetActive(false);
        m_TaskBarFront.gameObject.SetActive(false);
        m_TaskIcon.gameObject.SetActive(false);

        m_QueuePanel.SetActive(false);
    }

    internal void UpdateTaskDisplayStatus(ITaskable task)
    {
        if(task.TaskStatus == TaskStatus.Completed)
        {
            m_CurrentFocusTask = null;
            HideTaskStatus();
            ResetTaskIcon();
            return;
        }

        float ratio = (float)task.TaskProgressLevel / (float)task.MaxProgressLevel;
        m_TaskBarFront.fillAmount = ratio;
    }

    internal void UpdateStatusDisplay(BaseEntity entity)
    {
        float ratio = (float)entity.CurrentHealth / (float)entity.MaxHealth;
        m_StatusBarFront.fillAmount = ratio;
    }

    internal void UpdateTaskQueueIcons(ITaskable[] tasks)
    {
        if (!m_QueuePanel.activeInHierarchy)
            m_QueuePanel.SetActive(true);

        for (int i = 0; i < m_QueueImages.Count; i++)
        {
            if (tasks.Length > i)
            {
                m_QueueImages[i].gameObject.SetActive(true);
                m_QueueImages[i].sprite = tasks[i].TaskIcon;
            }

            else
            {
                m_QueueImages[i].gameObject.SetActive(false);
                m_QueueImages[i].sprite = m_ObjectSpriteCache;
            }
        }
    }

    internal void SetMultiEntityFocus(BaseEntity[] entities)
    {
        m_MultiSelectDisplayCache.Clear();

        if (entities.Length <= m_MultiSelectRange)
        {
            for (int i = 0; i < entities.Length; i++)
                m_MultiSelectDisplayCache.Add(entities[i]);
        }

        else
        {
            for (int i = 0; i < m_MultiSelectRange; i++)
                m_MultiSelectDisplayCache.Add(entities[i]);
        }

        m_MultipleEntityFocus = m_MultiSelectDisplayCache.ToArray();

        m_MultiSelectDisplayCache.Clear();

        UpdateMultiSelection(m_MultipleEntityFocus);
    }

    internal void ClearMultiEntityFocus()
    {
        m_MultipleEntityFocus = null;

        for (int i = 0; i < m_MultiObjectIcons.Count; i++)
        {
            m_MultiObjectIcons[i].gameObject.SetActive(false);
            m_MultiObjectIcons[i].sprite = m_ObjectSpriteCache;
            m_MultiObjectHealthBars[i].gameObject.SetActive(false);
            m_MultiObjectHealthBackers[i].gameObject.SetActive(false);
        }

        HideMultiSelectionPanel();
    }

    internal void UpdateMultiSelection(BaseEntity[] entities)
    {
        if (!m_MultiSelectionPanel.activeInHierarchy)
            DisplayMultiSelectionPanel();

        for(int i = 0; i < m_MultiObjectIcons.Count; i++)
        {
            if(entities.Length > i)
            {
                m_MultiObjectIcons[i].gameObject.SetActive(true);
                m_MultiObjectIcons[i].sprite = entities[i].EntityIcon;

                m_MultiObjectHealthBars[i].gameObject.SetActive(true);
                m_MultiObjectHealthBackers[i].gameObject.SetActive(true);

                UpdateHealthBar(m_MultiObjectHealthBars[i], entities[i]);
            }

            else
            {
                m_MultiObjectIcons[i].gameObject.SetActive(false);
                m_MultiObjectIcons[i].sprite = m_ObjectSpriteCache;
                m_MultiObjectHealthBars[i].gameObject.SetActive(false);
                m_MultiObjectHealthBackers[i].gameObject.SetActive(false);
            }
        }

        ProcessActionDisplayPriority(entities);
    }

    internal void UpdateMultiEntityStatus(BaseEntity[] entities)
    {
        for(int i = 0; i < entities.Length; i++)
        {
            UpdateHealthBar(m_MultiObjectHealthBars[i], entities[i]);
        }
    }

    internal void UpdateHealthBar(Image img, BaseEntity entity)
    {
        float ratio = (float)entity.CurrentHealth / (float)entity.MaxHealth;
        img.fillAmount = ratio;
    }

    internal void ProcessActionDisplayPriority(BaseEntity[] entities)
    {
        m_MultiUnitSortCache.Clear();
        string priorityEntityName = string.Empty;
        int currentHighestCount = 0;

        for(int i = 0; i < entities.Length; i++)
        {
            if (m_MultiUnitSortCache.ContainsKey(entities[i].EntityName))
                m_MultiUnitSortCache[entities[i].EntityName]++;

            else m_MultiUnitSortCache.Add(entities[i].EntityName, 1);
        }

        foreach(KeyValuePair<string, int> kvp in m_MultiUnitSortCache)
        {
            if(kvp.Value > currentHighestCount)
            {
                priorityEntityName = kvp.Key;
                currentHighestCount = kvp.Value;
            }
        }

        for(int i = 0; i < entities.Length; i++)
        {
            if(entities[i].EntityName == priorityEntityName)
            {
                if(entities[i].gameObject.GetComponent<ButtonStateHandler>()
                    && GameEngine.PlayerStateHandler.IsTeamMember(entities[i]))
                {
                    GameEngine.ActionPanelBehavior.SetCurrentHandler
                        (entities[i].gameObject.GetComponent<ButtonStateHandler>());
                }
                break;
            }
        }
    }

    string FormatAttackString(int atk, string type)
    {
        return string.Format
            ("<color=\"yellow\">Damage:</color> <color=\"white\">{0} ({1})</color>", atk, type);
    }

    string FormatDefenseString(int def)
    {
        return string.Format
            ("<color=\"yellow\">Defense:</color> <color=\"white\">{0}</color>", def);
    }

    string FormatHealthString(int hp)
    {
        return string.Format
            ("<color=\"yellow\">Health:</color> <color=\"white\">{0}</color>", hp);
    }
}