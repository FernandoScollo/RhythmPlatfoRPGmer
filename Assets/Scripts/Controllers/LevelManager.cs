using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    public enum LevelState { Exploration, Chase, Completed, Failed }
    [SerializeField] private InstrumentsCollectible objective;
    [SerializeField] private GameObject boss;
    [SerializeField] private GameObject bossGround;
    [SerializeField] private GameObject player;
    [SerializeField] private LevelState currentState = LevelState.Exploration;
    public LevelState CurrentState => currentState;
    public UnityEvent OnChaseStarted = new UnityEvent();
    public UnityEvent OnLevelCompleted = new UnityEvent();
    public UnityEvent OnLevelFailed = new UnityEvent();
    private MLHealth playerHealth;
    private MLHealth bossHealth;

    private void Awake()
    {
        currentState = LevelState.Exploration;
        if (boss != null) boss.SetActive(false);
        if (bossGround != null) bossGround.SetActive(false);
        if (boss != null) bossHealth = boss.GetComponent<MLHealth>();
        if (player != null) playerHealth = player.GetComponent<MLHealth>();
        if (objective == null || boss == null || playerHealth == null)
            Debug.LogError("LevelManager requires an objective, Boss and player with MLHealth.", this);
    }

    private void OnEnable()
    {
        if (objective != null) objective.OnCollected.AddListener(StartChase);
        if (playerHealth != null) playerHealth.OnDied.AddListener(FailLevel);
        if (bossHealth != null) bossHealth.OnDied.AddListener(BossDefeated);
    }

    private void Start()
    {
        if (playerHealth != null && playerHealth.IsDead) FailLevel();
        else if (objective != null && objective.IsCollected) StartChase();
    }

    private void OnDisable()
    {
        if (objective != null) objective.OnCollected.RemoveListener(StartChase);
        if (playerHealth != null) playerHealth.OnDied.RemoveListener(FailLevel);
        if (bossHealth != null) bossHealth.OnDied.RemoveListener(BossDefeated);
    }

    public void StartChase()
    {
        if (currentState != LevelState.Exploration || boss == null ||
            objective == null || !objective.IsCollected ||
            playerHealth == null || playerHealth.IsDead) return;
        currentState = LevelState.Chase;
        if (bossGround != null) bossGround.SetActive(true);
        boss.SetActive(true);
        BossChaseAI chase = boss.GetComponent<BossChaseAI>();
        if (chase != null) chase.BeginChase(player.transform);
        ProjectilShooter shooter = boss.GetComponent<ProjectilShooter>();
        if (shooter != null) shooter.Activate(player.transform);
        OnChaseStarted.Invoke();
    }

    // Future boss-death and exit-zone systems call these methods.
    public void BossDefeated() { CompleteLevel(); }
    public void PlayerEscaped() { CompleteLevel(); }

    private void CompleteLevel()
    {
        if (currentState != LevelState.Chase) return;
        currentState = LevelState.Completed;
        if (bossGround != null) bossGround.SetActive(false);
        if (boss != null) boss.SetActive(false);
        OnLevelCompleted.Invoke();
    }

    public void FailLevel()
    {
        if (currentState == LevelState.Completed || currentState == LevelState.Failed) return;
        currentState = LevelState.Failed;
        if (bossGround != null) bossGround.SetActive(false);
        if (boss != null) boss.SetActive(false);
        OnLevelFailed.Invoke();
    }
}
