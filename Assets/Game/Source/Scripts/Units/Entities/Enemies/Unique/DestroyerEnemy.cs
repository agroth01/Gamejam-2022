// /* --------------------
// -----------------------
// Creation date: 14/11/2022
// Author: Alex
// Description: This enemy will attempt to charge towards the player if within range in a straight line.
//              If the player is one tile away, a normal melee attack will be performed instead.
//              If not close enough for melee, but not in a straight line, enemy will throw a grenade causing poison hazard.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerEnemy : Enemy
{
    [Header("Charge")]
    public int m_chargeRange;
    public int m_chargeDamage;
    public int m_chargeSpeed;
    private bool m_isCharging;
    private Vector2Int m_chargePosition;

    [Header("Iron Fist")]
    public int m_fistDamage;

    [Header("Toxic Grenade")]
    public int m_grenadeRange;
    public int m_grenadeRadius;

    public override void DetermineAction()
    {
        // Charge will be prioritized if the enemy is in a straight line, and is more than 1 tile away.
        // If the player is 1 tile away, a normal melee attack will be performed instead.
        // Else, ranged attack will be performed.
        Vector2Int playerPosition = GetPlayer().GridPosition;
        int playerDistance = Grid.Instance.GetDistance(GridPosition, playerPosition);

        Debug.Log(playerDistance <= m_chargeRange);
        if (playerDistance <= m_chargeRange && !Grid.Instance.IsAdjacent(GridPosition, playerPosition) && Grid.Instance.InStraightLine(GridPosition, playerPosition))
            Charge();

        else if (Grid.Instance.IsAdjacent(GridPosition, playerPosition))
            Fist();

        else if (playerDistance <= m_grenadeRange)
            Grenade();
    }

    /// <summary>
    /// Charges towards the player. Deals damage to the first unit or obstacle hit.
    /// </summary>
    private void Charge()
    {
        // First we have to determine if we can charge the whole way, or if we have to stop early.
        // If we have to stop, determine what we hit and deal damage to the tile stopping enemy.
        Vector2Int playerPosition = GetPlayer().GridPosition;
        Direction direction = Grid.Instance.GetDirectionTo(playerPosition, GridPosition);

        Vector2Int chargePosition = GridPosition;
        Vector2Int nextPosition = Grid.Instance.PositionWithDirection(chargePosition, direction);
        int distance = 0;

        // I pray this does not cause an infinite while loop. Tired of restarting editor.
        while (distance < m_chargeRange)
        {
            if (!Grid.Instance.IsTileFree(nextPosition))
            {
                // We hit something. Deal damage to it.
                ICombatAction action = new SingleDamageAction(nextPosition, m_chargeDamage);
                SetAction(action);
                return;
            }

            else
            {
                // We hit nothing. Continue charging.
                chargePosition = nextPosition;
                nextPosition = Grid.Instance.PositionWithDirection(chargePosition, direction);
                distance++;
            }
        }

        // A bit hacky, but we have to do this to make the enemy move.
        m_chargePosition = chargePosition;
        m_isCharging = true;
    }

    private void Fist()
    {
        
    }

    private void Grenade()
    {

    }

    public override void DetermineMove()
    {
        // Ignore if we are already next to the player
        if (Grid.Instance.IsAdjacent(GridPosition, GetPlayer().GridPosition))
            return;

        // Enemy will always attempt to move to an adjacent tile to the player.
        // Don't move if there are no available positions.
        // TODO: Pick closest possible position instead.
        List<Vector2Int> adjacentPositions = Grid.Instance.GetFreeAdjacentTiles(GetPlayer().GridPosition);
        if (adjacentPositions.Count == 0)
            return;

        // Go through each adjacent tile to determine which one is the closest to enemy current position.
        Vector2Int targetPosition = adjacentPositions[0];
        foreach (Vector2Int position in Grid.Instance.GetFreeAdjacentTiles(GetPlayer().GridPosition))
        {
            // Pick the shortest path to the player
            int distance = Grid.Instance.GetDistance(GridPosition, position);
            if (distance > Grid.Instance.GetDistance(targetPosition, position))
            {
                targetPosition = position;
            }
        }

        // Move to the closest position
        List<Vector2Int> path = Grid.Instance.GetPath(GridPosition, targetPosition);

        // Remove all points in path that is outside of movement range of enemy.
        if (path.Count > MovementSpeed)
        {
            path.RemoveRange(MovementSpeed, path.Count - MovementSpeed);
        }

        // Perform the move
        ICombatAction move = new MoveAction(this, path, m_physicalMovementSpeed);
        SetAction(move);
    }
}
