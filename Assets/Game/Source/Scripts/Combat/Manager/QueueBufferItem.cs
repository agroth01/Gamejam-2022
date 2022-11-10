// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: An item in the queue buffer for turns. This is used for deciding in what order the actions should be
//              added to the turn queue.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct QueueBufferItem
{
    public int Priority;
    public ICombatAction Action;

    public QueueBufferItem(int priority, ICombatAction action)
    {
        Priority = priority;
        Action = action;
    }
}
