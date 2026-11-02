using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
[RequireComponent(typeof(MLPlayerMovement))]
public class PlayerSFXManager : MonoBehaviour
{
    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] normalFootsteps;
    [SerializeField] private AudioClip[] woodFootsteps;
    [SerializeField] private AudioClip[] sandFootsteps;
    [Header("Footstep Mix")]
    [SerializeField] private AudioMixerGroup outputGroup;
    [SerializeField, Range(0f, 1f)] private float normalVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float surfaceVolume = 1f;

    [SerializeField] private AudioSource normalSource;
    [SerializeField] private AudioSource surfaceSource;
    private MLPlayerMovement movement;
    private MLHealth health;
    private AudioClip lastNormal, lastWood, lastSand;

    private void Awake()
    {
        movement = GetComponent<MLPlayerMovement>();
        health = GetComponent<MLHealth>();
        normalSource = CreateSource("Normal Footsteps");
        surfaceSource = CreateSource("Surface Footsteps");
    }

    private AudioSource CreateSource(string sourceName)
    {
        GameObject channel = new GameObject(sourceName);
        channel.transform.SetParent(transform, false);
        AudioSource source = channel.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = outputGroup;
        return source;
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied.AddListener(StopFootsteps);
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied.RemoveListener(StopFootsteps);
        StopFootsteps();
    }

    public void PlayFootstep()
    {
        if (!isActiveAndEnabled || !movement.enabled || !movement.IsMoving ||
            movement.IsDashActive || (health != null && health.IsDead)) return;
        Collider2D ground = movement.GetGroundCollider();
        if (ground == null) return;

        // Two dedicated channels; replace tails instead of stacking one-shots.
        StopFootsteps();
        Play(normalSource, Choose(normalFootsteps, ref lastNormal), normalVolume);
        if (ground.CompareTag("Wood"))
            Play(surfaceSource, Choose(woodFootsteps, ref lastWood), surfaceVolume);
        else if (ground.CompareTag("Sand"))
            Play(surfaceSource, Choose(sandFootsteps, ref lastSand), surfaceVolume);
    }

    private static AudioClip Choose(AudioClip[] clips, ref AudioClip previous)
    {
        if (clips == null) return null;
        int alternatives = 0;
        AudioClip fallback = null;
        foreach (AudioClip clip in clips)
        {
            if (clip == null) continue;
            fallback = clip;
            if (clip != previous) alternatives++;
        }
        if (alternatives == 0) return previous = fallback;
        int selected = Random.Range(0, alternatives);
        foreach (AudioClip clip in clips)
            if (clip != null && clip != previous && selected-- == 0)
                return previous = clip;
        return null;
    }

    private void Play(AudioSource source, AudioClip clip, float volume)
    {
        if (clip == null) return;
        source.outputAudioMixerGroup = outputGroup;
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }

    private void StopFootsteps()
    {
        if (normalSource != null) normalSource.Stop();
        if (surfaceSource != null) surfaceSource.Stop();
    }
}
