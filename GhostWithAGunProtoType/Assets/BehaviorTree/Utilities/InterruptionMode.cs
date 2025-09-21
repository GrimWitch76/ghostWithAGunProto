using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT.Utilities
{
    /// <summary>
    /// Controls whether a condition interrupts execution immediately or waits until completion.
    /// </summary>
    public enum InterruptionMode
    {
        /// <summary>
        /// Interrupts execution as soon as the condition becomes false.
        /// </summary>
        [Tooltip("The condition is checked every frame. If the condition is false, the child node is interrupted.")]
        Reactive = 0,
        /// <summary>
        /// Allows the sequence to finish before re-evaluating the condition.
        /// </summary>
        [Tooltip("The condition is checked only once after a success or a failure. If the condition is false, the child node is interrupted.")]
        Latched = 1,
    }
}
