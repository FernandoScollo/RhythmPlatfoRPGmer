using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UnMuteSpecialChannels : MonoBehaviour
{
    public AudioMixer audioMixer; // Asigna tu AudioMixer aquí
    public string exposedParam; // El nombre del parámetro expuesto
    public float fadeDuration = 2f; // Duración del fade en segundos
    private bool isFading = false; // Para evitar múltiples activaciones

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFading)
        {
            StartCoroutine(FadeInVolume());
        }
    }
    private IEnumerator FadeInVolume()
    {
        Debug.Log("Me ejecuto");
        isFading = true;
        float startVolume;
        audioMixer.GetFloat(exposedParam, out startVolume);
        startVolume = Mathf.Pow(10, startVolume / 20); // Convertir dB a un rango lineal (0-1)

        float targetVolume = 1f; // Máximo volumen (lineal)
        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVolume) * 20); // Convertir de vuelta a dB
            yield return null;
        }

        audioMixer.SetFloat(exposedParam, Mathf.Log10(targetVolume) * 20); // Asegurarse de que alcanza el volumen final
        isFading = false;
    }
}
