using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public enum NodeState
    {
        Running = 0,
        Failure = 1,
        Success = 2,
        Interrupted = 3,
        Aborted = 4,
        NONE = 16
    }
}
