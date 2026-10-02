using UnityEngine;

public class OrderMarker : MonoBehaviour
{
    const float FoodSize = 6f;

    Transform pin, food, diamond, diamondShape, carrier;
    Vector3 pinRest;
    float seed;
    bool highlighted;

    public static OrderMarker Create(Order order, Transform restaurant, Transform home, GameObject foodModel)
    {
        var marker = new GameObject($"Order #{order.Id}").AddComponent<OrderMarker>();
        var material = new Material(Resources.Load<Material>("Materials/Marker")) { color = order.Color };

        marker.pin = new GameObject("Pickup").transform;
        marker.pin.SetParent(marker.transform, false);
        Shape(PrimitiveType.Cylinder, marker.pin, Vector3.zero, new Vector3(7.5f, 0.15f, 7.5f), material);
        if (foodModel != null) marker.food = AddFood(foodModel, marker.pin);
        else Shape(PrimitiveType.Sphere, marker.pin, Vector3.up * 3f, Vector3.one * FoodSize, material);
        WorldText.Create($"#{order.Id}", marker.pin, new Vector3(0f, FoodSize + 2f, 0f), Color.white, 0.5f);
        marker.pinRest = restaurant.position + Vector3.up * 20f + Vector3.right * SlotOffset(order.PickupSlot);

        marker.diamond = new GameObject("Dropoff").transform;
        marker.diamond.SetParent(marker.transform, false);
        marker.diamond.position = home.position + Vector3.up * 17f + Vector3.right * SlotOffset(order.DropoffSlot);
        marker.diamondShape = Shape(PrimitiveType.Cube, marker.diamond, Vector3.zero, Vector3.one * 4.2f, material).transform;
        marker.diamondShape.localRotation = Quaternion.Euler(45f, 0f, 45f);
        WorldText.Create($"#{order.Id}", marker.diamond, new Vector3(0f, 6.5f, 0f), Color.white, 0.5f);

        marker.seed = Random.value * 10f;
        return marker;
    }

    public void Carry(Transform courier)
    {
        carrier = courier;
        pin.localScale = Vector3.one * 0.5f;
    }

    public void SetHighlighted(bool on) => highlighted = on;

    public void Remove() => Destroy(gameObject);

    void LateUpdate()
    {
        float bob = Mathf.Sin((Time.time + seed) * 3f) * 0.6f;
        float scale = highlighted ? 1.4f + Mathf.Sin(Time.time * 8f) * 0.1f : 1f;
        if (carrier != null)
        {
            pin.position = carrier.position + Vector3.up * (10f + bob * 0.4f);
        }
        else
        {
            pin.position = pinRest + Vector3.up * bob;
            pin.localScale = Vector3.one * scale;
        }
        diamond.localScale = Vector3.one * scale;
        diamondShape.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        if (food != null) food.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
    }

    static Transform AddFood(GameObject model, Transform parent)
    {
        var holder = new GameObject("Food").transform;
        holder.SetParent(parent, false);
        var food = Instantiate(model, holder);
        food.transform.localPosition = Vector3.zero;

        var renderers = food.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Vector3 local = holder.InverseTransformPoint(bounds.center);
        float bottom = holder.InverseTransformPoint(bounds.min).y;
        food.transform.localPosition = new Vector3(-local.x, -bottom, -local.z);
        holder.localScale = Vector3.one * (FoodSize / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));
        holder.localPosition = Vector3.up * 0.1f;
        return holder;
    }

    static float SlotOffset(int slot) => (slot + 1) / 2 * 8f * (slot % 2 == 1 ? 1f : -1f);

    static GameObject Shape(PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(type);
        Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }
}
