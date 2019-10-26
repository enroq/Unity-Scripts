using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable

public enum CommandTargetType
{
    None,
    Terrain,
    Unit,
    Building,
    Resource
}

public enum CommandType
{
    None,
    Move,
    Attack,
    Engage,
    Defend,
    Patrol,
    Guard,
    Build,
    Follow,
    Harvest,
    Repair
}

public class CommandHandler : KaryonBehaviour
{
    int m_RightMouseIndex = 1;

    Ray m_CameraRayOut;
    RaycastHit[] m_CameraRaycastHits; 

    float m_RayOutDistance = 150.0f;

    BaseUnit m_DeltaUnit;
    List<BaseUnit> m_UnitCache = new List<BaseUnit>();

    BaseBuilding m_DeltaBuilding;
    List<BaseBuilding> m_BuildingCache = new List<BaseBuilding>();

    BaseWorker m_WorkerCache;
    MonoBehaviour m_TargetCache;
    HarvesterState m_HarvesterCache;

    float m_NotificationObjectLifespan = 0.5f;

    Vector3? m_DestinationCache = null;

    Coroutine m_NotificationDecayRoutine;

    [SerializeField]
    GameObject m_NotificationObject;

    internal SelectableObject[] ObjectsSelected
    {
        get { return GameEngine.SelectionHandler.SelectedObjects.ToArray(); }
    }

    void Update ()
    {
        if (GameEngine.ConstructionHandler.IsBuilding || GameEngine.AbilityTargetHandler.AbilityInQueue)
            return;

        if (Input.GetMouseButtonDown(m_RightMouseIndex) && !EventSystem.current.IsPointerOverGameObject())
        {
            m_CameraRayOut = CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            m_CameraRaycastHits = Physics.RaycastAll(m_CameraRayOut, m_RayOutDistance);

            if (m_CameraRaycastHits.Length > 0)
                ProcessRayHits(m_CameraRaycastHits);
        }
    }

    internal void ProcessRayHits(RaycastHit[] hits)
    {
        m_CameraRaycastHits = OrderedRaycasts(hits);
        for (int i = m_CameraRaycastHits.Length - 1; i >= 0; i--)
        {
            CommandTargetType targetType;
            if (HitActionableObject(m_CameraRaycastHits[i].collider, out targetType))
            {
                ExecuteCommandOnTarget(m_CameraRaycastHits[i], targetType);
                break;
            }
        }
    }

    internal void ProcessMinimapRayHit(RaycastHit hit)
    {
        ExecuteCommandOnTarget(hit, CommandTargetType.Terrain);
    }

    void ExecuteCommandOnTarget(RaycastHit hit, CommandTargetType targetType)
    {
        switch (targetType)
        {
            case CommandTargetType.Terrain:
                {
                    HandleTerrainCommand(hit.point);
                    break;
                }
            case CommandTargetType.Unit:
                {
                    HandleUnitTargetCommand(hit);
                    break;
                }
            case CommandTargetType.Building:
                {
                    HandleBuildingTargetCommand(hit);
                    break;
                }
            case CommandTargetType.Resource:
                {
                    HandleResourceCommand(hit);
                    break;
                }
            default: break;
        }
    }

    /// <summary>
    /// Determines whether or not a command (right) click has hit an
    /// actionable target and relays the corresponding command type if it did.
    /// </summary>
    /// <param name="collider">Collider hit on command (right) click.</param>
    /// <param name="targetType">Type of object hit.</param>
    /// <returns>Returns true if collider game object is recognized as actionable.</returns>
    bool HitActionableObject(Collider collider, out CommandTargetType targetType)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            targetType = CommandTargetType.Terrain;
            return true;
        }

        else if (collider.GetComponent<FoundationBehavior>() != null)
        {
            targetType = CommandTargetType.Building;
            return true;
        }

        else if (collider.GetComponent<BaseBuilding>() != null)
        {
            targetType = CommandTargetType.Building;
            return true;
        }

        else if (collider.GetComponent<BaseUnit>() != null)
        {
            targetType = CommandTargetType.Unit;
            return true;
        }

        else if(collider.GetComponent<ResourceNode>() != null)
        {
            targetType = CommandTargetType.Resource;
            return true;
        }

        targetType = CommandTargetType.None; return false;
    }

    void HandleResourceCommand(RaycastHit target)
    {
        if((m_TargetCache = target.collider.GetComponent<ResourceNode>()) != null)
        {
            if(ObjectsSelected.Length > 0)
                m_TargetCache.gameObject.GetComponent<SelectableObject>().FlashSelection();
            for (int i = ObjectsSelected.Length -1; i >= 0; i--)
            {
                if((m_HarvesterCache = ObjectsSelected[i].GetComponent<HarvesterState>()) != null)
                    m_HarvesterCache.SetTargetNode((ResourceNode)m_TargetCache, target.collider);
            }
        }
    }

    void HandleTerrainCommand(Vector3 point)
    {
        if (GameEngine.SelectionHandler.SelectedObjects.Count == 0)
            return;

        ActivateNotificationObject(point);

        for (int i = 0; i < ObjectsSelected.Length; i++)
        {
            if ((m_DeltaUnit = ObjectsSelected[i].GetComponent<BaseUnit>()) != null
                    && GameEngine.PlayerStateHandler.IsTeamMember(ObjectsSelected[i].gameObject))
                        m_UnitCache.Add(m_DeltaUnit);

            else 
                if ((m_DeltaBuilding = ObjectsSelected[i].GetComponent<BaseBuilding>()) != null
                        && GameEngine.PlayerStateHandler.IsTeamMember(ObjectsSelected[i].gameObject))
                            m_BuildingCache.Add(m_DeltaBuilding);
        }

        ProcessBuildingTerrainCommand(point);
        ProcessUnitTerrainCommands(point);

        m_UnitCache.Clear();  m_BuildingCache.Clear();
    }

    void ActivateNotificationObject(Vector3 v)
    {
        if (m_NotificationObject != null)
        {
            m_NotificationObject.SetActive(false);
            m_NotificationObject.transform.position = v;
            m_NotificationObject.SetActive(true);

            if(m_NotificationDecayRoutine != null)
                StopCoroutine(m_NotificationDecayRoutine);

            m_NotificationDecayRoutine = 
                StartCoroutine(NotificationObjectDecay());
        }
    }

    IEnumerator NotificationObjectDecay()
    {
        yield return new 
            WaitForSeconds(m_NotificationObjectLifespan);

        m_NotificationObject.SetActive(false);
    }

    void ProcessUnitTerrainCommands(Vector3 point)
    {
        if (m_UnitCache.Count == 1)
        {
            m_UnitCache[0].GoToPosition(point, CommandType.Move);
            m_UnitCache[0].PlayMovementConfirmSound();
        }

        else if (m_UnitCache.Count > 1)
            GameEngine.FormationHandler.HandleGroupMovement
                (m_UnitCache.ToArray(), point, CommandType.Move);
    }

    void ProcessBuildingTerrainCommand(Vector3 point)
    {
        for(int i = 0; i < m_BuildingCache.Count; i++)
        {
            m_BuildingCache[i].SetRallyPoint(point);
        }
    }

    void HandleBuildingTargetCommand(RaycastHit target)
    {
        m_TargetCache = target.collider.GetComponent<FoundationBehavior>();
        if (m_TargetCache != null)
        {
            if (ObjectsSelected.Length > 0)
                m_TargetCache.gameObject.GetComponent<SelectableObject>().FlashSelection();

            if (GameEngine.PlayerStateHandler.IsTeamMember
                (((FoundationBehavior)m_TargetCache).ParentBuilding))
            {
                WorkerTask task = target.collider.gameObject.
                    GetComponent<FoundationBehavior>().ConstructionTask;

                SendWorkersToTask(task);
                return;
            }

            else
            {
                SendUnitsToAttackEntity(((FoundationBehavior)m_TargetCache));
            }
        }

        m_TargetCache = target.collider.GetComponent<BaseBuilding>();
        if(m_TargetCache != null)
        {
            if (ObjectsSelected.Length > 0)
                m_TargetCache.gameObject.GetComponent<SelectableObject>().FlashSelection();

            if (!GameEngine.PlayerStateHandler.IsTeamMember(m_TargetCache.gameObject))
            {
                SendUnitsToAttackEntity(((BaseBuilding)m_TargetCache));
                return;
            }

            else
            {
                HandleAlliedEntityCommand((BaseBuilding)m_TargetCache);
                return;
            }
        }
    }

    void HandleUnitTargetCommand(RaycastHit target)
    {
        m_TargetCache = target.collider.GetComponent<BaseUnit>();
        if (m_TargetCache != null)
        {
            if (ObjectsSelected.Length > 0)
                m_TargetCache.gameObject.GetComponent<SelectableObject>().FlashSelection();

            if (!GameEngine.PlayerStateHandler.IsTeamMember(m_TargetCache.gameObject))
            {
                SendUnitsToAttackEntity((BaseUnit)m_TargetCache);
            }

            else
            {
                HandleAlliedEntityCommand((BaseUnit)m_TargetCache);
            }
        }
    }

    void SendUnitsToAttackEntity(BaseEntity entity)
    {
        for (int i = 0; i < ObjectsSelected.Length; i++)
        {
            if ((m_DeltaUnit = ObjectsSelected[i].gameObject.GetComponent<BaseUnit>()) != null
                && GameEngine.PlayerStateHandler.IsTeamMember(m_DeltaUnit))
            {
                if (m_DeltaUnit.gameObject.GetComponent<ICombatant>() != null)
                {
                    m_DeltaUnit.PlayAttackConfirmSound();
                    m_DeltaUnit.UpdateCommandState(CommandType.Attack);
                    m_DeltaUnit.gameObject.GetComponent<ICombatant>().EngageEntity(entity);
                }
            }
        }
    }

    void SendWorkersToTask(WorkerTask task)
    {
        for(int i = 0; i < ObjectsSelected.Length; i++)
        {
            if((m_WorkerCache = ObjectsSelected[i].gameObject.GetComponent<BaseWorker>()) != null
                && GameEngine.PlayerStateHandler.IsTeamMember(m_WorkerCache))
            {
                if (task.NumberOfWorkersAssigned < GameEngine.ConstructionHandler.MaxWorkersPerBuilding)
                {
                    if (task.TaskStatus != TaskStatus.Completed)
                        m_WorkerCache.SetCurrentTask(task);
                }
            }

            if (task.NumberOfWorkersAssigned >= GameEngine.ConstructionHandler.MaxWorkersPerBuilding)
                break;
        }
    }

    void HandleAlliedEntityCommand(BaseEntity entity)
    {
        if (GameEngine.SelectionHandler.SelectedObjects.Count == 0)
            return;

        for (int i = 0; i < ObjectsSelected.Length; i++)
        {
            if ((m_DeltaUnit = ObjectsSelected[i].GetComponent<BaseUnit>()) != null
                    && GameEngine.PlayerStateHandler.IsTeamMember(ObjectsSelected[i].gameObject))
            {
                m_UnitCache.Add(m_DeltaUnit);
            }

            else if ((m_DeltaBuilding = ObjectsSelected[i].GetComponent<BaseBuilding>()) != null
                        && GameEngine.PlayerStateHandler.IsTeamMember(ObjectsSelected[i].gameObject))
            {
                m_BuildingCache.Add(m_DeltaBuilding);
            }
        }

        if (entity is BaseBuilding && entity.CurrentHealth < entity.MaxHealth)
        {
            SendWorkersToTask(new RepairBuildingTask((BaseBuilding)entity));
        }

        ProcessBuildingAllyTargetCommand(entity);

        if (entity is BaseBuilding && m_UnitCache.Count > 0)
        {
            m_DestinationCache = ((BaseBuilding)entity).DetermineApproach(m_UnitCache[0]);

            if (m_DestinationCache != null)
                ProcessUnitAllyTargetCommand(m_DestinationCache.Value);
            else
                ProcessUnitAllyTargetCommand(entity.Origin);
        }

        else
            ProcessUnitAllyTargetCommand
                (entity.gameObject.transform.position);

        m_UnitCache.Clear(); m_BuildingCache.Clear();
    }

    void ProcessUnitAllyTargetCommand(Vector3 point)
    {
        if (m_UnitCache.Count == 1)
            m_UnitCache[0].GoToPosition(point, CommandType.Move);

        else if (m_UnitCache.Count > 1)
            GameEngine.FormationHandler.HandleGroupMovement
                (m_UnitCache.ToArray(), point, CommandType.Move);
    }

    void ProcessBuildingAllyTargetCommand(BaseEntity entity)
    {
        for (int i = 0; i < m_BuildingCache.Count; i++)
        {
            m_BuildingCache[i].SetRallyEntity(entity);
        }
    }
}
