public class NightShift : IShift
{
    public string Name => "Noche";
    public bool IsOnDuty(bool isNight) => isNight;
    public float Surcharge => 5f;
}
