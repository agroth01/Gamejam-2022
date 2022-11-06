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
    private List<ICombatAction> m_actionQueue;

    /// <summary>
    /// Amount of actions currently in the queue.
    /// </summary>
    public int Count { get { return m_actionQueue.Count; } }

    /// <summary>
    /// Initializes the turn queue.
    /// </summary>
    public void Initialize()
    {
        m_actionQueue = new List<ICombatAction>();
    }

    /// <summary>
    /// Adds an action to the queue.
    /// </summary>
    /// <param name="action">The action to add</param>
    public void AddAction(ICombatAction action)
    {
        m_actionQueue.Add(action);
    }

    /// <summary>
    /// Removes an action from the queue.
    /// </summary>
    /// <param name="action">Action to remove</param>
    public void RemoveAction(ICombatAction action)
    {
        m_actionQueue.Remove(action);
    }

    /// <summary>
    /// Gets the next action in the list. Will remove it from queue
    /// </summary>
    /// <returns>The next IcombatAction in queue.</returns>
    public ICombatAction GetNextAction()
    {
        // Get the first action in the queue
        ICombatAction nextAction = m_actionQueue[0];

        // Remove the first action from the queue
        m_actionQueue.RemoveAt(0);

        // Return the first action
        return nextAction;
    }
}
