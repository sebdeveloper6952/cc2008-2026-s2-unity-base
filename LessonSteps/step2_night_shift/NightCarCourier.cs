public class NightCarCourier : CarCourier
{
    public override string Description => "Carro · Noche";
    public override bool IsOnDuty(bool isNight) => isNight;
    public override float Fee(float km) => base.Fee(km) + 5f;
}
