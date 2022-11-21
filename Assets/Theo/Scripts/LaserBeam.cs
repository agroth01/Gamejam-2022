using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    private LineRenderer m_lineRenderer;

    private Transform m_playerTransform;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_playerTransform = GameObject.Find("Player").transform; // needs a better way to get the player
    }

    private void Update()
    {

        m_lineRenderer.transform.LookAt(m_playerTransform.position); // needs a yOffset to be added

        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.collider)
            {
                m_lineRenderer.SetPosition(1, new Vector3(0, 0, hit.distance));
            }
            else
            {
                m_lineRenderer.SetPosition(1, new Vector3(0, 0, 5000)); // probably not hardcode this and probably not to that extreme value
            }
        }
    }
}
