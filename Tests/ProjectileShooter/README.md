# Rhythm C projectile shooter

Configured in Level 1 Optimización de Sistemas 1, Boss > ProjectilShooter.
LevelManager.Activate passes the player after activating Boss. An unassigned rhythm source
resolves to RhythmPlatformController.Instance.channelC at activation; other users can assign
any RangeFrecuencyObtainer and call Activate(target)/Deactivate(). Disabling shooter cleans up shots.

Defaults: interval .5 seconds, life 3 seconds of effective movement, pool 10, speed 7, turning 180 degrees/second,
damage 1, radius .2, trail .3 seconds. Muzzle can be assigned; otherwise spawn is boss world
position + offset (0,3), with horizontal offset following facing but not inheriting scale.

The runtime pool builds its 10 placeholder objects once in Awake, in a separate world-space root
in the same scene. There is no Instantiate/Destroy per shot and no growth when full. This first
version builds the template in code, not in a saved projectile prefab. Sprite and shared material
are assigned in the scene and can be replaced in the shooter Inspector. No generated image assets.
EnemyProjectile is layer 15. Explicit overlap/circle casts handle player + configured obstacle
layers (Ground, Walls, Rhythm A/B/C), ignore triggers except the target player, owner and other
HomingProjectile objects. Kinematic trigger bodies do not push characters. An appearing platform
is detected on the next projectile physics update. Trails are cleared after positioning on reuse.

Play Mode acceptance checklist (manual, not yet executed):
1. Before instrument pickup: zero shots. Pickup enables shooter, including if C is already high.
2. Hold C high for >3 s: shots separated by at least .5 s; no more than pool capacity active.
3. Toggle C rapidly: never bypass cooldown. Below threshold: no new shots; existing ones continue.
4. Dodge: limited turning, no instantaneous reversal. Target center is player collider center.
5. Ground / active rhythm platform hit: return to pool. Disabled platform: pass through.
6. Activate platform on shot / spawn inside ground: immediate or next-fixed-step return.
7. Hit player: MLHealth damage once, return even if invulnerable. Death during hit cleans all shots.
8. Boss and other projectiles: ignored; flipping/scaling Boss never moves existing shots.
9. Expiry after 3 s of movement returns shot. Reuse has fresh timer/target and no trail connecting old position.
10. Disable shooter/Boss, complete level or die: all shots returned. Reactivate resets deadline.
11. Inspect placeholder/trail material rendering with project's URP renderer and in a build.

Build check: dotnet build Assembly-CSharp.csproj --no-restore -p:BuildProjectReferences=false
with CustomAfterMicrosoftCommonTargets pointing to this folder's Validation.targets.
This checks against local Unity DLLs; it is not a Play Mode or rendering test.

## A/B movement gate

Movement Rhythm defaults to RhythmPlatformController.Instance; it can be assigned explicitly.
Only actual A-active / B-inactive state allows movement. Neither active, B active, or a missing/
disabled controller pauses movement AND lifetime. Rhythm C still controls firing independently.
Projectile physics runs after the platform controller (DefaultExecutionOrder 100). During pause,
kinematic velocity is zero and trail emission stops without clearing the existing fading trail.
Overlap collisions and target/death cleanup still run. Pool exhaustion simply skips the shot.

Additional Play Mode checks:
- A for 1 s, B for 5 s, A for 2 s: expires after approximately 8 wall-clock seconds, not 3.
- Spawn during B: remains still, keeps full movement lifetime, no emitted trail.
- Switch A/B repeatedly: no drift, turning or timer consumption during B.
- Player touches a frozen projectile or platform appears over it: still resolves impact.
- Both A and B active through direct calls: B takes priority; neither active also pauses.
- Disable movement controller: shots pause; disable Boss: all shots are returned regardless of pause.
- Fill pool during a long B interval: no growth/no catch-up burst when A resumes.
