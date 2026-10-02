using UnityEngine;

/// <summary>
/// Drives a vehicle around a closed list of waypoints at a constant speed.
/// </summary>
[AddComponentMenu("City Sim/Waypoint Driver")]
public class WaypointDriver : MonoBehaviour
{
    [Tooltip("Points the vehicle visits in order, looping back to the first.")]
    public Transform[] waypoints;

    [Tooltip("Units per second along the road.")]
    public float speed = 14f;

    [Tooltip("Degrees per second the vehicle can turn.")]
    public float turnSpeed = 150f;

    [Tooltip("How close counts as having reached a waypoint.")]
    public float arriveDistance = 2f;

    int index;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[index];
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            index = (index + 1) % waypoints.Length;
            return;
        }

        Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform a = waypoints[i];
            Transform b = waypoints[(i + 1) % waypoints.Length];
            if (a == null || b == null) continue;
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.DrawWireSphere(a.position, 0.6f);
        }
    }
}
