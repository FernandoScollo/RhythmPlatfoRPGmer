using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAudioTimeController : MonoBehaviour
{
    public AudioSource musicSource;
    public Animator animationController;

    public string animationName;
    public float[] timeToTrigger;

    public float musicTimepassed;

    private void Update()
    {
        TimePassed();
        for(int i = 0; i < timeToTrigger.Length; i++)
        {
            if (musicTimepassed == timeToTrigger[i])
            {
                Debug.Log("He sido ejecutado");
                animationController.SetBool(animationName, true);
            }
            else if (musicTimepassed == timeToTrigger[i]+2)
                animationController.SetBool(animationName, false);

        }
        
    }

    void TimePassed()
    {
        musicTimepassed = Mathf.Round(musicSource.time);
    }
}
