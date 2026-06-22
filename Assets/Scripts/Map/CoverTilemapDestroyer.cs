using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
public class CoverTilemapDestroyer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDestroyDistance = 5f;   // максимальная дистанция от игрока
    [SerializeField] private Transform player;                // ссылка на игрока (перетащите в инспекторе)

    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        // Убедимся, что коллайдер есть и он не триггер
        TilemapCollider2D collider = GetComponent<TilemapCollider2D>();
        if (collider == null)
        {
            Debug.LogError("TilemapCollider2D отсутствует! Добавьте его вручную.");
        }
        else
        {
            collider.isTrigger = false; // чтобы клик проходил
        }
    }

    private void OnMouseDown()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned!");
            return;
        }

        // Получаем позицию мыши в мировых координатах
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Проверка расстояния до игрока
        float distance = Vector2.Distance(player.position, mouseWorldPos);
        if (distance > maxDestroyDistance)
        {
            Debug.Log($"Слишком далеко! Расстояние: {distance} (макс. {maxDestroyDistance})");
            return;
        }

        // Преобразуем в клетку тайлмапа
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);

        if (tilemap.HasTile(cellPos))
        {
            // Удаляем тайл
            tilemap.SetTile(cellPos, null);
            Debug.Log($"Укрытие разрушено в клетке {cellPos}");
        }
        else
        {
            Debug.Log($"В клетке {cellPos} нет тайла (клик мимо)");
        }
    }
}