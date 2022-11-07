// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: This is a very common type of enemy that will only attack with melee attacks.
// To-do: Find a way to move the code for reacting to being moved into Enemy class somehow.
// -----------------------
// ------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsman : Enemy
{
    [Header("Swordsman")]
    [SerializeField] private int m_damage;

    private GameObject m_highlight;
    private Direction m_attackDirection;
    private ICombatAction m_action;


    public override void DetermineAction()
    {
        if (m_highlight != null) Destroy(m_highlight);

        // Create attack in random direction
        m_attackDirection = (Direction)typeof(Direction).GetRandomValue();
        Vector2Int attackPosition = Grid.Instance.PositionWithDirection(GridPosition, m_attackDirection);

        m_action = new SingleDamageAction(attackPosition, m_damage);
        SetAction(m_action);

        m_highlight = Grid.Instance.HighlightTile(attackPosition, Color.red);
    }

    public override void OnFinishedMoving()
    {
        // Ignore if we don't have an action queued.
        if (m_action == null) return;

        // Clear current action from queue and update it with new position in mind
        RemoveAction(m_action);
        Vector2Int attackPosition = Grid.Instance.PositionWithDirection(GridPosition, m_attackDirection);
        m_action = new SingleDamageAction(attackPosition, m_damage);
        SetAction(m_action);

        // We need to update the position of the highlight. Since there is no functionality to move the highlight, we will just destroy it and create a new one.
        Destroy(m_highlight);
        m_highlight = Grid.Instance.HighlightTile(attackPosition, Color.red);        
    }
}
