// /* --------------------
// -----------------------
// Creation date: 03/11/2022
// Author: Alex
// Description: This is the actual class for containing information about the grid, such as tiles and units.
//              Also responsible for navigating units using pathfinding.
// -----------------------
// ------------------- */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("References")]
    public Transform GroundHolder;
    public List<Transform> ObstacleHolders;

    // Navmesh for pathfinder and accessing nodes.
    private GridNavMesh m_navmesh;
    
    // Pathfinder object used for navigation on the grid.
    private Pathfinder m_pathfinder;

    // To track where units are on the grid, we store them along with their positions in a dictionary.
    private Dictionary<Unit, Vector2Int> m_unitRegistry;

    // Singleton to access grid from any class. This is important, since anything modifying
    // the map will have to rebake the navmesh.
    public static Grid Instance { get; private set; }

    private void Awake()
    {
        // Throw error if there is more than one singleton instance in the scene
        if (Instance == null) Instance = this;
        else
        {
            Debug.LogError("There is more than one Grid in the scene!");
        }

        // Initialize navigation
        m_navmesh = new GridNavMesh(GroundHolder, ObstacleHolders, true);
        m_pathfinder = new Pathfinder(m_navmesh);

        // Registy
        m_unitRegistry = new Dictionary<Unit, Vector2Int>();
    }

    #region Navigation

    /// <summary>
    /// Rebuilds the navmesh for pathfinding. This should be called every time there is a change on the map,
    /// be it a unit moving, something getting destroyed or what not.
    /// </summary>
    public void BakeNavMesh()
    {
        m_navmesh.Bake();
    }

    /// <summary>
    /// Converts coordinates on the grid into a world position for
    /// units to update position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Vector3 GetWorldPosition(int x, int y)
    {
        Node nodeAtPosition = m_navmesh.GetNodeAt(x, y);
        return nodeAtPosition.WorldPosition;
    }

    /// <summary>
    /// Gives the position of something with the specified direction applied to it.
    /// Easy way to get adjecent tiles for example.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector2Int PositionWithDirection(Vector2Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector2Int(position.x, position.y + 1);
            case Direction.Down:
                return new Vector2Int(position.x, position.y - 1);
            case Direction.Left:
                return new Vector2Int(position.x - 1, position.y);
            case Direction.Right:
                return new Vector2Int(position.x + 1, position.y);
            default:
                return position;
        }
    }

    /// <summary>
    /// Gets a direction to a position from another position.
    /// </summary>
    /// <param name="from">Position to get direction from</param>
    /// <param name="to">Position to get direction to</param>
    /// <returns></returns>
    public Direction GetDirectionTo(Vector2Int to, Vector2Int from)
    {
        // We can get away with using multiple if statements only because
        // of the nature of the grid, they will only ever be in one of these.
        if (from.x < to.x) return Direction.Right;
        if (from.x > to.x) return Direction.Left;
        if (from.y < to.y) return Direction.Up;
        if (from.y > to.y) return Direction.Down;
        return Direction.None;
    }

    /// <summary>
    /// Converts coordinates in the world into a grid position.
    /// </summary>
    /// <param name="worldPosition">Position in world space</param>
    /// <returns></returns>
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Node nodeAtPosition = m_navmesh.GetNodeAt(worldPosition);
        if (nodeAtPosition == null)
            return Vector2Int.zero;
        
        return nodeAtPosition.GridPosition;
    }

    /// <summary>
    /// Returns a list of grid positions representing the path towards end
    /// </summary>
    /// <param name="start">Where to navigate from</param>
    /// <param name="end">Target position</param>
    /// <returns></returns>
    public List<Vector2Int> GetPath(Vector2Int start, Vector2Int end)
    {
        // Convert positions to nodes
        Node startNode = m_navmesh.GetNodeAt(start.x, start.y);
        Node endNode = m_navmesh.GetNodeAt(end.x, end.y);

        // Get the path
        List<Node> pathNodes = m_pathfinder.GetPath(startNode, endNode);
        if (pathNodes == null)
        {
            // In case there is no path found.
            return null;
        }
        
        List<Vector2Int> path = Node.ConvertToPositions(pathNodes);

        return path;
    }

    /// <summary>
    /// Checks if there are anything on the tile like a wall or unit. Returns true if not.
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns></returns>
    public bool IsTileFree(Vector2Int position)
    {
        // We need to check the node to see if it is obstructed.
        Node node = m_navmesh.GetNodeAt(position.x, position.y);
        return !node.IsObstructed;
    }

    /// <summary>
    /// Gets the manhatten distance between two points on the grid
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public int GetManhattenDistance(Vector2Int start, Vector2Int end)
    {
        return m_pathfinder.GetManhattenDistance(GetNodeAt(start.x, start.y), GetNodeAt(end.x, end.y));
    }

    /// <summary>
    /// Gets the node at the given point on grid.
    /// </summary>
    /// <param name="x">X value on grid.</param>
    /// <param name="y">Y value on grid.</param>
    /// <returns></returns>
    public Node GetNodeAt(int x, int y)
    {
        return m_navmesh.GetNodeAt(x, y);
    }

    #endregion

    #region Units

    /// <summary>
    /// Adds a unit to the registry. If they are not added, we will not be able to track them.
    /// </summary>
    /// <param name="unit">The unit to add.</param>
    public void RegisterUnit(Unit unit)
    {
        Vector2Int position = GetGridPosition(unit.transform.position);
        m_unitRegistry.Add(unit, position);
    }

    /// <summary>
    /// Removes the unit from the registry.
    /// </summary>
    /// <param name="unit"></param>
    public void UnregisterUnit(Unit unit)
    {
        m_unitRegistry.Remove(unit);
    }

    /// <summary>
    /// Updates the position of the unit in the registry.
    /// Has to be called when the unit moves.
    /// </summary>
    /// <param name="unit"></param>
    public void UpdateUnit(Unit unit)
    {
        Vector2Int position = GetGridPosition(unit.transform.position);
        m_unitRegistry[unit] = position;
    }

    /// <summary>
    /// Looks for a unit on the given position on the grid.
    /// Will return null if no unit is found.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns></returns>
    public Unit GetUnitAt(Vector2Int position)
    {
        // Look for unit in registry and return it
        foreach (KeyValuePair<Unit, Vector2Int> unit in m_unitRegistry)
        {
            if (unit.Value == position)
                return unit.Key;
        }

        return null;
    }

    #endregion

}
