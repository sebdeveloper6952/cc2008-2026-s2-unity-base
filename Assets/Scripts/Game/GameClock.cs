using UnityEngine;
using UnityEngine.InputSystem;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }

    [SerializeField] Light sun;
    [SerializeField] float secondsPerDay = 240f;
    [SerializeField] float startHour = 8f;
    [SerializeField] float nightStarts = 19f;
    [SerializeField] float nightEnds = 6f;

    [Header("Luz")]
    [SerializeField] Color dayLight = new Color(1f, 0.96f, 0.88f);
    [SerializeField] Color duskLight = new Color(1f, 0.6f, 0.35f);
    [SerializeField] Color nightLight = new Color(0.45f, 0.55f, 1f);
    [SerializeField] float dayIntensity = 1.3f;
    [SerializeField] float nightIntensity = 0.35f;

    public float Hour { get; private set; }
    public bool IsNight => Hour >= nightStarts || Hour < nightEnds;
    public string TimeText => $"{(int)Hour:00}:{(int)(Hour % 1f * 60f):00}";

    void Awake()
    {
        Instance = this;
        Hour = startHour;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
    }

    void Update()
    {
        Hour = (Hour + 24f / secondsPerDay * Time.deltaTime) % 24f;
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
            Hour = IsNight ? nightEnds : nightStarts;
        UpdateLighting();
    }

    void UpdateLighting()
    {
        float daylight = Mathf.Clamp01(Mathf.Min(Hour - (nightEnds - 0.5f), (nightStarts + 0.5f) - Hour));
        Color tint = daylight < 0.5f
            ? Color.Lerp(nightLight, duskLight, daylight * 2f)
            : Color.Lerp(duskLight, dayLight, (daylight - 0.5f) * 2f);
        if (sun != null)
        {
            sun.color = tint;
            sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylight);
        }
        RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.12f, 0.15f, 0.28f), new Color(0.62f, 0.68f, 0.78f), daylight);
        RenderSettings.ambientEquatorColor = Color.Lerp(new Color(0.08f, 0.10f, 0.18f), new Color(0.55f, 0.58f, 0.60f), daylight);
        RenderSettings.ambientGroundColor = Color.Lerp(new Color(0.05f, 0.05f, 0.08f), new Color(0.35f, 0.33f, 0.30f), daylight);
    }
}
