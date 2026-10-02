public interface IShift
{
    string Name { get; } 
    bool IsOnDuty(bool isNight);
    float Surcharge { get; }
}