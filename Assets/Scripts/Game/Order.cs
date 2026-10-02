using UnityEngine;

public enum OrderState { Waiting, Assigned, PickedUp, Delivered, Lost }

public class Order
{
    public int Id { get; }
    public string From { get; }
    public string To { get; }
    public Vector3 PickupPoint { get; }
    public Vector3 DropoffPoint { get; }
    public float Km { get; }
    public int Price { get; }
    public float TimeLimit { get; }
    public float TimeLeft { get; private set; }
    public Color Color { get; }
    public OrderState State { get; private set; } = OrderState.Waiting;
    public string CourierName { get; private set; }
    public bool IsLate => TimeLeft <= 0f;

    internal OrderMarker Marker { get; set; }
    internal int PickupSlot { get; set; }
    internal int DropoffSlot { get; set; }

    public Order(int id, string from, string to, Vector3 pickupPoint, Vector3 dropoffPoint,
                 float km, int price, float timeLimit, Color color)
    {
        Id = id;
        From = from;
        To = to;
        PickupPoint = pickupPoint;
        DropoffPoint = dropoffPoint;
        Km = km;
        Price = price;
        TimeLimit = TimeLeft = timeLimit;
        Color = color;
    }

    public void AssignTo(string courierName)
    {
        CourierName = courierName;
        State = OrderState.Assigned;
    }

    public void PickUp(Transform carrier)
    {
        State = OrderState.PickedUp;
        Marker.Carry(carrier);
    }

    public void Complete(float courierFee)
    {
        State = OrderState.Delivered;
        OrderBoard.Instance.Complete(this, courierFee);
    }

    internal void Tick(float seconds) => TimeLeft -= seconds;
    internal void Lose() => State = OrderState.Lost;
}
