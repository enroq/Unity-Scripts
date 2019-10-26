using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupMovement
{
    Vector3 m_ReferencePosition;

    BaseUnit[] m_Units;

    Vector3[] m_Destinations;
    Vector3[] m_Offsets;

    List<Vector3> m_ValidPositionCache = new List<Vector3>();
    List<NavigationHandler> m_FailedNavAgentCache = new List<NavigationHandler>();

    CommandType m_CurrentCommand;

    public GroupMovement(BaseUnit[] units, Vector3 destination, CommandType command, Vector3[] offsets)
    {
        m_Units = units;
        m_Offsets = offsets;
        m_CurrentCommand = command;
        m_ReferencePosition = destination; m_Destinations = new Vector3[m_Units.Length];

        m_Destinations[0] = m_ReferencePosition;
        Vector3 leadPosition = m_Units[0].transform.position;

        if (FormationHandler.ShouldSwapAxes((leadPosition - m_ReferencePosition)))
            for (int i = 1; i < m_Units.Length; i++)
                m_Offsets[i] = FormationHandler.SwapAxisValues(m_Offsets[i]);

        for (int i = 1; i < m_Units.Length; i++)
            m_Destinations[i] = m_ReferencePosition + m_Offsets[i];
    }

    public void MoveUnitsAsGroup()
    {
        for (int i = 0; i < m_Units.Length; i++)
        {
            m_Units[i].GoToPositionWithGroup(this, m_CurrentCommand, m_Destinations[i]);
            m_Units[i].PlayMovementConfirmSound();
        }
    }

    public void HandleFailedNavAttempts()
    {
        for(int i = 0; i < m_FailedNavAgentCache.Count; i++)
            m_FailedNavAgentCache[i].NavigateToGroupPosition
                (this, m_CurrentCommand != CommandType.None, GetRandomValidPosition());
    }

    public void NullCaches()
    {
        m_ValidPositionCache = null;
        m_Destinations = null;
        m_Offsets = null;
        m_Units = null;
    }

    internal void AddVectorToValidPositions(Vector3 position)
    {
        m_ValidPositionCache.Add(position);
    }

    internal void AddFailedNavAgent(NavigationHandler navHandler)
    {
        m_FailedNavAgentCache.Add(navHandler);
    }

    internal Vector3? GetRandomValidPosition()
    {
        if(m_ValidPositionCache.Count > 0)
            return m_ValidPositionCache
                [UnityEngine.Random.Range(0, m_ValidPositionCache.Count -1)];
        else
        {
            return null;
        }
    }
}
