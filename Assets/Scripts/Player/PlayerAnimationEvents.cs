using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerSFXManager playerSFXManager;

    private void Awake()
    {
        if (playerSFXManager == null)
            playerSFXManager = GetComponentInParent<PlayerSFXManager>();
    }

    // Animation Events are delivered to the Animator's GameObject.
    public void Footstep()
    {
        if (isActiveAndEnabled && playerSFXManager != null)
            playerSFXManager.PlayFootstep();
    }
}
