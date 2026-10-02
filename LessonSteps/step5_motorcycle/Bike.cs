public class Bike : IVehicle
{
    public string Name => "Bici";
    public float Speed => 10f;
    public float Fee(float km) => 8f + 2f * km;
    public string ModelPath => "Vehicles/Bike";
    public IVehicle Upgrade() => new Car();
}
