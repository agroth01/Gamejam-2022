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
        m_navmesh = new GridNavMesh(GroundHolder, ObstacleHolders);
        m_pathfinder = new Pathfinder(m_navmesh);
    }

    private void Update()
    {
        TestPathfindingClick();
    }

    /// <summary>
    /// Rebuilds the navmesh for pathfinding. This should be called every time there is a change on the map,
    /// be it a unit moving, something getting destroyed or what not.
    /// </summary>
    public void BakeNavMesh()
    {
        m_navmesh.Bake();
    }

    #region Debugging

    [Button("Test")]
    private void TestPathfinding()
    {
        float startTime = Time.realtimeSinceStartup;
        m_navmesh.Bake();
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

    private void TestPathfindingClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Transform player = GameObject.Find("Player").transform;

                Node node = m_navmesh.GetNodeAt(hit.point);
                Node playerNode = m_navmesh.GetNodeAt(player.position);
                List<Node> path = m_pathfinder.GetPath(playerNode, node);
                if (node != null)
                {
                    if (path != null)
                    {
                        Debug.Log("aaa");
                        List<Vector3> points = new List<Vector3>();
                        foreach (Node n in path)
                        {
                            points.Add(n.WorldPosition);
                        }

                        StartCoroutine(TestMove(player, points, 4));
                    }

                    else
                    {
                        Debug.Log("no path");
                    }

                    
                }

                else
                {
                    Debug.Log("no node clicked");
                }
            }
        }

    }

    private IEnumerator TestMove(Transform mover, List<Vector3> points, float speed)
    {
        foreach (Vector3 point in points)
        {
            while (Vector3.Distance(mover.position, point) > 0.1f)
            {
                mover.position = Vector3.MoveTowards(mover.position, point, speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    #endregion
}
