//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class NavigationHandlerGamma : KaryonBehaviour
//{
//    NavMeshAgent m_NavAgent;

//    NavMeshPath m_PrimaryPath;
//    NavMeshPath m_SubPath;

//    bool m_IsNavigating;
//    int m_CurrentPathIndex = 0;

//    Vector3 m_PathDestination = Vector3.zero;

//    Vector3[] m_PathWayPoints;

//    float m_StoppingDistanceCache;
//    float m_AccelerationCache;
//    float m_CurrentDistanceRemaining;

//    public float m_StopDistanceThreshold = 1.5f;
//    public float m_StopAccelFactor = 1.5f;

//    public float m_VelocityDampener = 0.575f;

//    void Start()
//    {
//        m_PrimaryPath = new NavMeshPath();

//        if ((m_NavAgent = GetComponent<NavMeshAgent>()) == null)
//            throw new MissingComponentException("Navigation Handler Missing Nav Agent.");

//        m_StoppingDistanceCache = m_NavAgent.stoppingDistance;
//        m_AccelerationCache = m_NavAgent.acceleration;
//    }

//    void Update()
//    {
//        if (m_IsNavigating)
//        {
//            if (GameEngine.DebugMode)
//                Debug.Log(string.Format("Nav Agent Distance Remaining: {0}", m_NavAgent.remainingDistance));

//            if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance * m_StopDistanceThreshold)
//            {
//                m_NavAgent.acceleration = m_AccelerationCache * m_StopAccelFactor;
//            }

//            if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
//            {
//                if (m_PathWayPoints.Length > m_CurrentPathIndex)
//                    MoveToNextWayPoint();
//                else
//                    TerminateNavigation();
//            }
//        }
//    }

//    internal void NavigateToPosition(Vector3 destination, GroupMovement group = null)
//    {
//        m_PathDestination = destination;
//        FlickerNavAgent();

//        if (m_IsNavigating)
//            TerminateNavigation();

//        if (PathIsNavigable())
//            MoveToNextWayPoint();

//        //else if (group != null)
//        //    group.AddFailedNavAgent(this);
//    }

//    internal void NavigateToPosition(Vector3 destination, float stopDistance)
//    {
//        m_NavAgent.stoppingDistance = stopDistance; NavigateToPosition(destination);
//    }

//    bool PathIsNavigable()
//    {
//        m_PrimaryPath = new NavMeshPath();
//        m_NavAgent.CalculatePath(m_PathDestination, m_PrimaryPath);

//        if (m_PrimaryPath.status != NavMeshPathStatus.PathInvalid)
//        {
//            m_PathWayPoints = m_PrimaryPath.corners;
//            return true;
//        }

//        else
//        {
//            m_NavAgent.SetDestination(m_PathDestination);
//            if (m_NavAgent.path.status != NavMeshPathStatus.PathInvalid)
//            {
//                m_PrimaryPath = m_NavAgent.path;
//                m_PathWayPoints = m_PrimaryPath.corners;
//                return true;
//            }
//        }

//        if(GameEngine.DebugMode)
//            Debug.Log(string.Format("{0} Unable To Navigate To [{1}]; Path Status ({2})",
//                gameObject, m_PathDestination, m_PrimaryPath.status));

//        return false;
//    }

//    void MoveToNextWayPoint()
//    {
//        DampenVelocity();
//        m_SubPath = new NavMeshPath();

//        m_NavAgent.CalculatePath(m_PathWayPoints[m_CurrentPathIndex], m_SubPath);

//        if (m_SubPath.status != NavMeshPathStatus.PathInvalid)
//        {
//            m_NavAgent.SetPath(m_SubPath);
//            m_IsNavigating = true;
//            m_CurrentPathIndex++;
//        }

//        else
//        {
//            m_NavAgent.SetDestination(m_PathWayPoints[m_CurrentPathIndex]);
//            if (m_NavAgent.path.status != NavMeshPathStatus.PathInvalid)
//            {
//                m_IsNavigating = true; m_CurrentPathIndex++;
//            }
//        }
//    }

//    internal void FlickerNavAgent()
//    {
//        m_NavAgent.enabled = false;
//        m_NavAgent.enabled = true;
//    }

//    void DampenVelocity()
//    {
//        m_NavAgent.velocity = (m_NavAgent.velocity * m_VelocityDampener);
//    }

//    void TerminateNavigation()
//    {
//        m_CurrentPathIndex = 0;
//        m_IsNavigating = false;

//        //m_NavAgent.stoppingDistance = m_StoppingDistanceCache;
//        m_NavAgent.acceleration = m_AccelerationCache;
//    }

//    void OnDrawGizmos()
//    {
//        if (GameEngine.DebugMode)
//        {
//            if (m_NavAgent == null || m_PrimaryPath == null)
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

//            var path = m_PrimaryPath;

//            line.positionCount = path.corners.Length;

//            for (int i = 0; i < path.corners.Length; i++)
//                line.SetPosition(i, path.corners[i]);

//        }
//    }
//}