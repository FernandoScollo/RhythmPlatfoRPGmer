# Boss chase prototype

Scene: Level 1 Optimización de Sistemas 1.
Boss root: dynamic Rigidbody2D, continuous detection, solid BoxCollider2D, BossChaseAI.
BossBody layer 14 collides only with Ground 6. Keep the child damage trigger on Default!
The old route Animator is disabled, not deleted. Body/eye sorting orders are -10/-9.

Code defaults (scene overrides may differ): movement 5.5, jump 17, gravity 3; charge 12 for up to 2 seconds;
stomp jump 22, horizontal speed capped at 10. Preparations, cooldowns, recovery and events
are configurable. Contact damage remains 2 for both attacks, with existing invulnerability.

## Melee synchronized to B

Preparation now lasts until the NEXT OnPlatformsBActivated event, rather than a fixed delay.
The old preparation durations no longer apply. WaitingRhythmB is appended to the enum to
preserve serialized values. Boss stops horizontally while waiting; gravity stays active.
The event is consumed in FixedUpdate after the controller completes all platform changes
(Boss execution order 50). It rechecks grounded, range, height/reachability, and charge
wall/edge safety; invalid attacks return to Chase without storing the beat. Successful attacks
start immediately without another preparation timer and continue if A starts afterwards.
Rhythm Controller may be assigned explicitly; otherwise BeginChase uses the singleton.
Disable/death cancels the pending attack. Disable unsubscribes; BeginChase rebinds once.

Additional manual tests (Play Mode pending):
- Select charge/stomp during A: wait stationary until the next B, then execute once.
- Select during already-active B: do not execute until B turns off and on again.
- Player leaves range before B: cancel and resume pursuit; do not attack remotely.
- Lose ground / wall appears before B: charge cancels; gravity remains active while waiting.
- Change to A during charge/stomp: finish attack normally.
- Disable/re-enable Boss during wait: no stale pulse or duplicate event listener.
- Projectile A movement and Rhythm C firing remain independent.

Play Mode checklist:
1. Boss inactive until instrument collected; then follows the configured player.
2. Turns both directions, rests on Ground, ignores all rhythm platforms even when they activate.
3. Near player: charge windup, fixed direction charge, recovery. Wall/cliff interrupts charge.
4. Next reachable attack: stomp windup, leap toward position sampled at takeoff, landing, recovery.
5. Contact respects MLHealth invulnerability and facing-direction knockback.
6. Death/escape disables Boss. Scene restart resets state and velocity on next pickup.
7. Test small ledges, high walls, gaps and narrow passages; verify no teleportation.
8. Check visual ordering against platforms and backgrounds.

Limitations: local obstacle sensing, not global platform pathfinding. Large body (11.2 x 11.5)
cannot use player-sized passages. Landing probes are heuristic, not full trajectory planning.
Play Mode needs validation in Unity; cached DLL compilation is not a gameplay test.
