using UnityEngine;
using System.Collections;

public class Courier : MonoBehaviour
{
    IVehicle vehicle;
    IShift shift;
    GameObject model;
    Order currentOrder;
    Transform parkingSpot;
    RouteDriver driver;

    public string Description => $"{vehicle.Name} + {shift.Name}";
    public float Fee(float km) => vehicle.Fee(km) + shift.Surcharge;
    public bool IsOnDuty(bool isNight) => shift.IsOnDuty(isNight);
    
    public string CourierName { get;  private set; }
    public string Status { get; private set; } = "Disponible";
    public int Deliveries { get; private set; }
    public bool IsAvailable => currentOrder == null;

   

    public void Setup(string courierName, IVehicle vehicle, IShift shift, Transform spot)
    {
        this.vehicle = vehicle;
        this.shift = shift;
        CourierName = courierName;
        parkingSpot = spot;
        transform.SetPositionAndRotation(spot.position, spot.rotation);
        model = Instantiate(Resources.Load<GameObject>(vehicle.ModelPath), transform);
        driver = gameObject.AddComponent<RouteDriver>();
        driver.Speed = vehicle.Speed;
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

    public bool Upgrade()
    {
        if (vehicle.Name == "Bici")
        {
            vehicle = new Car();
            Destroy(model);
            model = Instantiate(Resources.Load<GameObject>(vehicle.ModelPath), transform);
            driver.Speed = vehicle.Speed;
            return true;
        }

        return false;
    }
}