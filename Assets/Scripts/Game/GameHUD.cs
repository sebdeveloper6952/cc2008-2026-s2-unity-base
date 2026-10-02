using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] CourierCentral central;

    const float CardWidth = 270f, CardHeight = 58f, CardGap = 6f, HeaderHeight = 40f, Padding = 8f;
    const int UpgradePrice = 80;

    static readonly Color PanelColor = new Color(0.07f, 0.08f, 0.11f, 0.86f);
    static readonly Color CardColor = new Color(0.17f, 0.19f, 0.25f, 0.96f);
    static readonly Color CardSelected = new Color(0.30f, 0.36f, 0.50f, 1f);
    static readonly Color CardBusy = new Color(0.12f, 0.13f, 0.17f, 0.92f);
    static readonly Color TextMain = new Color(0.96f, 0.97f, 0.99f);
    static readonly Color TextDim = new Color(0.66f, 0.71f, 0.80f);
    static readonly Color Gold = new Color(1f, 0.84f, 0.35f);

    class OrderCard { public RectTransform Root; public Image Background, Timer; public Text Title, Detail; }
    class CourierCard { public RectTransform Root; public Image Background; public Text Name, Detail, Fee; public GameObject Tag; }

    readonly Dictionary<Order, OrderCard> orderCards = new Dictionary<Order, OrderCard>();
    readonly Dictionary<Courier, CourierCard> courierCards = new Dictionary<Courier, CourierCard>();

    Font font;
    RectTransform ordersPanel, couriersPanel;
    Text moneyText, statsText, clockText, toastText, emptyOrdersText;
    Order selected;
    float toastUntil;

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildLayout();
        OrderBoard.Message += ShowToast;
    }

    void OnDestroy() => OrderBoard.Message -= ShowToast;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Select(null);
        if (selected != null && selected.State != OrderState.Waiting) Select(null);

        RefreshTopBar();
        RefreshOrders();
        RefreshCouriers();
        toastText.enabled = Time.time < toastUntil;
    }

    void OnOrderClicked(Order order)
    {
        if (order.State != OrderState.Waiting) { ShowToast($"El pedido #{order.Id} ya tiene repartidor"); return; }
        Select(order == selected ? null : order);
    }

    void OnCourierClicked(Courier courier)
    {
        if (selected == null) { ShowToast("Primero elige un pedido"); return; }
        if (!courier.IsOnDuty(GameClock.Instance.IsNight)) { ShowToast($"{courier.CourierName} está fuera de turno"); return; }
        if (!courier.IsAvailable) { ShowToast($"{courier.CourierName} no está disponible"); return; }

        int id = selected.Id;
        if (central.Assign(selected, courier))
        {
            ShowToast($"{courier.CourierName} va por el pedido #{id}");
            Select(null);
        }
    }

    void OnUpgradeClicked(Courier courier)
    {
        var upgrade = courier.GetType().GetMethod("Upgrade", System.Type.EmptyTypes);
        if (upgrade == null)
        {
            ShowToast("Todavía no se pueden comprar mejoras");
            return;
        }
        if (OrderBoard.Instance.Money < UpgradePrice)
        {
            ShowToast($"Faltan Q{UpgradePrice - OrderBoard.Instance.Money} para la mejora");
            return;
        }
        string name = courier.CourierName;
        object upgraded = upgrade.Invoke(courier, null);
        if (upgraded is bool done && !done)
        {
            ShowToast($"{name} ya tiene el mejor vehículo");
            return;
        }
        OrderBoard.Instance.Spend(UpgradePrice);
        ShowToast($"¡{name} estrena vehículo! (-Q{UpgradePrice})");
    }

    void Select(Order order)
    {
        if (selected != null && selected.Marker != null) selected.Marker.SetHighlighted(false);
        selected = order;
        if (selected != null) selected.Marker.SetHighlighted(true);
    }

    void ShowToast(string message)
    {
        toastText.text = message;
        toastUntil = Time.time + 2.5f;
    }

    void RefreshTopBar()
    {
        OrderBoard board = OrderBoard.Instance;
        moneyText.text = $"Q {board.Money}";
        moneyText.color = board.Money >= 0 ? new Color(0.45f, 1f, 0.55f) : new Color(1f, 0.45f, 0.45f);
        statsText.text = $"Entregados {board.Delivered}    Perdidos {board.Lost}";
        bool night = GameClock.Instance.IsNight;
        clockText.text = (night ? "Noche  " : "Día  ") + GameClock.Instance.TimeText;
        clockText.color = night ? new Color(0.6f, 0.75f, 1f) : Gold;
    }

    void RefreshOrders()
    {
        List<Order> orders = OrderBoard.Instance.Orders;
        var gone = new List<Order>();
        foreach (Order order in orderCards.Keys) if (!orders.Contains(order)) gone.Add(order);
        foreach (Order order in gone) { Destroy(orderCards[order].Root.gameObject); orderCards.Remove(order); }

        for (int i = 0; i < orders.Count; i++)
        {
            Order order = orders[i];
            if (!orderCards.TryGetValue(order, out OrderCard card)) orderCards[order] = card = CreateOrderCard(order);
            card.Root.anchoredPosition = new Vector2(Padding, -HeaderHeight - i * (CardHeight + CardGap));
            card.Title.text = $"#{order.Id}  {order.From} » {order.To}";
            card.Detail.text = $"Q{order.Price} · {order.Km:0.0} km · {Describe(order)}";
            float t = Mathf.Clamp01(order.TimeLeft / order.TimeLimit);
            card.Timer.rectTransform.anchorMax = new Vector2(t, 1f);
            card.Timer.color = t > 0.5f ? new Color(0.4f, 0.9f, 0.5f) : t > 0.25f ? Gold : new Color(1f, 0.4f, 0.4f);
            card.Background.color = order == selected ? CardSelected : order.State == OrderState.Waiting ? CardColor : CardBusy;
        }
        emptyOrdersText.enabled = orders.Count == 0;
        ordersPanel.sizeDelta = new Vector2(CardWidth + 2 * Padding, HeaderHeight + Mathf.Max(1, orders.Count) * (CardHeight + CardGap) + 2f);
    }

    void RefreshCouriers()
    {
        List<Courier> couriers = central.Couriers;
        var gone = new List<Courier>();
        foreach (Courier courier in courierCards.Keys) if (courier == null || !couriers.Contains(courier)) gone.Add(courier);
        foreach (Courier courier in gone) { Destroy(courierCards[courier].Root.gameObject); courierCards.Remove(courier); }

        bool night = GameClock.Instance.IsNight;
        int row = 0;
        foreach (Courier courier in couriers)
        {
            if (courier == null) continue;
            if (!courierCards.TryGetValue(courier, out CourierCard card))
            {
                courierCards[courier] = card = CreateCourierCard(courier);
                card.Tag = WorldText.Create(courier.CourierName, courier.transform, new Vector3(0f, 7f, 0f), Color.white, 0.35f).gameObject;
            }
            bool onDuty = courier.IsOnDuty(night);
            card.Root.anchoredPosition = new Vector2(-Padding, -HeaderHeight - row++ * (CardHeight + CardGap));
            card.Name.text = $"{courier.CourierName}  <size=12><color=#A9B4C6>{courier.Description}</color></size>";
            card.Detail.text = onDuty ? $"{courier.Status} · {courier.Deliveries} entregas" : "Fuera de turno";
            var driver = courier.GetComponent<RouteDriver>();
            card.Tag.SetActive(driver != null && !driver.IsParked);
            card.Fee.text = selected != null && onDuty ? $"Q{Mathf.RoundToInt(courier.Fee(selected.Km))}" : "";
            card.Background.color = onDuty && courier.IsAvailable ? CardColor : CardBusy;
            card.Name.color = onDuty ? TextMain : TextDim;
        }
        couriersPanel.sizeDelta = new Vector2(CardWidth + 2 * Padding, HeaderHeight + Mathf.Max(1, row) * (CardHeight + CardGap) + 2f);
    }

    static string Describe(Order order)
    {
        string late = order.IsLate ? " · ¡tarde!" : "";
        switch (order.State)
        {
            case OrderState.Assigned: return $"{order.CourierName} va a recogerlo{late}";
            case OrderState.PickedUp: return $"{order.CourierName} lo lleva{late}";
            default: return "Esperando repartidor";
        }
    }

    void BuildLayout()
    {
        var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Transform root = canvasObject.transform;

        RectTransform top = Anchored("Top Bar", root, new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(640f, 54f));
        Fill(top, PanelColor);
        moneyText = Label(top, "", 28, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold, new Vector2(0f, 0f), new Vector2(0.3f, 1f), new Vector2(20f, 0f), Vector2.zero);
        statsText = Label(top, "", 17, TextAnchor.MiddleCenter, TextDim, FontStyle.Normal, new Vector2(0.3f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
        clockText = Label(top, "", 22, TextAnchor.MiddleRight, Gold, FontStyle.Bold, new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-20f, 0f));

        toastText = Label(Anchored("Toast", root, new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(700f, 36f)),
                          "", 20, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        toastText.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);

        ordersPanel = Anchored("Orders", root, new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(CardWidth + 2 * Padding, 100f));
        Fill(ordersPanel, PanelColor);
        Header(ordersPanel, "PEDIDOS");
        emptyOrdersText = Label(ordersPanel, "Sin pedidos por ahora", 15, TextAnchor.UpperLeft, TextDim, FontStyle.Italic,
                                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -HeaderHeight - 30f), new Vector2(-18f, -HeaderHeight));

        couriersPanel = Anchored("Couriers", root, new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(CardWidth + 2 * Padding, 100f));
        Fill(couriersPanel, PanelColor);
        Header(couriersPanel, "REPARTIDORES");

        RectTransform hint = Anchored("Hint", root, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(900f, 34f));
        Fill(hint, PanelColor);
        Label(hint, "1. Elige un pedido    2. Elige un repartidor        Esc: cancelar    N: día / noche",
              15, TextAnchor.MiddleCenter, TextDim, FontStyle.Normal, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    OrderCard CreateOrderCard(Order order)
    {
        var card = new OrderCard();
        card.Root = Anchored($"Order #{order.Id}", ordersPanel, new Vector2(0f, 1f), Vector2.zero, new Vector2(CardWidth, CardHeight));
        card.Background = Fill(card.Root, CardColor);
        card.Root.gameObject.AddComponent<Button>().onClick.AddListener(() => OnOrderClicked(order));

        Fill(Box("Color", card.Root, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(6f, 0f)), order.Color);
        card.Title = Label(card.Root, "", 15, TextAnchor.UpperLeft, TextMain, FontStyle.Bold, new Vector2(0f, 0.5f), Vector2.one, new Vector2(14f, -2f), new Vector2(-8f, -5f));
        card.Detail = Label(card.Root, "", 12, TextAnchor.UpperLeft, TextDim, FontStyle.Normal, Vector2.zero, new Vector2(1f, 0.5f), new Vector2(14f, 9f), new Vector2(-8f, 1f));

        RectTransform track = Box("Timer", card.Root, Vector2.zero, new Vector2(1f, 0f), new Vector2(14f, 5f), new Vector2(-8f, 8f));
        Fill(track, new Color(0f, 0f, 0f, 0.35f));
        card.Timer = Fill(Box("Fill", track, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero), Color.green);
        return card;
    }

    CourierCard CreateCourierCard(Courier courier)
    {
        var card = new CourierCard();
        card.Root = Anchored(courier.CourierName, couriersPanel, new Vector2(1f, 1f), Vector2.zero, new Vector2(CardWidth, CardHeight));
        card.Background = Fill(card.Root, CardColor);
        card.Root.gameObject.AddComponent<Button>().onClick.AddListener(() => OnCourierClicked(courier));

        card.Name = Label(card.Root, "", 16, TextAnchor.UpperLeft, TextMain, FontStyle.Bold, new Vector2(0f, 0.5f), Vector2.one, new Vector2(12f, -2f), new Vector2(-60f, -5f));
        card.Fee = Label(card.Root, "", 16, TextAnchor.UpperRight, Gold, FontStyle.Bold, new Vector2(0f, 0.5f), Vector2.one, new Vector2(12f, -2f), new Vector2(-10f, -5f));
        card.Detail = Label(card.Root, "", 12, TextAnchor.UpperLeft, TextDim, FontStyle.Normal, Vector2.zero, new Vector2(1f, 0.5f), new Vector2(12f, 9f), new Vector2(-104f, 1f));

        RectTransform upgradeButton = Box("Upgrade", card.Root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-98f, 7f), new Vector2(-8f, 26f));
        Fill(upgradeButton, new Color(0.16f, 0.50f, 0.30f, 1f));
        upgradeButton.gameObject.AddComponent<Button>().onClick.AddListener(() => OnUpgradeClicked(courier));
        Label(upgradeButton, $"Mejorar · Q{UpgradePrice}", 12, TextAnchor.MiddleCenter, TextMain, FontStyle.Bold, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return card;
    }

    void Header(RectTransform panel, string title) =>
        Label(panel, title, 15, TextAnchor.MiddleLeft, TextDim, FontStyle.Bold, new Vector2(0f, 1f), Vector2.one,
              new Vector2(18f, -HeaderHeight), new Vector2(-18f, 0f));

    static RectTransform Anchored(string name, Transform parent, Vector2 corner, Vector2 position, Vector2 size)
    {
        RectTransform rect = Box(name, parent, corner, corner, Vector2.zero, Vector2.zero);
        rect.pivot = corner;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static RectTransform Box(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    static Image Fill(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text Label(RectTransform parent, string text, int size, TextAnchor align, Color color, FontStyle style,
               Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var label = Box("Text", parent, anchorMin, anchorMax, offsetMin, offsetMax).gameObject.AddComponent<Text>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = align;
        label.color = color;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        return label;
    }
}
