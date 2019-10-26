using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable
public enum FormationType
{
    None,
    Grid,
    Arrow
}

/// <summary>
/// In Development: Not Fully Functioning
/// </summary>
public class FormationHandler : MonoBehaviour
{
    public FormationType m_CurrentFormationType = FormationType.Grid;

    public List<UnitFormation> m_AvailableFormations;

    Coroutine m_CleanupRoutine;

    public UnitFormation GetFormationById(string id)
    {
        for(int i = 0; i < m_AvailableFormations.Count; i++)
            if (m_AvailableFormations[i].m_FormationId.ToLower() == id.ToLower())
                return m_AvailableFormations[i];

        return null;
    }

    public Vector3[] GetFormationPositions(string id)
    {
        UnitFormation formation = GetFormationById(id);

        if (formation != null)
            return formation.FormationOffsets;

        else
            return null;
    }

    public void SendUnitsInFormation
        (BaseUnit[] units, Vector3 destination, CommandType command, string formation)
    {
        GroupMovement group = new GroupMovement
            (units, destination, command, GetFormationPositions(formation));

        group.MoveUnitsAsGroup();
        group.HandleFailedNavAttempts();

        m_CleanupRoutine = StartCoroutine(Cleanup(group));
    }

    public void SendUnits(BaseUnit[] units, Vector3 destination, CommandType command)
    {
        for (int i = 0; i < units.Length; i++)
        {
            units[i].GoToPosition(destination, command);
            units[i].PlayMovementConfirmSound();
        }
    }

    public void HandleGroupMovement(BaseUnit[] units, Vector3 destination, CommandType command)
    {
        switch(m_CurrentFormationType)
        {
            case FormationType.None:
                {
                    SendUnits(units, destination, command);
                    break;
                }
            case FormationType.Grid:
                {
                    SendUnitsInFormation(units, destination, command, "grid");
                    break;
                }
            default: break;
        }
    }

    /// <summary>
    /// Swaps a vector3's x and z values.
    /// </summary>
    internal static Vector3 SwapAxisValues(Vector3 vector)
    {
        Vector3 v = Vector3.zero;

        v.x = vector.z;
        v.z = vector.x;
        v.y = vector.y;

        return v;
    }

    internal static bool ShouldSwapAxes(Vector3 vector)
    {
        return Mathf.Abs(vector.z) > Mathf.Abs(vector.x);
    }

    IEnumerator Cleanup(GroupMovement group)
    {
        yield return new WaitForSeconds(1.0f);
        group.NullCaches();
        group = null;
    }
}