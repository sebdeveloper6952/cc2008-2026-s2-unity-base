public interface IVehicle
{
    string Name { get; }
    float Speed { get; }
    float Fee(float km);
    string ModelPath { get; }
    IVehicle Upgrade();
}
