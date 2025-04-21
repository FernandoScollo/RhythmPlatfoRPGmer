using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeFrecuencyObtainer : MonoBehaviour
{
    public AudioSpectrum spectrum;
    public enum FrequencyBands
    {
        LowBass, Bass, MidBass, Mid, MidHigh, HighMid, High
    }
    //Cantidad de Bandas
    public FrequencyBands[] freqBands;
    //Valor para las frecuencias de banda
    private List<float> frequencyBandsValue = new List<float>();
    private List<float> frequencyBandsMax = new List<float>();
    //Sumarle un valor de base para que no haya valores menores a 0
    public bool baseValueEnabled;
    public float baseValue;
    // Esto modifica si es false valores Peak, si true valores con buffer
    public bool withBuffer;

    // Rhythm Platformer
    public float totalFqValues;
    public float threshold = 2f;

    private void Start()
    {
        spectrum = GetComponent<AudioSpectrum>();
        for (int i = 0; i < freqBands.Length; i++)
        {
            frequencyBandsValue.Add(0f);
            frequencyBandsMax.Add(0f);
        }
    }
    
    public void GetFrequencyBandsBuffer()
    {
        for (int i = 0; i < freqBands.Length; i++)
        {
            switch (freqBands[i])
            {
                case FrequencyBands.LowBass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[0] + 2 + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[0];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.Bass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[1] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[1];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.MidBass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[2] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[2];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.Mid:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[3] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[3];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.MidHigh:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[4] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[4];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.HighMid:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[5] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[5];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.High:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[6] + spectrum.bandBuffer[7] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.bandBuffer[6] + spectrum.bandBuffer[7];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
            }

        }
    }

    public void GetFrequencyBands()
    {
        for (int i = 0; i < freqBands.Length; i++)
        {
            switch (freqBands[i])
            {
                case FrequencyBands.LowBass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[0] + 2 + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[0];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.Bass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[1] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[1];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.MidBass:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[2] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[2];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.Mid:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[3] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[3];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.MidHigh:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[4] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[4];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.HighMid:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[5] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[5];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                case FrequencyBands.High:
                    if (baseValueEnabled)
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[6] + spectrum.frequencyBands[7] + baseValue;
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
                    else
                    {
                        frequencyBandsValue[i] = spectrum.frequencyBands[6] + spectrum.frequencyBands[7];
                        if (frequencyBandsValue[i] > frequencyBandsMax[i])
                            frequencyBandsMax[i] = frequencyBandsValue[i];
                        break;
                    }
            }
        }
    }
    private void Update()
    {
        if (withBuffer)
            GetFrequencyBandsBuffer();
        else
            GetFrequencyBands();
    }

    private void FixedUpdate()
    {
        GetTotalFqValues();
    }

    public void GetTotalFqValues()
    {
        totalFqValues = 0;
        for (int i = 0; i < frequencyBandsValue.Count; i++)
        {
            totalFqValues += frequencyBandsValue[i];
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
