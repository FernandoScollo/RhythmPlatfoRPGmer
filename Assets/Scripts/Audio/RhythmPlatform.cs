using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlatform : MonoBehaviour
{
    public FrecuencyMusicManager frequencyValues;
    public float totalFqValues;
    public float threshold;

    private void FixedUpdate()
    {
        GetTotalFqValues();
    }

    public void GetTotalFqValues()
    {
        totalFqValues = 0;
        for (int i=0; i < frequencyValues.frequencyBandsValue.Count; i++)
        {
            totalFqValues += frequencyValues.frequencyBandsValue[i];
        }
    }

    public bool SoundON()
    {
        if (totalFqValues > threshold)
            return true;
        else
            return false;
    }
    

}
