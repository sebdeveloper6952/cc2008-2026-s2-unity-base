using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    [SerializeField] AudioClip newOrder;
    [SerializeField, Range(0f, 1f)] float newOrderVolume = 0.6f;
    [SerializeField] AudioClip orderDelivered;
    [SerializeField, Range(0f, 1f)] float orderDeliveredVolume = 0.8f;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    void OnEnable()
    {
        OrderBoard.OrderPlaced += OnOrderPlaced;
        OrderBoard.OrderDelivered += OnOrderDelivered;
    }

    void OnDisable()
    {
        OrderBoard.OrderPlaced -= OnOrderPlaced;
        OrderBoard.OrderDelivered -= OnOrderDelivered;
    }

    void OnOrderPlaced(Order order) => Play(newOrder, newOrderVolume);

    void OnOrderDelivered(Order order) => Play(orderDelivered, orderDeliveredVolume);

    void Play(AudioClip clip, float volume)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }
}
