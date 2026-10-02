public class Bike : IVehicle
{
    public string Name => "Bici";
    public float Speed => 5f;
    public float Fee(float km)
    {
        return 5f + 2f * km;
    }

    public string ModelPath => "Vehicles/Bike"; 
}