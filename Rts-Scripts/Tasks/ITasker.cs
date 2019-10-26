using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITasker
{
    Coroutine TaskRoutine { get; set; }

    ITaskable CurrentTask { get; set; }

    Queue<ITaskable> TaskQueue { get; set; }
}
