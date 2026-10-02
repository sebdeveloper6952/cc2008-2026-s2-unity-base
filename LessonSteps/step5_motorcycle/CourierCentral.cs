using System.Collections.Generic;
using UnityEngine;

public class CourierCentral : MonoBehaviour
{
    [SerializeField] Transform[] parkingSpots;

    public List<Courier> Couriers { get; } = new List<Courier>();

    void Start()
    {
        Hire("Ana", new Car(), new DayShift());
        Hire("Luis", new Car(), new DayShift());
        Hire("Sofía", new Bike(), new DayShift());
        Hire("Marta", new Car(), new NightShift());
        Hire("Diego", new Bike(), new NightShift());
        Hire("Pablo", new Motorcycle(), new NightShift());
    }

    void Hire(string name, IVehicle vehicle, IShift shift)
    {
        Courier courier = new GameObject(name).AddComponent<Courier>();
        courier.Setup(name, vehicle, shift, parkingSpots[Couriers.Count]);
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
