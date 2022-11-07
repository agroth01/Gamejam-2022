// /* --------------------
// -----------------------
// Creation date: 04/11/2022
// Author: Alex
// Description: This is the main class for the player entity. It is responsible for setting up the player's health and other
//              things.
// -----------------------
// ------------------- */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity, IDamagable
{
    [Header("Health")]
    [SerializeField] private int m_maxHealth = 3;
    [SerializeField] private int m_startingHealth = 3;

    [Header("Action Points")]
    [SerializeField] private int m_maxActionPoints = 5;
    [SerializeField] private int m_startingActionPoints = 5;
    [SerializeField] private int m_actionPointPoolSize = 3;

    [Header("Movement")]
    [SerializeField] private int m_moveCostPerTile;
    [SerializeField] private float m_moveSpeed;

    [Header("Melee")]
    [SerializeField] private int m_meleeCost;
    [SerializeField] private int m_meleeRange;
    [SerializeField] private int m_meleeDamage;

    [Header("Mind blast")]
    [SerializeField] private int m_mindBlastCost;
    [SerializeField] private int m_mindBlastPushForce;

    // AP
    private ActionPoints m_actionPoints;

    public Health Health
    {
        get { return m_health; }
    }

    public ActionPoints ActionPoints
    {
        get { return m_actionPoints; }
    }

    public override void Awake()
    {
        base.Awake();
        InitializeActionPoints();
    }


    public override void TakeDamage(int damage)
    {
        Debug.Log("Player took damage: " + damage);
        m_health.Damage(damage);
    }

    public void RestoreAP()
    {
        m_actionPoints.RestoreActionPoints();
    }

    #region Debugging

    [Button("Add shield")]
    public void AddShield()
    {
        AddStatusEffect(new ShieldStatusEffect(5));
    }

    #endregion

    #region Actions

    /// <summary>
    /// Enables move selection mode, where the player will be able to select a tile to move to.
    /// </summary>
    public void StartMoveSelection()
    {
        if (m_actionPoints.TotalActionPoints == 0)
        {
            return;
        }

        StartCoroutine(MoveSelection());
    }

    /// <summary>
    /// Draws a line from the player to the hovered tile for as long as the player hasn't clicked yet,
    /// or have not pressed escape.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveSelection()
    {
        LineRenderer lineRenderer = CreateLineRenderer();

        Vector2Int playerPosition = Grid.Instance.GetGridPosition(transform.position);
        List<Vector2Int> currentPath = new List<Vector2Int>();
        bool isValid = false;
        int distance = 0;

        while (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKeyDown(KeyCode.Escape))
        {
            // First get the grid position that mouse is currently looking at.
            Vector2Int mousePosition = GetCursorGridPosition();

            // Perform pathfinding towards the mouse position.
            currentPath = Grid.Instance.GetPath(playerPosition, mousePosition);

            // A path was found. Make sure that it is within the player's movement range.
            // Each item in path list equals one tile, so using the size is the same as distance
            if (currentPath != null && (currentPath.Count * m_moveCostPerTile) <= m_actionPoints.TotalActionPoints)
            {
                lineRenderer.positionCount = currentPath.Count + 1;
                lineRenderer.SetPosition(0, Grid.Instance.GetWorldPosition(playerPosition.x, playerPosition.y));
                // Go through each point and set the line renderer between them
                for (int i = 0; i < currentPath.Count; i++)
                {
                    Vector3 point = Grid.Instance.GetWorldPosition(currentPath[i].x, currentPath[i].y);
                    lineRenderer.SetPosition(i + 1, point);
                }

                // Since this is valid, we make the line renderer green
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                isValid = true;

                distance = currentPath.Count;
            }

            // No valid path found.
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                isValid = false;
            }

            yield return 0;
        }

        // If the player clicked and not pressed escape, and the path is valid, move the player.
        if (isValid && !Input.GetKeyDown(KeyCode.Escape))
        {
            ICombatAction moveAction = new MoveAction(this, currentPath, m_moveSpeed);
            moveAction.Execute();

            // Subtract the cost of the move from the action points.
            m_actionPoints.SpendActionPoints(distance * m_moveCostPerTile);
        }

        // Clean up linerenderer after
        Destroy(lineRenderer.gameObject);
    }

    /// <summary>
    /// Creates a new linerenderer for the move selection.
    /// </summary>
    /// <returns></returns>
    private LineRenderer CreateLineRenderer()
    {
        LineRenderer lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;

        return lineRenderer;
    }

    public void StartMeleeSelection()
    {
        if (m_actionPoints.TotalActionPoints < m_meleeCost)
        {
            Debug.Log("Not enough action points to perform melee attack.");
            return;
        }

        StartCoroutine(MeleeSelection());
    }

    private IEnumerator MeleeSelection()
    {
        // By default, have no target. We will use null to check
        // if valid target is chosen when the player clicks.
        IDamagable target = null;
        Vector2Int targetPosition = Vector2Int.zero;

        Debug.Log("Starting melee");

        while(!Input.GetKeyDown(KeyCode.Escape))
        {
            // Raycast from cursor to check if whatever cursor is over has IDamagable interface
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                // Check if it has IDamagable interface
                IDamagable potentialTarget = hit.collider.GetComponent<IDamagable>();
                if (potentialTarget != null)
                {
                    // We need to make sure that target is within 1 tile.
                    Vector2Int playerPos = Grid.Instance.GetGridPosition(transform.position);
                    Vector2Int targetPos = Grid.Instance.GetGridPosition(hit.point);
                    int distance = Grid.Instance.GetManhattenDistance(playerPos, targetPos);

                    if (distance <= m_meleeRange)
                    {
                        // A valid target.
                        target = potentialTarget;
                        targetPosition = targetPos;
                    }
                }
            }

            // TODO: add red/green indicator on target based on validity
            if (Input.GetKeyDown(KeyCode.Mouse0))
                break;

            yield return null;
        }

        // If we have a valid target, we will attack it.
        if (target != null)
        {
            ICombatAction melee = new SingleDamageAction(targetPosition, m_meleeDamage);
            melee.Execute();

            // Subtract the cost of the move from the action points.
            m_actionPoints.SpendActionPoints(m_meleeCost);
        }
    }

    /// <summary>
    /// Pushes adjecent enemies away.
    /// </summary>
    public void StartMindblast()
    {
        // Make sure we have enough action points to perform this action.
        if (m_actionPoints.TotalActionPoints < m_mindBlastCost)
        {
            Debug.Log("Not enough action points to perform mindblast.");
            return;
        }

        // Check for units above, below, right and left of player
        Vector2Int playerPos = Grid.Instance.GetGridPosition(transform.position);
        Vector2Int[] adjacentPositions = new Vector2Int[4];
        adjacentPositions[0] = Grid.Instance.PositionWithDirection(playerPos, Direction.Right);
        adjacentPositions[1] = Grid.Instance.PositionWithDirection(playerPos, Direction.Down);
        adjacentPositions[2] = Grid.Instance.PositionWithDirection(playerPos, Direction.Left);
        adjacentPositions[3] = Grid.Instance.PositionWithDirection(playerPos, Direction.Up);

        // Check if there is a unit in any of those positions
        for (int i = 0; i < adjacentPositions.Length; i++)
        {
            // Check if there is a unit in those positions
            Unit unit = Grid.Instance.GetUnitAt(adjacentPositions[i]);
            if (unit != null)
            {
                // We find direction and push. Checking how far they should get pushed and if they can
                // get pushed is handled by the unit itself.
                Vector2Int unitPosition = Grid.Instance.GetGridPosition(unit.transform.position);
                Direction pushDirection = Grid.Instance.GetDirectionTo(unitPosition, playerPos);
                unit.Push(pushDirection, m_mindBlastPushForce);
            }
        }

        // Subtract the cost of the move from the action points.
        m_actionPoints.SpendActionPoints(m_mindBlastCost);
    }

    #endregion

    /// <summary>
    /// Gets the current grid position that cursor is at.
    /// </summary>
    /// <returns></returns>
    private Vector2Int GetCursorGridPosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            return Grid.Instance.GetGridPosition(hit.point);
        }

        return Vector2Int.zero;
    }

    /// <summary>
    /// For the player, health will be initialized through variables set in the editor.
    /// </summary>
    public override void InitializeHealth()
    {
        m_health = new Health(m_maxHealth, m_startingHealth);
    }

    private void InitializeActionPoints()
    {
        m_actionPoints = new ActionPoints(m_maxActionPoints, m_startingActionPoints, m_actionPointPoolSize);
    }

    public override void OnDeath()
    {
        Debug.Log("Player has died!");
        Destroy(gameObject);
    }
}
