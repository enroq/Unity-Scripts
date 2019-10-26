using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWorker : BaseUnit, ITasker
{
    [SerializeField]
    private float m_WorkableRange = 3.0f;
    [SerializeField]
    private float m_ArrivalBufferDistance = 1.0f;

    public Coroutine TaskRoutine { get; set; }

    public ITaskable CurrentTask { get; set; }

    public Queue<ITaskable> TaskQueue { get; set; }

    HarvesterState m_HarvesterState;

    public float WorkableRange
    {
        get { return m_WorkableRange; }
        set { m_WorkableRange = value; }
    }

    public float ArrivalBufferDistance
    {
        get { return m_ArrivalBufferDistance; }
        set { m_ArrivalBufferDistance = value; }
    }

    public override void Start()
    {
        m_HarvesterState = gameObject.GetComponent<HarvesterState>();
        base.Start();
    }

    internal override void GoToPosition(Vector3 point, CommandType command)
    {
        if (m_HarvesterState != null)
            m_HarvesterState.ClearNavState();

        base.GoToPosition(point, command);
    }

    internal override void GoToPosition(Vector3 point)
    {
        CancelTask(); base.GoToPosition(point);
    }

    internal override void GoToPosition(Vector3 point, float stopDistance)
    {
        CancelTask(); base.GoToPosition(point, stopDistance);
    }

    internal void GoToTask(Vector3 taskPosition)
    {
        base.GoToPosition(taskPosition);
    }

    internal void GoToTask(Vector3 taskPosition, float stopDistance)
    {
        base.GoToPosition(taskPosition, stopDistance);
    }

    internal void CancelTask()
    {
        CurrentTask = null;
        StopAllCoroutines();
        UpdateCommandState(CommandType.None);
    }

    internal void SetCurrentTask(ITaskable task)
    {
        if (task is WorkerTask)
        {
            if (CurrentTask != null)
                ((WorkerTask)CurrentTask).NumberOfWorkersAssigned--;

            CurrentTask = task;

            if (task is ConstructionTask)
                UpdateCommandState(CommandType.Build);

            else if (task is RepairBuildingTask)
                UpdateCommandState(CommandType.Repair);

            if (m_HarvesterState != null)
                m_HarvesterState.ClearTargetNode();

            ((WorkerTask)CurrentTask).NumberOfWorkersAssigned++;

            WorkableRange =  m_ArrivalBufferDistance;

            GoToTask(((WorkerTask)CurrentTask).TaskPosition(this));
            TaskRoutine = StartCoroutine(AwaitArrival());
        }
    }

    internal bool InRangeOfTask()
    {
        if (GameEngine.DebugMode || m_DebugMode)
            Debug.Log(string.Format("({0}) Range To Task [Distance: {1}]", 
                gameObject.name, ((WorkerTask)CurrentTask).DistanceFromTask(this)));

        if (CurrentTask is WorkerTask)
        {
            if((CurrentTask is RepairBuildingTask && ((RepairBuildingTask)CurrentTask).TargetBuilding == null)
                || (CurrentTask is ConstructionTask && ((ConstructionTask)CurrentTask).TargetBuilding == null))
            {
                CancelTask();
                return false;
            }

            return ((WorkerTask)CurrentTask).DistanceFromTask(this) <= WorkableRange;
        }

        else
        {
            throw new UnityException
                (string.Format("{0}'s Current Task Is Not A Worker Task.", gameObject));
        }
    }

    IEnumerator AwaitArrival()
    {
        if (GameEngine.DebugMode || m_DebugMode)
            Debug.Log(string.Format("{0} Awaiting Arrival To Task..", gameObject));

        yield return new WaitUntil(() => InRangeOfTask());

        TaskRoutine = StartCoroutine(UpdateTaskProgress());
    }

    IEnumerator UpdateTaskProgress()
    {
        yield return new WaitForSeconds
            (GameEngine.ConstructionHandler.BuildIntervalDelay);

        if (CurrentTask != null)
        {
            CurrentTask.FurtherTaskProgress
                (GameEngine.ConstructionHandler.BuildIntervalProgress);

            if (CurrentTask.TaskStatus != TaskStatus.Completed && InRangeOfTask())
                TaskRoutine = StartCoroutine(UpdateTaskProgress());

            if (CurrentTask.TaskStatus == TaskStatus.Completed)
                UpdateCommandState(CommandType.None);

            if (GameEngine.DebugMode)
                Debug.Log(string.Format("{0} Furthering Progress Of Task.. ({1}/{2})",
                    gameObject, CurrentTask.TaskProgressLevel, CurrentTask.MaxProgressLevel));
        }
    }
}
