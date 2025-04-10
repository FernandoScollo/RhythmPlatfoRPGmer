using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(AudioSource))]
public class SpectrumData : MonoBehaviour
{
    AudioSource audioData;
    [Header("512 Variables to store fq information")]
    private float[] samplesLeft = new float[512];
    private float[] samplesRight = new float[512];

    [Header("8 Fq Bands peak and buffer")]
    public float[] frequencyBands = new float[8]; // 8 diffrent bands with peak and fast changes
    public float[] bandBuffer = new float[8]; // 8 diffrent bands with a smooth changes
    private float[] bufferDecreased = new float[8];

    [Header("64 Fq Bands peak and buffer")]
    private float[] frequencyBands64 = new float[64]; // 8 diffrent bands with peak and fast changes
    public float[] bandBuffer64 = new float[64]; // 8 diffrent bands with a smooth changes
    private float[] bufferDecreased64 = new float[64];
    public float[] frequencyBandHighest64 = new float[64];
    public float[] audioBand64;
    public float[] audioBandBuffer64;

    [Header("Variables for 0 to 1 variations FqBands and buffer")]
    public float[] frequencyBandHighest = new float[8];
    public float[] audioBand;
    public float[] audioBandBuffer;

    [Header("Total Fq sum amplitude")]
    public float _Amplitude, amplitudBuffer;
    float _peakAmplitude;

    [Header("Dinamic floor level for fq Bands")]
    public float audioProfile; 

    public enum StereoChannel
    {
        STEREO, LEFT, RIGHT
    }
    public StereoChannel stereoChannel;
    void Start()
    {
        audioData = GetComponent<AudioSource>();
        audioBand = new float[8];
        audioBandBuffer = new float[8];
        audioBand64 = new float[64];
        audioBandBuffer64 = new float[64];
    }
    void AudioProfile(float audioProfile)
    {
        for (int i =0; i<8; i++)
        {
            frequencyBandHighest[i] = audioProfile;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        MakeFrequencyBands64();
        BandBuffer64();
        CreateAudioBands();
        CreateAudioBands64();
        GetAmplitude();
    }
    void GetSpectrumAudioSource() // Gets a valuo for every 512 frequency range
    {
        audioData.GetSpectrumData(samplesLeft, 0, FFTWindow.Blackman);
        audioData.GetSpectrumData(samplesRight, 1, FFTWindow.Blackman);
    }
    void MakeFrequencyBands() // Gets a value for every 8 frequency range bands
    {   // 43 Hz per Sample
        //20 - 90 Hz  3 => 0 al 3
        //90 - 250 Hz 3 => 4 al 7
        //250 - 500 Hz 5 => 8 al 13
        //500 - 2000Hz 34 => 14 al 48
        //2000 - 4000 Hz 46 => 49 al 95
        //4000 - 6000 Hz 46 => 96 al 142
        //6000 - 12000 Hz 139 => 143 al 282
        //12000 - 20000 Hz 235 => 282 al 511
        int count = 0;
        for (int i=0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int) Mathf.Pow(2, i) * 2; // Manage the point where the frequency band-
                                                        //- change to store the information-
                                                        //- to the new frequency band
            if (i == 7) // add the 2 sample's rest from to the 510 samples
                sampleCount += 2;

        for (int j=0; j< sampleCount; j++) // calculate the smaples value sum 
            {
                if (stereoChannel == StereoChannel.STEREO)
                {
                    average +=(samplesLeft[count] + samplesRight[count]) * (count + 1);//??????? * (count +1)???????
                }
                if (stereoChannel == StereoChannel.LEFT)
                {
                    average +=samplesLeft[count]  * (count + 1);//??????? * (count +1)???????
                }
                if (stereoChannel == StereoChannel.RIGHT)
                {
                    average = average + samplesRight[count] * (count + 1);//??????? * (count +1)???????
                }
                count++; 
            }
            average = average / count; // divides the sample sum for the samples cuantity 
            frequencyBands[i] = average * 10; // multply the average by 10 to make litle changes more notorious
        }
    }
    void BandBuffer() // Gets a value for every 8 frequency range bands and smooth the decrease
    {
        for (int g = 0; g < 8; ++g)
        {
            if (frequencyBands[g] > bandBuffer[g])//if bandBuffer is smaller than freq band makes bandbufer freq band and restart bufferdecreased
            {
                bandBuffer[g] = frequencyBands[g];
                bufferDecreased[g] = 0.001f;
            }
            if (frequencyBands[g] < bandBuffer[g])//if freq band is smaller than buffer band buffer decrease
            {
                bandBuffer[g] = bandBuffer[g] - bufferDecreased[g];
                bufferDecreased[g] = bufferDecreased[g] * 1.1f;
            }
        }
    }
    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (frequencyBands[i] > frequencyBandHighest[i])
            {
                frequencyBandHighest = frequencyBands;
            }
            audioBand[i] = (frequencyBands[i] / frequencyBandHighest[i]);
            audioBandBuffer[i] = (bandBuffer[i] / frequencyBandHighest[i]);
        }
    }
    void MakeFrequencyBands64() // Gets a value for every 8 frequency range bands
    {  
        int count = 0;
        int sampleCount = 1;
        int power = 0;
        for (int i = 0; i < 64; i++)
        {
            if (sampleCount == 16 || sampleCount == 32 || sampleCount == 40 || sampleCount == 48 || sampleCount == 56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);
                if (power ==3)
                    sampleCount -= 2;
            }
            float average = 0;
            for (int j = 0; j < sampleCount; j++) // calculate the smaples value sum 
            {
                if (stereoChannel == StereoChannel.STEREO)
                {
                    average += (samplesLeft[count] + samplesRight[count]) * (count + 1);
                }
                if (stereoChannel == StereoChannel.LEFT)
                {
                    average += samplesLeft[count] * (count + 1);
                }
                if (stereoChannel == StereoChannel.RIGHT)
                {
                    average = average + samplesRight[count] * (count + 1);
                }
                count++;
            }
            average = average / count; // divides the sample sum for the samples cuantity 
            frequencyBands64[i] = average * 80; // multply the average by 10 to make litle changes more notorious
        }
    }
    void BandBuffer64() // Gets a value for every 8 frequency range bands and smooth the decrease
    {
        for (int g = 0; g < 64; ++g)
        {
            if (frequencyBands64[g] > bandBuffer64[g])//if bandBuffer is smaller than freq band makes bandbufer freq band and restart bufferdecreased
            {
                bandBuffer64[g] = frequencyBands64[g];
                bufferDecreased64[g] = 0.001f;
            }
            if (frequencyBands64[g] < bandBuffer64[g])//if freq band is smaller than buffer band buffer decrease
            {
                bandBuffer64[g] = bandBuffer64[g] - bufferDecreased64[g];
                bufferDecreased64[g] = bufferDecreased64[g] * 1.1f;
            }
        }
    }
    void CreateAudioBands64()
    {
        for (int i = 0; i < 8; i++)
        {
            if (frequencyBands64[i] > frequencyBandHighest64[i])
            {
                frequencyBandHighest64 = frequencyBands64;
            }
            audioBand64[i] = (frequencyBands64[i] / frequencyBandHighest64[i]);
            audioBandBuffer64[i] = (bandBuffer64[i] / frequencyBandHighest64[i]);
        }
    }
    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;
        for (int i=0; i<8; i++)
        {
            currentAmplitude += frequencyBands[i];
            currentAmplitudeBuffer += bandBuffer[i];
        }
        //Debug.Log("current amplitud: " + currentAmplitude);
        if (currentAmplitude > _peakAmplitude)
        {
            _peakAmplitude = currentAmplitude;
        }
        _Amplitude = currentAmplitude / _peakAmplitude;
        amplitudBuffer = currentAmplitudeBuffer / _peakAmplitude;
    }
}
