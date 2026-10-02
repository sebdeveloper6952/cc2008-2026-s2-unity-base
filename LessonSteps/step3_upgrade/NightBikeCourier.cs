public class NightBikeCourier : BikeCourier
{
    public override string Description => "Bici · Noche";
    public override bool IsOnDuty(bool isNight) => isNight;
    public override float Fee(float km) => base.Fee(km) + 5f;
}
