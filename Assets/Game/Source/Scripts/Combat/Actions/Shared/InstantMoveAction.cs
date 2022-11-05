// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: This action will allow the entity to instantly move to any position on the grid.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMoveAction : ICombatAction
{
    // The entity that wants to move
    private Entity m_entity;

    // Where we will move to.
    private Vector2Int m_targetPosition;

    public void Execute()
    {
        m_entity.SetPosition(m_targetPosition.x, m_targetPosition.y);
    }

    public InstantMoveAction(Entity entity, Vector2Int targetPosition)
    {
        m_entity = entity;
        m_targetPosition = targetPosition;
    }
}
