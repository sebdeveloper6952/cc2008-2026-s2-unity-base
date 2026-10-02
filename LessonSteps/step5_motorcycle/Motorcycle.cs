public class Motorcycle : IVehicle
{
    public string Name => "Moto";
    public float Speed => 16f;
    public float Fee(float km) => 10f + 3f * km;
    public string ModelPath => "Vehicles/Motorcycle";
    public IVehicle Upgrade() => new Car();
}
