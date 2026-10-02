using System.Collections;
using UnityEngine;

public class Courier : MonoBehaviour
{
    IVehicle vehicle;
    IShift shift;
    GameObject model;

    public string Description => $"{vehicle.Name} · {shift.Name}";
    public float Fee(float km) => vehicle.Fee(km) + shift.Surcharge;
    public bool IsOnDuty(bool isNight) => shift.IsOnDuty(isNight);

    public string CourierName { get; private set; }
    public string Status { get; private set; } = "Disponible";
    public int Deliveries { get; private set; }
    public bool IsAvailable => currentOrder == null;

    Order currentOrder;
    Transform parkingSpot;
    RouteDriver driver;

    public void Setup(string courierName, IVehicle vehicle, IShift shift, Transform spot)
    {
        CourierName = courierName;
        parkingSpot = spot;
        transform.SetPositionAndRotation(spot.position, spot.rotation);
        driver = gameObject.AddComponent<RouteDriver>();
        this.shift = shift;
        ChangeVehicle(vehicle);
    }

    public void ChangeVehicle(IVehicle newVehicle)
    {
        vehicle = newVehicle;
        if (model != null) Destroy(model);
        model = Instantiate(Resources.Load<GameObject>(vehicle.ModelPath), transform);
        driver.Speed = vehicle.Speed;
    }

    public bool Upgrade()
    {
        IVehicle better = vehicle.Upgrade();
        if (better == null) return false;
        ChangeVehicle(better);
        return true;
    }

    public void Deliver(Order order)
    {
        currentOrder = order;
        StopAllCoroutines();
        StartCoroutine(DeliveryTrip());
    }

    IEnumerator DeliveryTrip()
    {
        Status = "Recogiendo";
        yield return driver.DriveTo(currentOrder.PickupPoint);
        currentOrder.PickUp(transform);

        Status = "Entregando";
        yield return driver.DriveTo(currentOrder.DropoffPoint);
        currentOrder.Complete(Fee(currentOrder.Km));
        Deliveries++;
        currentOrder = null;

        Status = "Regresando";
        yield return driver.ParkAt(parkingSpot);
        Status = "Disponible";
    }
}
