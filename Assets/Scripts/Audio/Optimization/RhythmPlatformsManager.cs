using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlatformsManager : MonoBehaviour
{
    public RangeFrecuencyObtainer kick;
    public RangeFrecuencyObtainer snare;
    public RangeFrecuencyObtainer channelC;
    // Plataformas A

    [SerializeField] List<GameObject> A_Platforms;
    // Plataformas B
    [SerializeField] List<GameObject> B_Platforms;
    // Plataformas C
    [SerializeField] List<GameObject> C_Platforms;


    public bool platformABlocked, platformBBlocked;

    private void Start()
    {
        InitiatePlatforms();
    }

    private void FixedUpdate()
    {
        ManagePlatforms();
    }

    void InitiatePlatforms()
    {
        PlatformsAState(false);
        PlatformsBState(false);
        PlatformsCState(false);
    }
   
    public void ManagePlatforms()
    {
        if (kick.SoundON())
        {
            PlatformsAState(true);
            PlatformsBState(false);
            platformABlocked = true;
            platformBBlocked = false;
        }
        else if (snare.SoundON())
        {
            PlatformsAState(false);
            PlatformsBState(true);
            platformABlocked = false;
            platformBBlocked = true;
        }

        if (channelC.SoundON())
            PlatformsCState(true);
        else
            PlatformsCState(false);
    }



    public void PlatformsAState(bool isActive)
    {
        foreach(GameObject Aplatform in A_Platforms)
        {
            Aplatform.SetActive(isActive);
        }
    }
    public void PlatformsBState(bool isActive)
    {
        foreach (GameObject Bplatform in B_Platforms)
        {
            Bplatform.SetActive(isActive);
        }
    }
    public void PlatformsCState(bool isActive)
    {
        foreach (GameObject Cplatform in C_Platforms)
        {
            Cplatform.SetActive(isActive);
        }
    }
}
