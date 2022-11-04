// /* --------------------
// -----------------------
// Creation date: 03/11/2022
// Author: Alex
// Description: Represents a node on the grid.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    // Positions of the node on grid coordinates and in world space.
    public Vector2Int GridPosition;
    public Vector3 WorldPosition;

    // Flag for determining if this node is valid to move to.
    public bool IsObstructed;

    // Properties for calculating pathfinding
    public int HCost;
    public int GCost;
    public int FCost { get { return HCost + GCost; } }
    public Node Parent;

    public Node(Vector2Int gridPosition, Vector3 worldPosition, bool isObstructed)
    {
        GridPosition = gridPosition;
        WorldPosition = worldPosition;
        IsObstructed = isObstructed;
    }
}
