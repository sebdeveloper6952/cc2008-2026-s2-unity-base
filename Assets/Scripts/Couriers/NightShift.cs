public class NightShift : IShift
{
    public string Name => "Noche";
    public bool IsOnDuty(bool isNight)
    {
        if (isNight) return true;
        return false;
    }

    public float Surcharge => 5f;
}