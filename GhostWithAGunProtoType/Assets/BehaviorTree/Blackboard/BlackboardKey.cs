using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    [System.Serializable]
    public class BlackboardKey
    {
        [field: SerializeField] public string Value { get; set; }
    }
}
