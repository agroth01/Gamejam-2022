// /* --------------------
// -----------------------
// Creation date: 12/11/2022
// Author: Alex
// Description: A single turn queue item. This is used to store a unit and its action.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TurnQueueItem
{
    public Unit Unit;
    public ICombatAction Action;

    public TurnQueueItem(Unit unit, ICombatAction action)
    {
        this.Unit = unit;
        this.Action = action;
    }
}
