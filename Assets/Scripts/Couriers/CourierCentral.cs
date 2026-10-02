using System.Collections.Generic;
using UnityEngine;

public class CourierCentral : MonoBehaviour
{
    [SerializeField] Transform[] parkingSpots;

    public List<Courier> Couriers { get; } = new List<Courier>();

    void Start()
    {
        Hire<Courier>("Ana", new Car(), new DayShift());
        Hire<Courier>("Javier", new Bike(), new DayShift());
        
        Hire<Courier>("Otto", new Bike(), new NightShift());
        Hire<Courier>("Alex", new Car(), new NightShift());
    }

    void Hire<T>(string name, IVehicle vehicle, IShift shift) where T : Courier
    {
        T courier = new GameObject(name).AddComponent<T>();
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
