using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MLPlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private MLPlayerMovement playerMovement;
    [SerializeField] private PlayerDashSkill guitarDashSkill;
    [SerializeField] private PlayerBomboDashSkill bomboDashSkill;

    [Header("Basic Animation States")]
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string walkState = "Walk";
    [SerializeField] private string jumpState = "Jump";
    [SerializeField] private string airJumpState = "AirJump";
    [SerializeField] private string landState = "Land";

    [Header("Guitar Dash Animation States")]
    [SerializeField] private string guitarDashState = "GuitarDash";
    [SerializeField] private string guitarDashLandState = "GuitarDashLand";

    [Header("Bombo Dash Animation States")]
    [SerializeField] private string bomboDashState = "BomboDash";
    [SerializeField] private string bomboDashLandState = "BomboDashLand";

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.05f;
    [SerializeField] private float landTransitionDuration = 0.12f;

    private string currentState;

    private bool guitarDashAnimationFinished;
    private bool bomboDashAnimationFinished;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<MLPlayerMovement>();

        if (guitarDashSkill == null)
            guitarDashSkill = GetComponent<PlayerDashSkill>();

        if (bomboDashSkill == null)
            bomboDashSkill = GetComponent<PlayerBomboDashSkill>();
    }

    private void OnEnable()
    {
        // Player Movement
        if (playerMovement != null)
        {
            playerMovement.OnJump.AddListener(HandleJump);
            playerMovement.OnLand.AddListener(HandleLand);
        }

        // Guitar Dash
        if (guitarDashSkill != null)
        {
            guitarDashSkill.OnDashStarted.AddListener(HandleGuitarDashStarted);
            guitarDashSkill.OnDashFinished.AddListener(HandleGuitarDashFinished);
        }

        // Bombo Dash
        if (bomboDashSkill != null)
        {
            bomboDashSkill.OnDashStarted.AddListener(HandleBomboDashStarted);
            bomboDashSkill.OnDashFinished.AddListener(HandleBomboDashFinished);
        }
    }

    private void OnDisable()
    {
        // Player Movement
        if (playerMovement != null)
        {
            playerMovement.OnJump.RemoveListener(HandleJump);
            playerMovement.OnLand.RemoveListener(HandleLand);
        }

        // Guitar Dash
        if (guitarDashSkill != null)
        {
            guitarDashSkill.OnDashStarted.RemoveListener(HandleGuitarDashStarted);
            guitarDashSkill.OnDashFinished.RemoveListener(HandleGuitarDashFinished);
        }

        // Bombo Dash
        if (bomboDashSkill != null)
        {
            bomboDashSkill.OnDashStarted.RemoveListener(HandleBomboDashStarted);
            bomboDashSkill.OnDashFinished.RemoveListener(HandleBomboDashFinished);
        }
    }

    private void Start()
    {
        PlayAnimation(idleState);
    }

    private void Update()
    {
        if (animator == null || playerMovement == null)
            return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // =================================================
        // GUITAR DASH
        // =================================================

        if (currentState == guitarDashState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (!guitarDashAnimationFinished &&
                stateInfo.IsName(guitarDashState) &&
                stateInfo.normalizedTime >= 1f)
            {
                guitarDashAnimationFinished = true;

                // Freeze on last frame
                animator.speed = 0f;
            }

            return;
        }

        // =================================================
        // GUITAR DASH LAND
        // =================================================

        if (currentState == guitarDashLandState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(guitarDashLandState) &&
                stateInfo.normalizedTime >= 1f)
            {
                GoToGroundAnimation();
            }

            return;
        }

        // =================================================
        // BOMBO DASH
        // =================================================

        if (currentState == bomboDashState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (!bomboDashAnimationFinished &&
                stateInfo.IsName(bomboDashState) &&
                stateInfo.normalizedTime >= 1f)
            {
                bomboDashAnimationFinished = true;

                // Freeze on last frame
                animator.speed = 0f;
            }

            return;
        }

        // =================================================
        // BOMBO DASH LAND
        // =================================================

        if (currentState == bomboDashLandState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(bomboDashLandState) &&
                stateInfo.normalizedTime >= 1f)
            {
                GoToGroundAnimation();
            }

            return;
        }

        // =================================================
        // JUMP
        // =================================================

        if (currentState == jumpState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(jumpState) &&
                stateInfo.normalizedTime >= 1f)
            {
                PlayAnimation(airJumpState);
            }

            return;
        }

        // =================================================
        // AIR JUMP
        // =================================================

        if (currentState == airJumpState)
        {
            return;
        }

        // =================================================
        // LAND
        // =================================================

        if (currentState == landState)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(landState) &&
                stateInfo.normalizedTime >= 1f)
            {
                GoToGroundAnimation();
            }

            return;
        }

        // =================================================
        // GROUND
        // =================================================

        if (playerMovement.IsGrounded)
        {
            GoToGroundAnimation();
        }
    }

    // =====================================================
    // JUMP
    // =====================================================

    private void HandleJump()
    {
        // No interrumpir un dash
        if (IsDashing())
            return;

        PlayAnimation(jumpState);
    }

    // =====================================================
    // LAND
    // =====================================================

    private void HandleLand()
    {
        // No interrumpir un dash
        if (IsDashing())
            return;

        PlayAnimation(landState);
    }

    // =====================================================
    // GUITAR DASH START
    // =====================================================

    private void HandleGuitarDashStarted()
    {
        // Seguridad
        if (bomboDashSkill != null && bomboDashSkill.IsDashing)
            return;

        guitarDashAnimationFinished = false;

        animator.speed = 1f;

        PlayAnimation(guitarDashState);
    }

    // =====================================================
    // GUITAR DASH FINISH
    // =====================================================

    private void HandleGuitarDashFinished()
    {
        guitarDashAnimationFinished = false;

        animator.speed = 1f;

        PlayAnimation(guitarDashLandState);
    }

    // =====================================================
    // BOMBO DASH START
    // =====================================================

    private void HandleBomboDashStarted()
    {
        // Seguridad
        if (guitarDashSkill != null && guitarDashSkill.IsDashing)
            return;

        bomboDashAnimationFinished = false;

        animator.speed = 1f;

        PlayAnimation(bomboDashState);
    }

    // =====================================================
    // BOMBO DASH FINISH
    // =====================================================

    private void HandleBomboDashFinished()
    {
        bomboDashAnimationFinished = false;

        animator.speed = 1f;

        PlayAnimation(bomboDashLandState);
    }

    // =====================================================
    // GROUND ANIMATION
    // =====================================================

    private void GoToGroundAnimation()
    {
        if (playerMovement.IsMoving)
            PlayAnimation(walkState, landTransitionDuration);
        else
            PlayAnimation(idleState, landTransitionDuration);
    }

    // =====================================================
    // CHECK DASH
    // =====================================================

    private bool IsDashing()
    {
        bool guitarDashing =
            guitarDashSkill != null &&
            guitarDashSkill.IsDashing;

        bool bomboDashing =
            bomboDashSkill != null &&
            bomboDashSkill.IsDashing;

        return guitarDashing || bomboDashing;
    }

    // =====================================================
    // PLAY ANIMATION
    // =====================================================

    private void PlayAnimation(string stateName)
    {
        PlayAnimation(stateName, transitionDuration);
    }

    private void PlayAnimation(string stateName, float duration)
    {
        if (currentState == stateName)
            return;

        currentState = stateName;

        animator.CrossFade(
            stateName,
            duration
        );
    }
}