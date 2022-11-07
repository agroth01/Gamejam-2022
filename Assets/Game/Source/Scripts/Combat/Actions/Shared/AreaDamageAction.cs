// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: This action will deal damage to multiple tiles, creating a single damage action for each tile.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDamageAction : ICombatAction
{
    private List<ICombatAction> m_actions;

    public void Execute()
    {
        foreach (ICombatAction action in m_actions)
        {
            action.Execute();
        }
    }

    public AreaDamageAction(List<ICombatAction> actions)
    {
        m_actions = actions;
    }
}
