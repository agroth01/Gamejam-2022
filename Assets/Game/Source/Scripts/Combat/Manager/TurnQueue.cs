// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: A turn queue is a collection of ICombatActions that can be iterated through.
//              Keep in mind that this is only for the enemies. Player will not use a queue system.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnQueue
{
    // Store all actions in a list
    private List<TurnQueueItem> m_queue;

    /// <summary>
    /// Amount of actions currently in the queue.
    /// </summary>
    public int Count { get { return m_queue.Count; } }

    /// <summary>
    /// Initializes the turn queue.
    /// </summary>
    public void Initialize()
    {
        m_queue = new List<TurnQueueItem>();
    }

    /// <summary>
    /// Adds an action to the queue.
    /// </summary>
    /// <param name="action">The action to add</param>
    public void AddAction(Unit unit, ICombatAction action)
    {
        m_queue.Add(new TurnQueueItem(unit, action));
    }

    /// <summary>
    /// Removes an action from the queue.
    /// </summary>
    /// <param name="action">Action to remove</param>
    public void RemoveAction(ICombatAction action)
    {
        // Loop through all actions in the queue and remove turn queue item with the same action.
        for (int i = 0; i < m_queue.Count; i++)
        {
            if (m_queue[i].Action == action)
            {
                m_queue.RemoveAt(i);
                break;
            }
        }
    }

    public TurnQueueItem GetNext()
    {
        // Get the first action in the queue
        TurnQueueItem item = m_queue[0];

        // Remove the action from the queue
        m_queue.RemoveAt(0);

        // Return the action
        return item;
    }
}
