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

    public override void DetermineMove()
    {
        // To begin with, if the enemy is adjacent to the player, the enemy will not move.
        Vector2Int playerPosition = GetPlayer().GridPosition;
        if (Grid.Instance.IsAdjacent(playerPosition, GridPosition)) return;

        // To start with, we will attempt to get a valid move position with hazards
        // baked into the mesh. This way, the enemy will always try to avoid hazards if possible.
        Grid.Instance.BakeWithHazards();
        List<Vector2Int> path = GetPath();
        if (path == null)
        {
            // If it gets here, it means there were no valid path with hazards baked into mesh.
            // Therefore, we try again, but while ignoring hazards.
            Grid.Instance.BakeNavMesh();
            path = GetPath();

            // If there still is no path, we just return and skip moving
            if (path == null) return;
        }

        // Now that a path is determined, we can move the enemy along the path. Remove all points in
        // path that is outside of movement range of enemy.
        if (path.Count > MovementSpeed)
        {
            path.RemoveRange(MovementSpeed, path.Count - MovementSpeed);          
        }

        // Finally, we create the action for moving and add it to queue once more.
        ICombatAction move = new MoveAction(this, path, m_physicalMovementSpeed);
        SetAction(move);
    }

    /// <summary>
    /// Finds the position to move into.
    /// </summary>
    /// <returns></returns>
    private List<Vector2Int> GetPath()
    {
        // Find all the possible adjacent tiles to the player
        Vector2Int playerPos = GetPlayer().GridPosition;
        List<Vector2Int> potentialPositions = Grid.Instance.GetFreeAdjacentTiles(playerPos);

        if (potentialPositions.Count == 0)
        {
            // In the case there are no adjacent tiles, we don't want to move. Maybe add some other
            // behaviour here later. TODO.
            return null;
        }

        // Now we go through each of the potential positions and determine which one is the
        // closest to this enemy.
        Vector2Int targetPosition = potentialPositions[0];
        foreach (Vector2Int potentialPosition in potentialPositions)
        {
            // If the potential position is closer than the current target position, we set it as the new target position.
            if (Grid.Instance.GetDistance(GridPosition, potentialPosition) < Grid.Instance.GetDistance(GridPosition, targetPosition))
            {
                targetPosition = potentialPosition;
            }
        }

        // Now we have the target position, we can attempt to get a path for it.
        List<Vector2Int> path = Grid.Instance.GetPath(GridPosition, targetPosition);
        if (path != null)
        {
            // If we have a path, we can return it.
            return path;
        }

        // As default fallback, return null to notify that no path was found.
        return null;
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
