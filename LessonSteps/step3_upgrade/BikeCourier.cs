using UnityEngine;

public class BikeCourier : Courier
{
    public override string Description => "Bici";
    public override float Speed => 10f;
    public override float Fee(float km) => 8f + 2f * km;
    protected override string ModelPath => "Vehicles/Bike";

    public override bool Upgrade()
    {
        GameObject body = gameObject;
        string courierName = CourierName;
        Transform spot = parkingSpot;
        Destroy(transform.GetChild(0).gameObject);
        Destroy(this);
        body.AddComponent<CarCourier>().Setup(courierName, spot);
        return true;
    }
}
