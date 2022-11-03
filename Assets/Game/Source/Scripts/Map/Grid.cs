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

    private void Awake()
    {
        m_navmesh = new GridNavMesh(GroundHolder, ObstacleHolders);
        m_pathfinder = new Pathfinder(m_navmesh);
    }

    [Button("Test")]
    private void TestPathfinding()
    {
        float startTime = Time.realtimeSinceStartup;
        m_navmesh.GenerateMesh();
        Debug.Log("Baked grid nav mesh in: " + (Time.realtimeSinceStartup - startTime) + " seconds.");
        startTime = Time.realtimeSinceStartup;

        List<Node> path = m_pathfinder.GetPath(m_navmesh.GetNodeAt(0, 2), m_navmesh.GetNodeAt(2, 8));
        Debug.Log("Found path in: " + (Time.realtimeSinceStartup - startTime) + " seconds.");
        if (path != null)
        {
            Debug.DrawLine(m_navmesh.GetNodeAt(0, 2).WorldPosition, path[0].WorldPosition, Color.red, 5);
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].WorldPosition, path[i + 1].WorldPosition, Color.red, 5);
            }
        }

        else
        {
            Debug.Log("no path found");
        }
    }
}
