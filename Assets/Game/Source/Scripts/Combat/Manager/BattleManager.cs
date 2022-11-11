// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: The battle manager will be responsible for driving the different states of the battle.
// -----------------------
// ------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // The current state of the battle.
    private BattleState m_state;

    // Instance of a turn queue in order to track what order of actions.
    private TurnQueue m_turnQueue;

    // Because we want to be able to add actions with different priority based on speed,
    // we store all actions in a buffer that will be added to turn queue when starting enemy turn.
    private List<QueueBufferItem> m_turnQueueBuffer;

    // Track the player for easy access to methods and variables.
    private Player m_player;
    private List<Enemy> m_enemies;

    // Events
    public Action OnPlayerTurn;
    public Action OnEnemyTurn;

    private void Awake()
    {
        // Set singleton instance and throw error if it already exists.
        if (Instance == null) Instance = this;
        else Debug.LogError("There is more than one BattleManager in the scene!");

        // Setup what is needed
        m_turnQueue = new TurnQueue();
        m_turnQueue.Initialize();
        m_state = BattleState.Player;
        m_turnQueueBuffer = new List<QueueBufferItem>();

        // Find the player. It is invoked with some delay, as it is not guaranteed to be
        // registered in grid in Awake().
        this.Invoke(() => m_player = (Player)Grid.Instance.GetUnitsOfType<Player>()[0], 0.25f);
    }

    private void Start()
    {
        // For debugging, start player turn from here.
        this.Invoke(StartPlayerTurn, 0.25f);
    }

    #region Combat

    /// <summary>
    /// Spawns a unit on the grid. Will throw an error message if the prefab does
    /// not contain a unit script.
    /// </summary>
    /// <param name="unitPrefab">Prefab for the unit to spawn.</param>
    /// <param name="spawnPosition">Position on grid to spawn.</param>
    public void SpawnUnit(GameObject unitPrefab, Vector2Int spawnPosition)
    {
        // Check that prefab has unit component and throw error otherwise
        if (unitPrefab.GetComponent<Unit>() == null)
        {
            Debug.LogError("Tried to spawn object " + unitPrefab.name + " but it does not contain a Unit component!");
            return;
        }
        
        // To start with, we create the object as a child of entities gameobject, so that
        // the newly created unit will be included when baking navmesh. We do not need to
        // do anything else, as unit registration and automatic baking happens in unit script.
        Vector3 worldPos = Grid.Instance.GetWorldPosition(spawnPosition.x, spawnPosition.y);
        Unit unit = Instantiate(unitPrefab, worldPos, Quaternion.identity).GetComponent<Unit>();
        unit.transform.parent = GameObject.Find("Entities").transform;
    }

    #endregion

    #region Queue

    /// <summary>
    /// Adds an action to the queue. Will be used during enemy turns.
    /// </summary>
    /// <param name="action">The action to add</param>
    public void AddActionToQueue(ICombatAction action, int priority)
    {
        // Create new turn buffer item and add it.
        QueueBufferItem item = new QueueBufferItem(priority, action);
        m_turnQueueBuffer.Add(item);
    }

    /// <summary>
    /// Removes an action from the queue.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    public void RemoveActionFromQueue(ICombatAction action)
    {
        // Find the item with the action and remove it.
        QueueBufferItem item = m_turnQueueBuffer.Find(x => x.Action == action);
        m_turnQueueBuffer.Remove(item);
    }

    #endregion

    #region Turns

    /// <summary>
    /// Starts performing all the actions for the enemies.
    /// </summary>
    public void StartEnemyTurn()
    {
        // Make sure we are not already in the enemy turn
        if (m_state == BattleState.Enemy) return;
        m_state = BattleState.Enemy;

        // Apply effects for end of player turn
        m_player.OnTurnEnd();

        // All actions in the buffer will be added into the queue based
        // on their priority.
        BufferToQueue();       

        OnEnemyTurn?.Invoke();
        StartCoroutine(PerformEnemyActions());
    }

    /// <summary>
    /// Marks the current state of battle as player
    /// </summary>
    public void StartPlayerTurn()
    {
        m_player.OnTurnStart();
        m_player.RestoreAP();
        m_state = BattleState.Player;
        OnPlayerTurn?.Invoke();
    }

    private void DetermineEnemyMoves()
    {
        // Since all units are already registered in the grid, we can loop through
        // the registry and get all enemies.
        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemy.DetermineAction();
        }
    }

    /// <summary>
    /// Coroutine that will perform all the actions for the enemies.
    /// </summary>
    private IEnumerator PerformEnemyActions()
    {
        
        // Go through the logic for start of turn for each enemy unit.
        // This is for tracking status effects.
        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemy.OnTurnStart();
        }

        // Now it is time to perform all actions that is in the queue.
        // This will be any actions that is not moving, as that will happen
        // later in this coroutine.
        while (m_turnQueue.Count > 0)
        {
            ICombatAction action = m_turnQueue.GetNextAction();
            yield return action.Execute();
        }

        // Since we now have determined if the enemy should move, we can now
        // loop through turn queue again, as units might want to move.
        yield return HandleEnemyMovement();

        // End of turn effects gets processed here
        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemy.OnTurnEnd();
        }

        // Enemies should then choose their new moves for next round after having moved.
        DetermineEnemyMoves();

        // When done, it goes back to being the player's turn.
        StartPlayerTurn();
    }

    /// <summary>
    /// Moves all actions in the buffer into the queue based on their priority
    /// </summary>
    private void BufferToQueue()
    {
        m_turnQueueBuffer.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        foreach (QueueBufferItem item in m_turnQueueBuffer)
        {
            m_turnQueue.AddAction(item.Action);
        }
        m_turnQueueBuffer.Clear();
    }

    /// <summary>
    /// Gets all desired actions for movement from enemies and perform the actions.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleEnemyMovement()
    {
        // Start with running individual logic for each enemy where they want to move
        List<Enemy> enemies = new List<Enemy>();
        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemies.Add(enemy);
        }

        
        // This is a very hacky solution to fix the problem of enemies deciding to move into
        // the same position. Essentially, instead of every enemy deciding where to move and then
        // move one by one, each enemy decides where to move and then moves instantly.
        enemies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        foreach (Enemy enemy in enemies)
        {
            enemy.DetermineMove();
            BufferToQueue();
            while (m_turnQueue.Count > 0)
            {
                ICombatAction action = m_turnQueue.GetNextAction();
                yield return action.Execute();
            }
        }

        //// Then, move the actions from buffer into queue
        //BufferToQueue();

        //// Finally, we loop through the action queue and perform those actions
        //while (m_turnQueue.Count > 0)
        //{
        //    ICombatAction action = m_turnQueue.GetNextAction();
        //    yield return action.Execute();
        //}
    }

    #endregion
}
