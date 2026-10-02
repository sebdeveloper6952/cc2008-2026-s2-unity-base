using System.Collections.Generic;
using UnityEngine;

public class CourierCentral : MonoBehaviour
{
    [SerializeField] Transform[] parkingSpots;

    public List<Courier> Couriers { get; } = new List<Courier>();

    void Start()
    {
        Hire<CarCourier>("Ana");
        Hire<CarCourier>("Luis");
        Hire<BikeCourier>("Sofía");
        Hire<NightCarCourier>("Marta");
        Hire<NightBikeCourier>("Diego");
    }

    void Hire<T>(string name) where T : Courier
    {
        T courier = new GameObject(name).AddComponent<T>();
        courier.Setup(name, parkingSpots[Couriers.Count]);
        Couriers.Add(courier);
    }

    public bool Assign(Order order, Courier courier)
    {
        if (!courier.IsAvailable || !courier.IsOnDuty(GameClock.Instance.IsNight)) return false;
        order.AssignTo(courier.CourierName);
        courier.Deliver(order);
        return true;
    }
}
