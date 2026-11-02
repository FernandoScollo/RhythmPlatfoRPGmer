# Player skill projectiles — manual Play Mode checks

Scene: `Assets/Scenes/Levels/Level 1 Optimización de Sistemas 1.unity`.

1. Enter Play Mode: Boss and Boss Ground must be inactive. Collect the instrument:
   both activate; Boss MLHealth shows 15. Existing chase, rhythm attacks and C shooter remain functional.
2. Face right and press J, then K: each successful dash emits exactly one shot left.
   Face left and repeat: shots go right. Rejected skill input must not fire.
3. Ground jump emits nothing. Air jump emits one downward shot below the collider.
   Extra air jumps (Max Jumps > 2) also fire once each.
4. Change each of the three damage values in MLPlayerStats independently. Only its
   skill uses that damage; already flying shots retain their original damage.
   Instrument damage modifiers must add/subtract, clamped to zero.
5. Player shots keep moving during B and expire after 3 game-time seconds.
   Boss homing shots still pause during B and retain their effective-movement lifetime.
6. Hit Ground, Walls and active rhythm platforms: shots disappear. Inactive platforms
   do not block. Activate a platform over a shot: it disappears next physics step.
   Fire next to a thin wall: the muzzle offset must not allow shooting through it.
7. Hit the Boss: one damage application per shot, no physical shove or friendly fire.
   At default damage, 15 hits complete the level, disable Boss Ground and the Boss,
   and return Boss projectiles to their pool. Escape/failure also disables Boss Ground.
8. Player death/disable clears its projectiles. Scene reload leaves no old pool behind.
9. Fire repeatedly: the pool stays at 16 objects, skipping shots if exhausted.
   Reused shots must not retain an old trail, direction or damage.

Inspector tuning: MLPlayerStats (three damages), PlayerSkillProjectileEmitter on
MLPlayer 1 prefab (speed, life, radius, pool capacity, masks, sprite/material/trail),
Boss MLHealth (max health/invulnerability), LevelManager (Boss Ground reference).
Target Layers defaults to BossBody; add other enemies' hurtbox layers and MLHealth
to reuse this projectile system. Explicit queries do not change the physics matrix.

Compilation validated with Validation.targets (cached Unity references). Play Mode
checks above still require execution in Unity; no runtime result is claimed.
