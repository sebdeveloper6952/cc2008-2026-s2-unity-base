using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteDriver : MonoBehaviour
{
    public float Speed = 10f;
    public bool IsParked { get; private set; } = true;
    [SerializeField] float turnSpeed = 540f;
    [SerializeField] float stopSeconds = 0.6f;
    [SerializeField] float wheelRadius = 0.45f;

    readonly List<Transform> wheels = new List<Transform>();

    public IEnumerator DriveTo(Vector3 destination)
    {
        IsParked = false;
        yield return Follow(RoadNetwork.Instance.Route(transform.position, destination));
        yield return new WaitForSeconds(stopSeconds);
    }

    public IEnumerator ParkAt(Transform spot)
    {
        IsParked = false;
        var path = RoadNetwork.Instance.Route(transform.position, spot.position);
        path.Add(spot.position);
        yield return Follow(path);
        IsParked = true;
    }

    IEnumerator Follow(List<Vector3> path)
    {
        foreach (var point in path)
        {
            var target = new Vector3(point.x, transform.position.y, point.z);
            while ((target - transform.position).sqrMagnitude > 0.0001f)
            {
                Vector3 toTarget = target - transform.position;
                float step = Speed * Time.deltaTime;
                transform.position = toTarget.magnitude <= step ? target : transform.position + toTarget.normalized * step;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), turnSpeed * Time.deltaTime);
                SpinWheels(step);
                yield return null;
            }
        }
    }

    void SpinWheels(float distance)
    {
        if (wheels.Count == 0 || wheels[0] == null)
        {
            wheels.Clear();
            foreach (var t in GetComponentsInChildren<Transform>())
                if (t.name.StartsWith("Wheel")) wheels.Add(t);
        }
        float degrees = distance / wheelRadius * Mathf.Rad2Deg;
        foreach (var wheel in wheels) wheel.Rotate(degrees, 0f, 0f, Space.Self);
    }
}
