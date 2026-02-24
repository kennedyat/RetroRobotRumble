using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private bool loop = true;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool orientToPath = true;
    [SerializeField] private float rotationSpeed = 10f;

    private int currentIndex = 0;

    private void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("PathFollower has no waypoints assigned.", this);
            enabled = false;
            return;
        }

        // Start at the first waypoint position
        transform.position = waypoints[0].position;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Transform target = waypoints[currentIndex];

        Vector3 toTarget = target.position - transform.position;
        float step = speed * Time.deltaTime;

        // If close enough to the waypoint, snap to it and go to the next one
        if (toTarget.sqrMagnitude <= step * step)
        {
            transform.position = target.position;

            currentIndex++;

            if (currentIndex >= waypoints.Count)
            {
                if (loop)
                {
                    currentIndex = 0;   // loop back to start
                }
                else
                {
                    enabled = false;    // stop at the last waypoint
                    return;
                }
            }

            target = waypoints[currentIndex];
            toTarget = target.position - transform.position;
        }

        // Move toward current waypoint
        transform.position += toTarget.normalized * step;

        // Rotate along path direction
        if (orientToPath && toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // Optional: draw the path in the Scene view
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.magenta;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform a = waypoints[i];
            Transform b = waypoints[(i + 1) % waypoints.Count];

            if (a == null || b == null) continue;

            Gizmos.DrawSphere(a.position, 0.2f);
            Gizmos.DrawLine(a.position, b.position);

            if (!loop && i == waypoints.Count - 2) break;
        }
    }
}
