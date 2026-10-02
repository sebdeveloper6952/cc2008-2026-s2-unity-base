public class DayShift : IShift
{
    public string Name => "Día";
    public bool IsOnDuty(bool isNight)
    {
        if (isNight) return false;
        return true;
    }

    public float Surcharge => 0f;
}