using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RhythmPlatformController : MonoBehaviour
{
    public static RhythmPlatformController Instance;

    [Header("Frequency Detection")]
    public RangeFrecuencyObtainer kick;
    public RangeFrecuencyObtainer snare;
    public RangeFrecuencyObtainer channelC;

    // =====================================================
    // PLATFORMS A
    // =====================================================

    [Header("Platforms A")]
    [SerializeField] private List<GameObject> A_Platforms;

    // =====================================================
    // PLATFORMS B
    // =====================================================

    [Header("Platforms B")]
    [SerializeField] private List<GameObject> B_Platforms;

    // =====================================================
    // PLATFORMS C
    // =====================================================

    [Header("Platforms C")]
    [SerializeField] private List<GameObject> C_Platforms;

    // =====================================================
    // ACTIVATION EVENTS
    // =====================================================

    [Header("Platform Activation Events")]
    public UnityEvent OnPlatformsAActivated = new UnityEvent();
    public UnityEvent OnPlatformsBActivated = new UnityEvent();
    public UnityEvent OnPlatformsCActivated = new UnityEvent();

    // =====================================================
    // STATES
    // =====================================================

    public bool platformABlocked;
    public bool platformBBlocked;

    private bool platformsAActive;
    private bool platformsBActive;
    // Read the actual platform state, not a second threshold calculation.
    public bool ProjectileMovementAllowed => isActiveAndEnabled && platformsAActive && !platformsBActive;
    private bool platformsCActive;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        InitiatePlatforms();
    }

    // =====================================================
    // FIXED UPDATE
    // =====================================================

    private void FixedUpdate()
    {
        ManagePlatforms();
    }

    // =====================================================
    // INITIALIZE
    // =====================================================

    private void InitiatePlatforms()
    {
        PlatformsAState(false);
        PlatformsBState(false);
        PlatformsCState(false);

        platformsAActive = false;
        platformsBActive = false;
        platformsCActive = false;
    }

    // =====================================================
    // MANAGE PLATFORMS
    // =====================================================

    public void ManagePlatforms()
    {
        // =================================================
        // KICK → A
        // =================================================

        if (kick.SoundON())
        {
            PlatformsAState(true);
            PlatformsBState(false);

            platformABlocked = true;
            platformBBlocked = false;
        }

        // =================================================
        // SNARE → B
        // =================================================

        else if (snare.SoundON())
        {
            PlatformsAState(false);
            PlatformsBState(true);

            platformABlocked = false;
            platformBBlocked = true;
        }

        // =================================================
        // CHANNEL C → C
        // =================================================

        if (channelC.SoundON())
        {
            PlatformsCState(true);
        }
        else
        {
            PlatformsCState(false);
        }
    }

    // =====================================================
    // PLATFORMS A
    // =====================================================

    public void PlatformsAState(bool isActive)
    {
        // -----------------------------------------------
        // OFF → ON
        // -----------------------------------------------

        if (isActive && !platformsAActive)
        {
            OnPlatformsAActivated?.Invoke();

            foreach (GameObject platform in A_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                bool isEjecting = false;

                if (ejection != null)
                {
                    isEjecting =
                        ejection.PrepareForActivation();
                }

                // Sprite siempre aparece inmediatamente
                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = true;
                }

                // Si no hay expulsión, activamos collider
                // inmediatamente.
                if (!isEjecting)
                {
                    BoxCollider2D collider =
                        platform.GetComponent<BoxCollider2D>();

                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
        }

        // -----------------------------------------------
        // ON → OFF
        // -----------------------------------------------

        if (!isActive && platformsAActive)
        {
            foreach (GameObject platform in A_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                if (ejection != null)
                {
                    ejection.ResetActivation();
                }

                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = false;
                }

                BoxCollider2D collider =
                    platform.GetComponent<BoxCollider2D>();

                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        platformsAActive = isActive;
    }

    // =====================================================
    // PLATFORMS B
    // =====================================================

    public void PlatformsBState(bool isActive)
    {
        // -----------------------------------------------
        // OFF → ON
        // -----------------------------------------------

        if (isActive && !platformsBActive)
        {
            OnPlatformsBActivated?.Invoke();

            foreach (GameObject platform in B_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                bool isEjecting = false;

                if (ejection != null)
                {
                    isEjecting =
                        ejection.PrepareForActivation();
                }

                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = true;
                }

                if (!isEjecting)
                {
                    BoxCollider2D collider =
                        platform.GetComponent<BoxCollider2D>();

                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
        }

        // -----------------------------------------------
        // ON → OFF
        // -----------------------------------------------

        if (!isActive && platformsBActive)
        {
            foreach (GameObject platform in B_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                if (ejection != null)
                {
                    ejection.ResetActivation();
                }

                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = false;
                }

                BoxCollider2D collider =
                    platform.GetComponent<BoxCollider2D>();

                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        platformsBActive = isActive;
    }

    // =====================================================
    // PLATFORMS C
    // =====================================================

    public void PlatformsCState(bool isActive)
    {
        // -----------------------------------------------
        // OFF → ON
        // -----------------------------------------------

        if (isActive && !platformsCActive)
        {
            OnPlatformsCActivated?.Invoke();

            foreach (GameObject platform in C_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                bool isEjecting = false;

                if (ejection != null)
                {
                    isEjecting =
                        ejection.PrepareForActivation();
                }

                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = true;
                }

                if (!isEjecting)
                {
                    BoxCollider2D collider =
                        platform.GetComponent<BoxCollider2D>();

                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
        }

        // -----------------------------------------------
        // ON → OFF
        // -----------------------------------------------

        if (!isActive && platformsCActive)
        {
            foreach (GameObject platform in C_Platforms)
            {
                RhythmPlatforms ejection =
                    platform.GetComponent<RhythmPlatforms>();

                if (ejection != null)
                {
                    ejection.ResetActivation();
                }

                SpriteRenderer sprite =
                    platform.GetComponent<SpriteRenderer>();

                if (sprite != null)
                {
                    sprite.enabled = false;
                }

                BoxCollider2D collider =
                    platform.GetComponent<BoxCollider2D>();

                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        platformsCActive = isActive;
    }

    // =====================================================
    // ADD PLATFORM A
    // =====================================================

    public void AddAPlatform(GameObject platform)
    {
        if (!A_Platforms.Contains(platform))
        {
            A_Platforms.Add(platform);
        }
    }

    // =====================================================
    // ADD PLATFORM B
    // =====================================================

    public void AddBPlatform(GameObject platform)
    {
        if (!B_Platforms.Contains(platform))
        {
            B_Platforms.Add(platform);
        }
    }

    // =====================================================
    // ADD PLATFORM C
    // =====================================================

    public void AddCPlatform(GameObject platform)
    {
        if (!C_Platforms.Contains(platform))
        {
            C_Platforms.Add(platform);
        }
    }

}
