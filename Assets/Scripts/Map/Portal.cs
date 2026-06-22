using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GenerateMap generateMap;
    [SerializeField] private GameObject player;
    [SerializeField] private CameraMove cameraMove;
    [SerializeField] private PlayerStatsRecorder playerStatsRecorder; // НОВОЕ

    [Header("Health Settings")]
    [SerializeField] private bool restoreHealthOnPortal = true;

    private Vector3 playerStartPosition;
    private HealthController healthController;

    private void Start()
    {
        if (generateMap == null)
            generateMap = FindFirstObjectByType<GenerateMap>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (cameraMove == null && Camera.main != null)
            cameraMove = Camera.main.GetComponent<CameraMove>();

        if (player != null)
        {
            playerStartPosition = player.transform.position;
            healthController = player.GetComponent<HealthController>();
            playerStatsRecorder = player.GetComponent<PlayerStatsRecorder>(); // НОВОЕ
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        if (generateMap == null || player == null) return;

        // === ЗАПИСЬ СТАТИСТИКИ ПЕРЕД ПЕРЕЗАПУСКОМ ===
        if (playerStatsRecorder != null)
        {
            playerStatsRecorder.FinishRun();
            Debug.Log("Run finished via portal. Stats saved.");
        }

        // Восстанавливаем здоровье, если нужно
        if (restoreHealthOnPortal && healthController != null)
        {
            healthController.RestoreFullHealth();
        }

        // Удаляем врагов
        foreach (Transform child in generateMap.transform)
            if (child.CompareTag("Enemy")) Destroy(child.gameObject);

        // Перегенерация карты
        generateMap.GenerateFullMap();

        // Сброс игрока
        player.transform.position = playerStartPosition;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Сброс камеры
        if (cameraMove != null)
        {
            var camPos = Camera.main.transform.position;
            camPos.x = player.transform.position.x;
            if (cameraMove.limitLeft && camPos.x < cameraMove.leftLimit)
                camPos.x = cameraMove.leftLimit;
            Camera.main.transform.position = camPos;
            cameraMove.ResetSmoothDamp();
        }
    }
}