public class BikeCourier : Courier
{
    public override string Description => "Bici";
    public override float Speed => 10f;
    public override float Fee(float km) => 8f + 2f * km;
    protected override string ModelPath => "Vehicles/Bike";
}
