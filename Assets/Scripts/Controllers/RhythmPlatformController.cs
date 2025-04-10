using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlatformController : MonoBehaviour
{
    public RhythmPlatform kick;
    public RhythmPlatform snare;
    public RhythmPlatform channelC;
    // Plataformas A
    [SerializeField] List<SpriteRenderer> platformASpt;
    [SerializeField] List<SpriteRenderer> platformBSpt;
    // Plataformas B
    [SerializeField] List<BoxCollider2D> platformACld;
    [SerializeField] List<BoxCollider2D> platformBCld;
    // Plataformas C
    [SerializeField] List<SpriteRenderer> platformCSpt;
    [SerializeField] List<BoxCollider2D> platformCCld;


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
        if(kick.SoundON())
        {
            PlatformsAState(true);
            PlatformsBState(false);
            platformABlocked = true;
            platformBBlocked = false;
        }
        if (snare.SoundON())
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
        foreach (SpriteRenderer spritesA in platformASpt)
        {
            spritesA.enabled = isActive;
        }
        foreach (BoxCollider2D CollidersA in platformACld)
        {
            CollidersA.enabled = isActive;
        }
    }
    public void PlatformsBState(bool isActive)
    {
        foreach (SpriteRenderer spritesB in platformBSpt)
        {
            spritesB.enabled = isActive;
        }
        foreach (BoxCollider2D CollidersB in platformBCld)
        {
            CollidersB.enabled = isActive;
        }
    }
    public void PlatformsCState(bool isActive)
    {
        foreach (SpriteRenderer spritesC in platformCSpt)
        {
            spritesC.enabled = isActive;
        }
        foreach (BoxCollider2D CollidersC in platformCCld)
        {
            CollidersC.enabled = isActive;
        }
    }

}
