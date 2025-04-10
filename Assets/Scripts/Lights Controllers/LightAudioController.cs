using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAudioController : MonoBehaviour
{
    public FrecuencyMusicManager fqModifiers;
    public UnityEngine.Rendering.Universal.Light2D light2D;
    float lightIntensity;

    private void Start()
    {
        lightIntensity = light2D.intensity;
    }
    private void Update()
    {
        IntensityLight();
    }
    void IntensityLight()
    {

        light2D.intensity = (lightIntensity + (fqModifiers.frequencyBandsValue[0]/4));
    }
}
