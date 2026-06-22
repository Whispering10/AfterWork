using UnityEngine;

public class EnemyDeathRecorder : MonoBehaviour
{
    private HealthController healthController;
    private PlayerStatsRecorder statsRecorder;

    public void SetStatsRecorder(PlayerStatsRecorder recorder)
    {
        statsRecorder = recorder;
    }

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        if (healthController != null)
            healthController.HealthIsNull += OnDeath;
    }

    private void OnDestroy()
    {
        if (healthController != null)
            healthController.HealthIsNull -= OnDeath;
    }

    private void OnDeath(object sender, System.EventArgs e)
    {
        if (statsRecorder != null)
        {
            statsRecorder.OnEnemyKilled();
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerStatsRecorder recorder = player.GetComponent<PlayerStatsRecorder>();
                if (recorder != null) recorder.OnEnemyKilled();
            }
        }
    }
}