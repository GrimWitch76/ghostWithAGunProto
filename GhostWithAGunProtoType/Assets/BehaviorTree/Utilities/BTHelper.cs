using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BT.Utilities
{
    public static class BTHelper
    {
        /// <summary>
        /// Shuffles a list or an array of objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ShuffleList<T>(IList<T> list)
        {
            if (list == null)
            {
                return;
            }

            if (list.Count is 0 or 1)
            {
                return;
            }

            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
        }
    }
}