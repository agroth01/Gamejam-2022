// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: A combat action tied to a certain weight. This is used to pick some combat actions with more frequency
//              than others. If all actions should be equally likely, all weights should be the same.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WeightedCombatAction
{
    public ICombatAction combatAction;
    public int weight;

    public WeightedCombatAction(ICombatAction combatAction, int weight)
    {
        this.combatAction = combatAction;
        this.weight = weight;
    }
}
