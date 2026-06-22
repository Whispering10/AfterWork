using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Tilemap))]
public class BreakableTilemap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDestroyDistance = 4f;
    [SerializeField] private bool isBreakable = true;

    private Tilemap tilemap;
    private Transform player;
    private Camera mainCam;
    private InputAction clickAction;
    private PlayerStatsRecorder statsRecorder; // <-- äîáàâèëè

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            enabled = false;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            statsRecorder = playerObj.GetComponent<PlayerStatsRecorder>(); // <-- ïîëó÷èëè ññûëêó
        }
        mainCam = Camera.main;

        clickAction = new InputAction("Break", binding: "<Mouse>/leftButton");
        clickAction.performed += ctx => TryDestroyAtMouse();
        clickAction.Enable();
    }

    private void OnDestroy()
    {
        if (clickAction != null)
        {
            clickAction.performed -= ctx => TryDestroyAtMouse();
            clickAction.Disable();
        }
    }

    private void TryDestroyAtMouse()
    {
        if (!isBreakable || player == null || mainCam == null || tilemap == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        worldPos.z = 0f;

        if (Vector2.Distance(player.position, worldPos) > maxDestroyDistance) return;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f);
        if (hit.collider != null && hit.collider.GetComponent<Tilemap>() == tilemap)
        {
            Vector3Int cellPos = tilemap.WorldToCell(hit.point);
            DestroyTileAtCell(cellPos);
        }
    }

    public bool DestroyTileAtWorldPos(Vector3 worldPos)
    {
        if (!isBreakable || tilemap == null) return false;
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        return DestroyTileAtCell(cellPos);
    }

    private bool DestroyTileAtCell(Vector3Int cellPos)
    {
        if (tilemap.HasTile(cellPos))
        {
            tilemap.SetTile(cellPos, null);
            Debug.Log($"[BreakableTilemap] Destroyed tile at {cellPos} on {tilemap.name}");

            // === ÇÀÏÈÑÜ ÑÒÀÒÈÑÒÈÊÈ ===
            if (statsRecorder != null) statsRecorder.OnObjectDestroyed();

            return true;
        }
        return false;
    }

    public void SetBreakable(bool breakable) => isBreakable = breakable;
}