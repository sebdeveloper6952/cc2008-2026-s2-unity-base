using System.Collections;
using UnityEngine;

public abstract class Courier : MonoBehaviour
{
    public abstract string Description { get; }
    public abstract float Speed { get; }
    public abstract float Fee(float km);
    protected abstract string ModelPath { get; }

    public string CourierName { get; private set; }
    public string Status { get; private set; } = "Disponible";
    public int Deliveries { get; private set; }
    public bool IsAvailable => currentOrder == null;
    public virtual bool IsOnDuty(bool isNight) => !isNight;

    Order currentOrder;
    protected Transform parkingSpot;
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

    public virtual bool Upgrade() => false;

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
