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

public class MeleeEnemy : Enemy
{
    [Header("Swordsman")]
    [SerializeField] private int m_damage;
    [SerializeField] private float m_physicalMovementSpeed; // How fast unit moves from til to tile
    
    private Direction m_attackDirection;
    private ICombatAction m_action;

    /// <summary>
    /// This is called right before the players turn starts. In other words, this will get called AFTER DetermineMove().
    /// </summary>
    public override void DetermineAction()
    {
        // Here, enemy will deal damage to the tile the player is on if the enemy is adjecent to player.
        // If enemy is not adjecent, enemy will perform alternate action.
        Vector2Int playerPosition = Grid.Instance.GetGridPosition(GetPlayer().transform.position);
        if (Grid.Instance.IsAdjacent(playerPosition, GridPosition))
        {
            // The player is adjecent. We get the direction to the player and create an attack action in that direction.
            Direction directionToPlayer = Grid.Instance.GetDirectionTo(playerPosition, GridPosition);
            Vector2Int attackPosition = Grid.Instance.PositionWithDirection(GridPosition, directionToPlayer);

            // Cache the attack direction in case the enemy gets pushed, so that we can recalculate new attack position
            // later.
            m_attackDirection = directionToPlayer;

            // Store the action as a private variable, so that we can remove it later if we need to.
            m_action = new SingleDamageAction(attackPosition, m_damage);
            SetAction(m_action);
        }

        else
        {
            // There is no secondary action for the melee enemy as of now,
            // so this will just be left blank to skip the turn.
        }
    }

    /// <summary>
    /// This is called after perfoming action queued from last turn. Will be called BEFORE DetermineAction().
    /// </summary>
    public override void DetermineMove()
    {
        // If enemy is not adjecent to the player, enemy will move to become adjecent to player.
        // Otherwise, enemy skips moving.
        Vector2Int playerPosition = Grid.Instance.GetGridPosition(GetPlayer().transform.position);
        if (Grid.Instance.IsAdjacent(playerPosition, GridPosition)) return;

        // We know now that we are not adjecent and need to move towards player. We find the tile adjecent
        // to the player that is closest, then calculate the path towards that tile and move as much as we can.
        Direction direction = Grid.Instance.GetDirectionTo(GridPosition, playerPosition);
        List<Vector2Int> path = Grid.Instance.GetPath(GridPosition, Grid.Instance.PositionWithDirection(playerPosition, direction));

        // Remove all paths that are over the movement range of the enemy. This will be everything after the index
        // MovementSpeed - 1.
        if (path != null)
        {
            if (path.Count > MovementSpeed)
                path.RemoveRange(MovementSpeed, path.Count - MovementSpeed);

            // Finally, we create the action for moving and add it to queue once more.
            ICombatAction move = new MoveAction(this, path, m_physicalMovementSpeed);
            SetAction(move);
        }

        else
        {
            Debug.Log("No valid path was found to player.");
        }
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
    }
}
