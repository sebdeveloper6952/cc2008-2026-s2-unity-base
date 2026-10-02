using System.Collections;
using UnityEngine;

public class CarCourier : MonoBehaviour
{
    public string Description => "Carro";
    public float Speed => 20f;
    public float Fee(float km) => 15f + 4f * km;
    string ModelPath => "Vehicles/Car";

    public string CourierName { get; private set; }
    public string Status { get; private set; } = "Disponible";
    public int Deliveries { get; private set; }
    public bool IsAvailable => currentOrder == null;
    public bool IsOnDuty(bool isNight) => true;

    Order currentOrder;
    Transform parkingSpot;
    RouteDriver driver;

    public void Setup(string courierName, Transform spot)
    {
        CourierName = courierName;
        parkingSpot = spot;
        transform.SetPositionAndRotation(spot.position, spot.rotation);
        Instantiate(Resources.Load<GameObject>(ModelPath), transform);
        driver = gameObject.AddComponent<RouteDriver>();
        driver.Speed = Speed;
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
