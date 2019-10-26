//using System.Collections;
//using UnityEngine;
//using UnityEngine.AI;

//#pragma warning disable
//public class NavigationHandlerTheta : MonoBehaviour
//{
//    NavMeshAgent m_NavAgent;
//    NavMeshPath m_NavPath;
//    NavMeshObstacle m_NavObstacle;

//    Vector3 m_PathDestination = Vector3.zero;
//    Vector3 m_PathOrigin = Vector3.zero;

//    bool m_IsNavigating = false;
//    bool m_ExecutingCommand = false;

//    float m_StopDistanceCache;
//    float m_AccelerationCache;

//    Coroutine m_NavRoutine;

//    BaseEntity m_EntityRelative;

//    void Start()
//    {
//        m_NavPath = new NavMeshPath();

//        if ((m_NavAgent = GetComponent<NavMeshAgent>()) == null)
//            throw new MissingComponentException("Navigation Handler Missing Nav Agent.");

//        if ((m_NavObstacle = GetComponent<NavMeshObstacle>()) == null)
//            throw new MissingComponentException("Navigation Handler Missing Nav Obstacle.");

//        m_NavRoutine = StartCoroutine(DisableNavAgent());

//        m_StopDistanceCache = m_NavAgent.stoppingDistance;
//        m_AccelerationCache = m_NavAgent.acceleration;
//    }

//    void Update()
//    {
//        if (m_IsNavigating)
//        {
//            if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
//            {
//                if (GameEngine.DebugMode)
//                    Debug.Log(string.Format("{0} Has Terminated Navigation [Remaining Distance: {1} | Stopping Distance {2}]",
//                        gameObject, m_NavAgent.remainingDistance, m_NavAgent.stoppingDistance));

//                TerminateNavigation();
//            }
//        }
//    }

//    internal void AttachEntity(BaseEntity entity)
//    {
//        m_EntityRelative = entity;
//    }

//    internal void AcknowledgeCommand()
//    {
//        m_ExecutingCommand = true;
//    }

//    internal void TerminateMoveCommand()
//    {
//        m_ExecutingCommand = false;
//        if (m_EntityRelative.CurrentCommand == CommandType.Move)
//            m_EntityRelative.UpdateCommandState(CommandType.None);
//    }

//    void ZeroVelocity()
//    {
//        m_NavAgent.velocity = Vector3.zero;
//    }

//    void ObtainProperVelocity()
//    {
//        m_NavAgent.velocity = m_NavAgent.desiredVelocity;
//    }

//    IEnumerator InitializeNavigation()
//    {
//        m_NavObstacle.enabled = false;

//        yield return new WaitWhile
//            (() => m_NavObstacle.enabled);

//        m_NavAgent.enabled = true;
//        InitializePath();
//    }

//    IEnumerator InitializeGroupNavigation(GroupMovement group)
//    {
//        m_NavObstacle.enabled = false;

//        yield return new WaitWhile
//            (() => m_NavObstacle.enabled);

//        m_NavAgent.enabled = true;
//        InitializeGroupPath(group);
//    }

//    IEnumerator DisableNavAgent()
//    {
//        m_NavAgent.enabled = false;

//        yield return new WaitWhile
//            (() => m_NavAgent.enabled);

//        m_NavObstacle.enabled = true;
//    }

//    void FlickerNavAgent()
//    {
//        m_NavAgent.enabled = false;
//        m_NavAgent.enabled = true;
//    }

//    internal void TerminateNavigation()
//    {
//        if (!m_NavAgent.enabled)
//            return;

//        if (GameEngine.DebugMode)
//            Debug.Log(string.Format
//                ("{0} Has Terminated Navigation At {1} From Destination", gameObject, m_NavAgent.remainingDistance));

//        ZeroVelocity();
//        m_NavRoutine = StartCoroutine(DisableNavAgent());
//        m_IsNavigating = false;

//        m_EntityRelative.DisableWalkAnimation();

//        if (m_ExecutingCommand)
//            TerminateMoveCommand();

//        m_NavAgent.stoppingDistance = m_StopDistanceCache;
//        m_NavAgent.acceleration = m_AccelerationCache;
//    }

//    internal void UpdatePathVectors(Vector3 position)
//    {
//        m_PathDestination = position;
//        m_PathOrigin = transform.position;
//    }

//    internal void NavigateToPosition(Vector3 position, bool isCommand, float stopDistance)
//    {
//        if (isCommand)
//            AcknowledgeCommand();

//        NavigateToPosition(position, stopDistance);
//    }

//    internal void NavigateToPosition(Vector3 position, bool isCommand)
//    {
//        if (isCommand)
//            AcknowledgeCommand();

//        NavigateToPosition(position);
//    }

//    internal void NavigateToPosition(Vector3 position, float stopDistance)
//    {
//        m_NavAgent.stoppingDistance = stopDistance; NavigateToPosition(position);
//    }

//    internal void NavigateToPosition(Vector3 position)
//    {
//        UpdatePathVectors(position);
//        m_EntityRelative.EnableWalkAnimation();

//        m_NavRoutine = StartCoroutine(InitializeNavigation());
//    }

//    internal void NavigateToGroupPosition(GroupMovement group, bool isCommand, Vector3 position)
//    {
//        if (isCommand)
//            AcknowledgeCommand();

//        NavigateToGroupPosition(group, position);
//    }

//    internal void NavigateToGroupPosition(GroupMovement group, Vector3 position)
//    {
//        UpdatePathVectors(position);
//        m_EntityRelative.EnableWalkAnimation();

//        m_NavRoutine = StartCoroutine(InitializeGroupNavigation(group));
//    }

//    internal void InitializeGroupPath(GroupMovement group)
//    {
//        if (m_IsNavigating)
//            ObtainProperVelocity();

//        m_NavPath = new NavMeshPath();
//        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

//        if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
//        {
//            group.AddVectorToValidPositions(m_PathDestination);
//            m_NavAgent.SetPath(m_NavPath);
//            m_IsNavigating = true;

//            if (GameEngine.DebugMode)
//                Debug.Log(string.Format
//                    ("{0} Initialized Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
//        }

//        else
//        {
//            group.AddFailedNavAgent(this);

//            if (GameEngine.DebugMode)
//                Debug.Log(string.Format
//                    ("{0} Failed To Initialize Navigation, Adding To Group's Failed Nav Attempts.", gameObject));
//        }
//    }

//    internal void InitializePath()
//    {
//        if (m_IsNavigating)
//            ObtainProperVelocity();

//        m_NavPath = new NavMeshPath();
//        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

//        if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
//        {
//            m_NavAgent.SetPath(m_NavPath);
//            m_IsNavigating = true;

//            if (GameEngine.DebugMode)
//                Debug.Log(string.Format
//                    ("{0} Initialized Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
//        }

//        else
//        {
//            m_NavAgent.SetDestination(m_PathDestination);
//            if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
//                m_IsNavigating = true;

//            else if (GameEngine.DebugMode)
//            {
//                Debug.Log(string.Format
//                    ("{0} Failed To Initialize Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
//            }
//        }
//    }

//    internal void UpdatePathDestination(Vector3 position, float stopDistance)
//    {
//        m_NavAgent.stoppingDistance = stopDistance; UpdatePathDestination(position);
//    }

//    internal void UpdatePathDestination(Vector3 position)
//    {
//        if (!m_NavAgent.enabled)
//            return;

//        m_PathDestination = position;
//        m_NavPath = new NavMeshPath();
//        m_NavAgent.CalculatePath(m_PathDestination, m_NavPath);

//        if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
//        {
//            m_NavAgent.SetPath(m_NavPath);
//            m_IsNavigating = true;

//            if (GameEngine.DebugMode)
//                Debug.Log(string.Format
//                    ("{0} Initialized Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
//        }

//        else
//        {
//            m_NavAgent.SetDestination(m_PathDestination);
//            if (m_NavPath.status != NavMeshPathStatus.PathInvalid)
//                m_IsNavigating = true;

//            else if (GameEngine.DebugMode)
//            {
//                Debug.Log(string.Format
//                    ("{0} Failed To Initialize Navigation [Path Status: {1}]", gameObject, m_NavAgent.path.status));
//            }
//        }
//    }

//    void OnDrawGizmos()
//    {
//        if (GameEngine.DebugMode)
//        {
//            if (m_NavAgent == null || m_NavAgent.path == null)
//                return;

//            var line = GetComponent<LineRenderer>();
//            if (line == null)
//            {
//                line = gameObject.AddComponent<LineRenderer>();
//                line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.yellow };

//                line.startWidth = 0.15f;
//                line.endWidth = 0.15f;

//                line.startColor = Color.yellow;
//                line.endColor = Color.yellow;
//            }

//            var path = m_NavAgent.path;

//            line.positionCount = path.corners.Length;

//            for (int i = 0; i < path.corners.Length; i++)
//                line.SetPosition(i, path.corners[i]);
//        }
//    }
//}

///*
// *  ///<remarks>
//    / Commented Code Provides Behavior Desired (Keeping Units From Going Past Destination At High Speeds). But At What Cost?
//    / Remember to calculate MinChallengegDistance and add ResetDistanceCache() to path starts if reimplemented.
//    / Should probably change the min challenge distance to a factor of the stopping distance instead of a % of total path.
//    /</remarks>

//    float m_MinChallengeDistance = 0.0f;       //Minimum Distance Required To Query If Agent Has Passed Destination.
//    float m_MinChallengeDistanceRatio = 0.35f; //Factor By Which To Multiply Total Path Distance To Obtain Minimum Challenge Distance.

//    / <summary>
//    / Cache Values Stored After Distance Checks To Be Compared Against Next Frame.
//    / </summary>
//    float m_DistToOriginCache = -1.0f;
//    float m_DistFromDestCache = -1.0f;

//    / <summary>
//    / Marginal value added to distances upon challenge 
//    / to compensate for unity float rounding discrepancies.
//    / </summary>
//    float m_DistanceChallengeThreshold = 0.01f;

//    void Update()
//    {
//        if (m_IsNavigating)
//        {
//            Vector3 position = transform.position;

//            float originDistance = Vector3.Distance(position, m_PathOrigin);
//            float destinationDistance = Vector3.Distance(position, m_PathDestination);

//            if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
//            {
//                TerminateNavigation();
//                if (GameEngine.DebugMode)
//                    Debug.Log(string.Format("{0} Has Terminated Navigation [Remaining Distance: {1} | Stopping Distance {2}]",
//                        gameObject, m_NavAgent.remainingDistance, m_NavAgent.stoppingDistance));
//            }

//            else if (HasPassedDestination(originDistance, destinationDistance))
//            {
//                if (GameEngine.DebugMode)
//                    Debug.Log(string.Format("{0} Has Terminated Navigation [Origin: ({1}|{2}) Destination: ({3}|{4})]",
//                        gameObject, originDistance, m_DistToOriginCache, destinationDistance, m_DistFromDestCache));

//                TerminateNavigation();
//            }

//            m_DistFromDestCache = destinationDistance;
//            m_DistToOriginCache = originDistance;
//        }
//    }

//    / <summary>
//    / Checks current distance to make sure we're close enough to the destination
//    / to challenge the distance from origin position.
//    / </summary>
//    / <remarks>
//    / This prevents undesirable navigation terminations when
//    / nav agents in a group move away from both the origin
//    / and destination when trying to navigate around a crowd.
//    / </remarks>
//    bool WithinChallengeDistance()
//    {
//        return m_NavAgent.remainingDistance <= m_MinChallengeDistance;
//    }

//    / <summary>
//    / Determines whether or not distance caches are in their 'initial' state.   
//    / </summary>
//    / <remarks>
//    / This prevents the cache challenge from always returning true without
//    / relying on setting the cache to an abritrary number that will always
//    / be larger than the distances could be (such as mathf.infinity).
//    / </remarks>
//    internal bool CachesInitialized()
//    {
//        return (m_DistToOriginCache != -1.0f || m_DistFromDestCache != -1.0f);
//    }

//    / <summary>
//    / Determines whether or not the navigating object's distance from both
//    / the origin and destination have increased from the previous frame.
//    / This keeps navigating objects from going past their destination.
//    / </summary>
//    / <param name="originDistance">Current Distance From Origin Position.</param>
//    / <param name="destinationDistance">Current Distance To Destination Postion.</param>
//    / <returns>
//    / Returns true if both origin distance and destination distance
//    / are greater than their cached values from the last frame.
//    / </returns>
//    internal bool HasPassedDestination(float originDistance, float destinationDistance)
//    {
//        return
//            CachesInitialized() && WithinChallengeDistance()
//                && (originDistance > (m_DistToOriginCache + m_DistanceChallengeThreshold)
//                && destinationDistance > (m_DistFromDestCache + m_DistanceChallengeThreshold));
//    }

//    / <summary>
//    / Returns distance caches to initial state.
//    / Use when when ending or starting a path.
//    / </summary>
//    internal void ResetDistanceCaches()
//    {
//        m_DistFromDestCache = -1.0f;
//        m_DistToOriginCache = -1.0f;
//    }

//            m_PositionRelativeToTarget = m_NavAgent.steeringTarget - transform.position;
//            m_PositionRelativeToTarget.y = transform.position.y / 2;

//            m_TargetRotation = Quaternion.LookRotation(m_PositionRelativeToTarget);
//            transform.rotation = Quaternion.Slerp
//                (transform.rotation, m_TargetRotation, Time.deltaTime * m_TurnSpeed);
//    */
