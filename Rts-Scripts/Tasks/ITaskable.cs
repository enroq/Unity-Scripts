using UnityEngine;

public enum TaskStatus
{
    Unstarted,
    Incomplete,
    Completed
}

public interface ITaskable
{
    Sprite TaskIcon { get; }

    TaskStatus TaskStatus { get; set; }

    int TaskProgressLevel { get; set; }

    int MaxProgressLevel { get; set; }

    void FurtherTaskProgress(int i);

    void UpdateTaskStatus();
}

public interface WorkerTask : ITaskable
{
    int NumberOfWorkersAssigned { get; set; }

    float ArrivalOffset { get; }

    float DistanceFromTask(BaseEntity entity);

    Vector3 TaskPosition(BaseEntity entity);
}
