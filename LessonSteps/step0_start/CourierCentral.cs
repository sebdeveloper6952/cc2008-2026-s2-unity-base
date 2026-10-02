using System.Collections.Generic;
using UnityEngine;

public class CourierCentral : MonoBehaviour
{
    [SerializeField] Transform[] parkingSpots;

    public List<CarCourier> Couriers { get; } = new List<CarCourier>();

    void Start()
    {
        Hire<CarCourier>("Ana");
        Hire<CarCourier>("Luis");
    }

    void Hire<T>(string name) where T : CarCourier
    {
        T courier = new GameObject(name).AddComponent<T>();
        courier.Setup(name, parkingSpots[Couriers.Count]);
        Couriers.Add(courier);
    }

    public bool Assign(Order order, CarCourier courier)
    {
        if (!courier.IsAvailable || !courier.IsOnDuty(GameClock.Instance.IsNight)) return false;
        order.AssignTo(courier.CourierName);
        courier.Deliver(order);
        return true;
    }
}
