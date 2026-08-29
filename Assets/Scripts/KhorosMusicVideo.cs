using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KhorosMusicVideo : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioClip[] ostClips;
    int clip = 0;

    void Start()
    {
        StartCoroutine(PlayInOrder());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator PlayInOrder()
    {
        while(clip < ostClips.Length)
        {
            if(!musicPlayer.isPlaying)
            {
                StartCoroutine(PlayTheClip(clip));
            }
            yield return null;
        }
    }
    public IEnumerator PlayTheClip(int clipNumber)
    {
        musicPlayer.clip = ostClips[clipNumber];
        musicPlayer.Play();
        clip++;
        yield return null;

    }
}
