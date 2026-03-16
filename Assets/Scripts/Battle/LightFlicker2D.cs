using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker2D : MonoBehaviour
{
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 3.5f;

    private Light2D _light;
    private float _randomizer;

    void Start()
    {
        _light = GetComponent<Light2D>();
        _randomizer = Random.Range(0f, 65535f);
    }

    void Update()
    {
        if (_light == null) return;
        float noise = Mathf.PerlinNoise(_randomizer, Time.time * flickerSpeed);
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
