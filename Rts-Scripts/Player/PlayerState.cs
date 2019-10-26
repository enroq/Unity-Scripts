using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class PlayerState : MonoBehaviour
{
    [SerializeField]
    bool m_DebugMode;
    [SerializeField]
    private int m_Team;
    [SerializeField]
    private string m_PlayerName;
    [SerializeField]
    private PlayerType m_PlayerType;
    [SerializeField]
    private int m_ColorId;

    private string m_PlayerId;

    private Dictionary<ResourceType, int>
        m_CurrentResources = new Dictionary<ResourceType, int>();

    private List<BaseBuilding> m_CurrentStructures = new List<BaseBuilding>();
    private List<BaseUnit> m_CurrentUnits = new List<BaseUnit>();

    [SerializeField]
    public string PlayerId
    {
        get { return m_PlayerId; }
    }

    private void Awake()
    {
        m_PlayerId = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        for (int i = 0; i < GameEngine.ResourceManager.Resources.Count; i++)
            m_CurrentResources.Add(GameEngine.ResourceManager.Resources[i], 0);
    }

    public int Team
    {
        get { return m_Team; }
        set { m_Team = value; }
    }

    public string PlayerName
    {
        get { return m_PlayerName; }
        set { m_PlayerName = value; }
    }

    public int ColorId
    {
        get { return m_ColorId; }
        set { m_ColorId = value; }
    }

    internal BaseBuilding[] CurrentStructures
    {
        get { return m_CurrentStructures.ToArray(); }
    }

    internal BaseUnit[] CurrentUnits
    {
        get { return m_CurrentUnits.ToArray(); }
    }

    internal int MaxResourceAmount
    {
        get { return GameEngine.ResourceManager.MaxResourceAmount; }
    }

    internal bool IsWithinUnitLimit
    {
        get
        {
            if (GameEngine.PlayerStateHandler.DebugMode)
                Debug.Log(string.Format("[{2}] Unit Count: {0} | Unit Limit: {1}", 
                    m_CurrentUnits.Count, GameEngine.PlayerStateHandler.UnitLimitCount, m_PlayerName));

            return m_CurrentUnits.Count <= GameEngine.PlayerStateHandler.UnitLimitCount;
        }
    }

    internal void AddBuildingToState(BaseBuilding building)
    {
        if (!m_CurrentStructures.Contains(building))
            m_CurrentStructures.Add(building);
    }

    internal void AddUnitToState(BaseUnit unit)
    {
        if (!m_CurrentUnits.Contains(unit))
            m_CurrentUnits.Add(unit);
    }

    internal void RemoveBuildingFromState(BaseBuilding building)
    {
        if (m_CurrentStructures.Contains(building))
            m_CurrentStructures.Remove(building);
    }

    internal void RemoveUnitFromState(BaseUnit unit)
    {
        if (m_CurrentUnits.Contains(unit))
            m_CurrentUnits.Remove(unit);
    }

    internal bool IsTeamMember(GameObject gameObj)
    {
        if (gameObj != null && gameObj.GetComponent<BaseEntity>() != null)
            return IsTeamMember(gameObj.GetComponent<BaseEntity>().Team);

        else
            return false;
    }

    internal bool IsTeamMember(BaseEntity entity)
    {
        return IsTeamMember(entity.Team);
    }

    internal bool IsTeamMember(int team)
    {
        if (GameEngine.PlayerStateHandler.OverrideTeam)
            return true;

        else
            return (team == m_Team);
    }

    internal bool ConsumeResource(ResourceType type, int amt)
    {
        if (m_CurrentResources.ContainsKey(type))
        {
            if (m_CurrentResources[type] - amt >= 0)
            {
                m_CurrentResources[type] -= amt;
                UpdateResourceDisplay(m_CurrentResources[type]);
                return true;
            }
        }
        return false;
    }

    internal void StoreResource(ResourceType type, int amt)
    {
        if (m_CurrentResources.ContainsKey(type))
        {
            if (m_CurrentResources[type] + amt <= MaxResourceAmount)
                m_CurrentResources[type] += amt;
            else
                m_CurrentResources[type] = MaxResourceAmount;

            UpdateResourceDisplay(m_CurrentResources[type]);
        }
    }

    void UpdateResourceDisplay(int amt)
    {
        if(GameEngine.PlayerStateHandler.IsTeamMember(Team))
            GameEngine.ResourceManager.UpdateResourceDisplay(amt);
    }
}
