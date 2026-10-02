public class CarCourier : Courier
{
    public override string Description => "Carro";
    public override float Speed => 20f;
    public override float Fee(float km) => 15f + 4f * km;
    protected override string ModelPath => "Vehicles/Car";
}
