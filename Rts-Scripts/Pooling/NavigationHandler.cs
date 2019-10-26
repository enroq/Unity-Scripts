using System.Collections;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable
public class NavigationHandler : MonoBehaviour
{
    NavMeshAgent m_NavAgent;
    NavMeshPath m_NavPath;
    NavMeshObstacle m_NavObstacle;

    Vector3 m_PathDestination = Vector3.zero;
    Vector3 m_PathOrigin = Vector3.zero;

    [SerializeField]
    bool m_DebugMode;
    [SerializeField]
    bool m_IsNavigating = false;

    bool m_ExecutingCommand = false;

    float m_StopDistanceCache;
    float m_AccelerationCache;

    BaseEntity m_EntityRelative;

    void Start()
    {
        m_NavPath = new NavMeshPath();

        if ((m_NavAgent = GetComponent<NavMeshAgent>()) == null)
            throw new MissingComponentException("Navigation Handler Missing Nav Agent.");

        m_StopDistanceCache = m_NavAgent.stoppingDistance;
        m_AccelerationCache = m_NavAgent.acceleration;
    }

    void Update()
    {
        if (m_IsNavigating)
        {
            if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
            {
                if (GameEngine.DebugMode)
                    Debug.Log(string.Format("{0} Has Terminated Navigation [Remaining Distance: {1} | Stopping Distance {2}]",
                        gameObject, m_NavAgent.remainingDistance, m_NavAgent.stoppingDistance));

                TerminateNavigation();
            }
        }
    }

    internal void AttachEntity(BaseEntity entity)
    {
        m_EntityRelative = entity;
    }

    internal void AcknowledgeCommand()
    {
        m_ExecutingCommand = true;
    }

    internal void TerminateMoveCommand()
    {
        m_ExecutingCommand = false;
        if (m_EntityRelative.CurrentCommand == CommandType.Move)
            m_EntityRelative.UpdateCommandState(CommandType.None);
    }

    void ZeroVelocity()
    {
        m_NavAgent.velocity = Vector3.zero;
    }

    void ObtainProperVelocity()
    {
        m_NavAgent.velocity = m_NavAgent.desiredVelocity;
    }

    void FlickerNavAgent()
    {
        m_NavAgent.enabled = false;
        m_NavAgent.enabled = true;
    }

    internal void TerminateNavigation()
    {
        if (!m_NavAgent.enabled)
            return;

        m_EntityRelative.DisableWalkAnimation();

        ZeroVelocity();
        m_IsNavigating = false;

        if (m_ExecutingCommand)
            TerminateMoveCommand();

        if (GameEngine.DebugMode)
            Debug.Log(string.Format
                ("{0} Has Terminated Navigation At {1} From Destination", gameObject, m_NavAgent.remainingDistance));

        FlickerNavAgent();

        m_NavAgent.stoppingDistance = m_StopDistanceCache;
        m_NavAgent.acceleration = m_AccelerationCache;
    }

    internal void UpdatePathVectors(Vector3 position)
    {
        m_PathDestination = position;
        m_PathOrigin = transform.position;
    }

    internal void NavigateToPosition(Vector3 position, bool isCommand, float stopDistance)
    {
        if (isCommand)
            AcknowledgeCommand();

        NavigateToPosition(position, stopDistance);
    }

    internal void NavigateToPosition(Vector3 position, bool isCommand)
    {
        if (isCommand)
            AcknowledgeCommand();

        NavigateToPosition(position);
    }

    internal void NavigateToPosition(Vector3 position, float stopDistance)
    {
        m_NavAgent.stoppingDistance = stopDistance; NavigateToPosition(position);
    }

    internal void NavigateToPosition(Vector3 position)
    {
        UpdatePathVectors(position);

        InitializePath();
    }

    internal void NavigateToGroupPosition(GroupMovement group, bool isCommand, Vector3? position)
    {
        if (isCommand)
            AcknowledgeCommand();

        NavigateToGroupPosition(group, position);
    }

    internal void NavigateToGroupPosition(GroupMovement group, Vector3? position)
    {
        if (position != null)
        {
            UpdatePathVectors((Vector3)position);

            InitializeGroupPath(group);
        }
    }

    internal void InitializeGroupPath(GroupMovement group)
    {
        if (m_IsNavigating)
            ObtainProperVelocity();

        m_NavPath = new NavMeshPath();
        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

        if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
        {
            group.AddVectorToValidPositions(m_PathDestination);
            m_EntityRelative.EnableWalkAnimation();
            m_NavAgent.SetPath(m_NavPath);
            m_IsNavigating = true;

            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Initialized Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
        }

        else
        {
            group.AddFailedNavAgent(this);

            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Failed To Initialize Navigation, Adding To Group's Failed Nav Attempts.", gameObject));
        }
    }

    internal void InitializePath()
    {
        if (m_IsNavigating)
            ObtainProperVelocity();

        m_NavPath = new NavMeshPath();
        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

        if(m_NavPath.status != NavMeshPathStatus.PathInvalid)
        {
            m_EntityRelative.EnableWalkAnimation();
            m_NavAgent.SetPath(m_NavPath);
            m_IsNavigating = true;

            if(GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Initialized Navigation [Path Status: {1}]", gameObject, m_NavPath.status));
        }

        else
        {
            m_NavAgent.SetDestination(m_PathDestination);

            if (m_NavAgent.path.status != NavMeshPathStatus.PathInvalid)
            {
                m_EntityRelative.EnableWalkAnimation();
                m_IsNavigating = true;
            }

            else if (GameEngine.DebugMode)
            {
                Debug.Log(string.Format
                    ("{0} Failed To Initialize Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
            }
        }
    }

    internal void UpdatePathDestination(Vector3 position, float stopDistance)
    {
        m_NavAgent.stoppingDistance = stopDistance; UpdatePathDestination(position);
    }

    internal void UpdatePathDestination(Vector3 position)
    {
        if (!m_NavAgent.enabled)
            return;

        m_PathDestination = position;
        m_NavPath = new NavMeshPath();
        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

        if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
        {
            m_NavAgent.SetPath(m_NavPath);
            m_IsNavigating = true;

            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("{0} Updated Navigation [Path Status: {1}]", gameObject, m_NavPath.status));
        }

        else
        {
            m_NavAgent.SetDestination(m_PathDestination);
            if (m_NavAgent.path.status != NavMeshPathStatus.PathInvalid)
                m_IsNavigating = true;

            else if (GameEngine.DebugMode)
            {
                Debug.Log(string.Format
                    ("{0} Failed To Initialize Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
            }
        }
    }

    void OnDrawGizmos()
    {
        if (GameEngine.DebugMode || m_DebugMode)
        {
            if (m_NavAgent == null || m_NavAgent.path == null)
                return;

            var line = GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.yellow };

                line.startWidth = 0.15f;
                line.endWidth = 0.15f;

                line.startColor = Color.yellow;
                line.endColor = Color.yellow;
            }

            var path = m_NavAgent.path;

            line.positionCount = path.corners.Length;

            for (int i = 0; i < path.corners.Length; i++)
                line.SetPosition(i, path.corners[i]);
        }
    }
}