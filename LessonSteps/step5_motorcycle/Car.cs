public class Car : IVehicle
{
    public string Name => "Carro";
    public float Speed => 20f;
    public float Fee(float km) => 15f + 4f * km;
    public string ModelPath => "Vehicles/Car";
    public IVehicle Upgrade() => null;
}
