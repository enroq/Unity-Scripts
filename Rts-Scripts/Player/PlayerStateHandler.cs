using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public enum PlayerType
{
    None,
    Human,
    Computer
}

public class PlayerStateHandler : MonoBehaviour
{
    [SerializeField]
    private bool m_DebugMode;
    [SerializeField]
    private int m_ControllingPlayerIndex;
    [SerializeField]
    private int m_UnitCountLimit = 200;
    [SerializeField]
    private bool m_OverrideTeam;
    [SerializeField]
    private List<Material> m_PlayerColorMaterials;
    [SerializeField]
    private GameObject m_PlayerStateObject;

    private Dictionary<int, PlayerState> 
        m_PlayerStates = new Dictionary<int, PlayerState>();

    private void Awake()
    {
        foreach(PlayerState state in m_PlayerStateObject.GetComponents<PlayerState>())
        {
            m_PlayerStates.Add(state.Team, state);
        }
    }

    internal bool DebugMode
    {
        get { return m_DebugMode; }
    }

    internal int UnitLimitCount
    {
        get { return m_UnitCountLimit; }
    }

    internal bool OverrideTeam
    {
        get { return m_OverrideTeam; }
    }

    internal PlayerState GetControllingPlayer()
    {
        return GetStateByIndex(m_ControllingPlayerIndex);
    }

    internal PlayerState GetStateByIndex(int index)
    {
        return m_PlayerStates[index];
    }

    internal bool IsTeamMember(GameObject obj)
    {
        return GetStateByIndex(m_ControllingPlayerIndex).IsTeamMember(obj);
    }

    internal bool IsTeamMember(BaseEntity entity)
    {
        return GetStateByIndex(m_ControllingPlayerIndex).IsTeamMember(entity);
    }

    internal bool IsTeamMember(int teamHash)
    {
        return GetStateByIndex(m_ControllingPlayerIndex).IsTeamMember(teamHash);
    }

    internal bool UnitsAreOfTeam(BaseEntity delta, BaseEntity gamma)
    {
        return delta.Team == gamma.Team;
    }
}
