public class DayShift : IShift
{
    public string Name => "Día";
    public bool IsOnDuty(bool isNight) => !isNight;
    public float Surcharge => 0f;
}
