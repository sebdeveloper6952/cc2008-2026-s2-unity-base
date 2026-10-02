using UnityEngine;

public class WorldText : MonoBehaviour
{
    static Font font;

    public static TextMesh Create(string text, Transform parent, Vector3 localPosition, Color color, float size)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var label = NewText(text, color, size);
        label.transform.SetParent(parent, false);
        label.transform.localPosition = localPosition;
        label.gameObject.AddComponent<WorldText>();

        var shadow = NewText(text, new Color(0f, 0f, 0f, 0.65f), size);
        shadow.transform.SetParent(label.transform, false);
        shadow.transform.localPosition = new Vector3(size, -size, 0.05f);
        return label;
    }

    static TextMesh NewText(string text, Color color, float size)
    {
        var mesh = new GameObject("Label").AddComponent<TextMesh>();
        mesh.text = text;
        mesh.font = font;
        mesh.fontSize = 64;
        mesh.fontStyle = FontStyle.Bold;
        mesh.characterSize = size;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
        mesh.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        return mesh;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}

public class FloatingText : MonoBehaviour
{
    const float Lifetime = 1.8f;
    TextMesh[] texts;
    Color[] colors;
    float age;

    public static void Spawn(string message, Vector3 position, Color color)
    {
        var label = WorldText.Create(message, null, position, color, 0.6f);
        label.gameObject.AddComponent<FloatingText>();
    }

    void Start()
    {
        texts = GetComponentsInChildren<TextMesh>();
        colors = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++) colors[i] = texts[i].color;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * (5f * Time.deltaTime);
        float alpha = 1f - Mathf.Clamp01((age - Lifetime * 0.5f) / (Lifetime * 0.5f));
        for (int i = 0; i < texts.Length; i++)
            texts[i].color = new Color(colors[i].r, colors[i].g, colors[i].b, colors[i].a * alpha);
        if (age >= Lifetime) Destroy(gameObject);
    }
}
