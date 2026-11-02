using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public enum SpikeRhythmType
{
    A,
    B,
    C
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip activationClip;

    [Header("Rhythm Synchronization")]
    [SerializeField] private SpikeRhythmType rhythmType =
        SpikeRhythmType.A;

    private RhythmPlatformController rhythmController;
    private SpikeRhythmType subscribedRhythmType;
    [SerializeField]private bool isSubscribed;
    private PlayableGraph animationGraph;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        DestroyAnimationGraph();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        rhythmController = RhythmPlatformController.Instance;

        if (rhythmController == null)
            return;

        subscribedRhythmType = rhythmType;

        switch (subscribedRhythmType)
        {
            case SpikeRhythmType.A:
                rhythmController.OnPlatformsAActivated.AddListener(
                    PlayActivationAnimation
                );
                break;

            case SpikeRhythmType.B:
                rhythmController.OnPlatformsBActivated.AddListener(
                    PlayActivationAnimation
                );
                break;

            case SpikeRhythmType.C:
                rhythmController.OnPlatformsCActivated.AddListener(
                    PlayActivationAnimation
                );
                break;
        }

        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || rhythmController == null)
            return;

        switch (subscribedRhythmType)
        {
            case SpikeRhythmType.A:
                rhythmController.OnPlatformsAActivated.RemoveListener(
                    PlayActivationAnimation
                );
                break;

            case SpikeRhythmType.B:
                rhythmController.OnPlatformsBActivated.RemoveListener(
                    PlayActivationAnimation
                );
                break;

            case SpikeRhythmType.C:
                rhythmController.OnPlatformsCActivated.RemoveListener(
                    PlayActivationAnimation
                );
                break;
        }

        isSubscribed = false;
    }

    public void PlayActivationAnimation()
    {
        Debug.Log("PLay Spike Trap");
        if (animator == null || activationClip == null)
            return;

        DestroyAnimationGraph();

        animationGraph = PlayableGraph.Create(
            $"{name} Spike Animation"
        );

        AnimationPlayableOutput animationOutput =
            AnimationPlayableOutput.Create(
                animationGraph,
                "Spike Animation",
                animator
            );

        AnimationClipPlayable clipPlayable =
            AnimationClipPlayable.Create(
                animationGraph,
                activationClip
            );

        animationOutput.SetSourcePlayable(clipPlayable);
        animationGraph.Play();
    }

    private void DestroyAnimationGraph()
    {
        if (animationGraph.IsValid())
            animationGraph.Destroy();
    }
}
