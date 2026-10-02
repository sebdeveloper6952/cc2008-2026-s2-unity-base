using System.Collections.Generic;
using UnityEngine;

public class OrderBoard : MonoBehaviour
{
    public static OrderBoard Instance { get; private set; }
    public static event System.Action<string> Message;
    public static event System.Action<Order> OrderPlaced;
    public static event System.Action<Order> OrderDelivered;

    [SerializeField] Transform restaurantsGroup;
    [SerializeField] Transform homesGroup;
    [SerializeField] Vector2 secondsBetweenOrders = new Vector2(8f, 13f);
    [SerializeField] int maxOrders = 6;
    [SerializeField] int startingMoney = 100;

    public List<Order> Orders { get; } = new List<Order>();
    public int Money { get; private set; }
    public int Delivered { get; private set; }
    public int Lost { get; private set; }

    static readonly Color[] Palette =
    {
        new Color(1f, 0.30f, 0.35f), new Color(0.25f, 0.65f, 1f), new Color(1f, 0.80f, 0.15f),
        new Color(0.70f, 0.40f, 1f), new Color(0.15f, 0.90f, 0.75f), new Color(1f, 0.55f, 0.15f),
        new Color(1f, 0.45f, 0.85f), new Color(0.55f, 0.95f, 0.25f),
    };

    readonly List<Transform> restaurants = new List<Transform>();
    readonly List<Transform> homes = new List<Transform>();
    int nextId = 1;
    float nextOrderAt;

    void Awake()
    {
        Instance = this;
        Money = startingMoney;
    }

    void Start()
    {
        foreach (Transform t in restaurantsGroup) restaurants.Add(t);
        foreach (Transform t in homesGroup) homes.Add(t);
        nextOrderAt = Time.time + 2f;
    }

    void Update()
    {
        if (Time.time >= nextOrderAt)
        {
            if (Orders.Count < maxOrders) Spawn();
            nextOrderAt = Time.time + Random.Range(secondsBetweenOrders.x, secondsBetweenOrders.y);
        }

        for (int i = Orders.Count - 1; i >= 0; i--)
        {
            Order order = Orders[i];
            order.Tick(Time.deltaTime);
            bool expired = order.State == OrderState.Waiting ? order.TimeLeft <= 0f : order.TimeLeft <= -order.TimeLimit;
            if (expired)
            {
                order.Lose();
                Lost++;
                Remove(order);
                Notify($"Se perdió el pedido #{order.Id} ({order.From})");
            }
        }
    }

    public void Complete(Order order, float courierFee)
    {
        if (!Orders.Contains(order)) return;
        int pay = order.IsLate ? order.Price / 2 : order.Price;
        int net = pay - Mathf.RoundToInt(courierFee);
        Money += net;
        Delivered++;
        Remove(order);
        OrderDelivered?.Invoke(order);
        FloatingText.Spawn(net >= 0 ? $"+Q{net}" : $"-Q{-net}", order.DropoffPoint + Vector3.up * 10f,
                           net >= 0 ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f));
        if (order.IsLate) Notify($"El pedido #{order.Id} llegó tarde: pagaron la mitad");
    }

    public void Spend(int amount) => Money -= amount;

    public static void Notify(string message) => Message?.Invoke(message);

    void Spawn()
    {
        Transform restaurant = restaurants[Random.Range(0, restaurants.Count)];
        Transform home = homes[0];
        float km = 0f;
        for (int tries = 0; tries < 10; tries++)
        {
            home = homes[Random.Range(0, homes.Count)];
            km = RoadNetwork.Instance.Distance(Door(restaurant), Door(home)) / 100f;
            if (km >= 1.2f) break;
        }

        int price = Mathf.RoundToInt((20f + 12f * km + Random.Range(0f, 15f)) / 5f) * 5;
        float timeLimit = 40f + 20f * km;
        var order = new Order(nextId, restaurant.name, HomeName(home), Door(restaurant), Door(home),
                              km, price, timeLimit, Palette[(nextId - 1) % Palette.Length]);
        nextId++;
        order.PickupSlot = FreeSlot(o => o.From == order.From && o.State != OrderState.PickedUp, o => o.PickupSlot);
        order.DropoffSlot = FreeSlot(o => o.To == order.To, o => o.DropoffSlot);
        order.Marker = OrderMarker.Create(order, restaurant, home);
        Orders.Add(order);
        OrderPlaced?.Invoke(order);
    }

    int FreeSlot(System.Predicate<Order> sameBuilding, System.Func<Order, int> slotOf)
    {
        int slot = 0;
        while (Orders.Exists(o => sameBuilding(o) && slotOf(o) == slot)) slot++;
        return slot;
    }

    void Remove(Order order)
    {
        Orders.Remove(order);
        order.Marker.Remove();
    }

    static Vector3 Door(Transform building)
    {
        Transform door = building.Find("Door");
        return door != null ? door.position : building.position;
    }

    static string HomeName(Transform home) =>
        int.TryParse(home.name.Replace("Home_", ""), out int n) ? $"Casa {n}" : home.name;
}
