# Rhythm activation / freeze checks

Activation is synchronous: PrepareForActivation enables the collider immediately and returns
true to tell the existing controller activation is handled (not pending). No ejection coroutine.
Standing on top without penetration does not cause damage or separation. Four axis-aligned
exits are checked by a swept player bounds box plus destination overlap, ignoring the newly
appeared platform, self, triggers and already-ignored pairs. Smallest safe exit wins.
If blocked, the player freezes with zero velocity/gravity and FreezeAll constraints; only
contacts with the trapping platform are ignored. Each overlapping activation owns a lock.
Disabling a platform releases its lock; last release restores constraints and zero velocity.
Dash, jump and knockback cannot start while frozen. Existing damage occurs once per activation,
not every frame; freeze itself never calls Kill or adds damage. Existing damage can still be lethal.
Player Rigidbody uses continuous collision detection. Animator pauses during freeze.

Play Mode checklist (not yet run automatically):
- Spawn a platform around player in open space: immediate collider, safe shortest exit, one damage event.
- Player stands exactly on top: no ejection/freeze.
- Floor below and wall on one side: never push through either; use another free side.
- Fully enclosed: immediate activation, zero movement/gravity, no repeated damage.
- Hold movement/jump/J/K, apply boss knockback while frozen: position remains fixed.
- Overlap A and C: disabling A alone must not release; disabling C restores control.
- Alternate A/B on the same physics frame and repeat several cycles: no stale locks.
- Disable/destroy a trapping platform: restore player constraints, no stored impulse.
- Freeze while dashing: dash cancelled and animation resumes on release.
- Death and scene reload: no retained collision-ignore state.
- Verify Ground contacts remain enabled before/during/after freeze.

Safe separation is an instantaneous validated relocation, not the previous timed push.
Old ejectionDuration/pushSpeed serialized values no longer control activation.
