using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSourceController : MonoBehaviour
{
    public AudioSource[] sourceMusic;
    public AudioMixer mixer;
    public string specialMixerChanel;

    private void Start()
    {
        MuteSelectedChannels(specialMixerChanel);
        StartCoroutine(PlayAfterTime(1f));

    }

    private void OnEnable()
    {
       // StartCoroutine(PlayMusicSources());
    }
    public IEnumerator PlayMusicSources()
    {
        for (int i=0; i< sourceMusic.Length; i++)
        {
            sourceMusic[i].Play();
        }
        yield return null;
    }
    public IEnumerator PlayAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(PlayMusicSources());
    }
    void MuteSelectedChannels(string channelToMute)
    {
        float startVolume = Mathf.Log10(0.0001f) * 20;
        mixer.SetFloat(channelToMute, startVolume);
    }
}
