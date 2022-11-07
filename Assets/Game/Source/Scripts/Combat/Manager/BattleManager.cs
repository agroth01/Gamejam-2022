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
        this.Invoke(StartPlayerTurn, 0.5f);
    }

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
        m_turnQueueBuffer.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        foreach (QueueBufferItem item in m_turnQueueBuffer)
        {
            m_turnQueue.AddAction(item.Action);
        }
        m_turnQueueBuffer.Clear();

        OnEnemyTurn?.Invoke();
        StartCoroutine(PerformEnemyActions());
    }

    /// <summary>
    /// Marks the current state of battle as player
    /// </summary>
    public void StartPlayerTurn()
    {
        // Enemies should choose their actions as soon as player turn starts,
        // so that player can plan around it.
        DetermineEnemyMoves();


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

        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemy.OnTurnStart();
        }

        // Go through all the actions in the queue and perform them.
        while (m_turnQueue.Count > 0)
        {
            ICombatAction action = m_turnQueue.GetNextAction();
            action.Execute();
            yield return 0;
        }

        foreach (Enemy enemy in Grid.Instance.GetUnitsOfType<Enemy>())
        {
            enemy.OnTurnEnd();
        }

        // When done, it goes back to being the player's turn.
        StartPlayerTurn();
    }

    #endregion
}
