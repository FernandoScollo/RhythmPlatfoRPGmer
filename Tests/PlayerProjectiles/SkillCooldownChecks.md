# Dash cooldown and immediate chaining — Play Mode checklist

Scene: Level 1 Optimización de Sistemas 1.

- Both bars initially full. Press J: guitar bar empties and refills in 2 game-time
  seconds. K does the same for bombo. Rejected presses do not reset either timer.
- Press J then K before guitar ends: guitar finishes/cancels first, bombo starts
  immediately. Repeat in reverse. Exactly one projectile per accepted dash.
- After chaining, try the first skill before its own 2 seconds elapse: no movement,
  animation or projectile should start; the currently active dash is not cancelled.
- During death, platform freeze, knockback or pause, neither skill starts. Ground
  and air jumps retain their previous rules (air jump does not start either cooldown).
- Pause halfway through recharge: bars and cooldown freeze together. Unpause resumes.
- Disabling the UI does not change availability. Disabling/re-enabling a skill does
  not reset its timestamp; cancelling a dash never refunds its cooldown.
- Change cooldown durations in MLPlayerStats: the next use captures the new duration.
  Existing recharge retains its original duration. Zero cooldown is supported, but
  the same dash still cannot restart itself while running.
- Inspect animation transitions J -> K and K -> J; no old coroutine should stop the
  new dash when the old dash's original duration would have elapsed.

MLPlayerSkillUI is attached to the scene Canvas. It creates missing bars at runtime
using the supplied sprites. Existing Image references can be assigned instead;
configure Guitar Top and Bombo Top to reuse a manually arranged HUD. Top is filled
left-to-right over the static Botton UI. Layout is configured before entering Play.

Build validated; these interactive checks are not yet executed in Unity.
