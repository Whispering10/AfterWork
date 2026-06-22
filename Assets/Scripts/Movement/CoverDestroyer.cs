using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class CoverDestroyer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDestroyDistance = 5f;
    [SerializeField] private LayerMask coverLayerMask;

    private Tilemap coverTilemap;
    private Camera mainCamera;
    private InputAction destroyAction;

    private void Awake()
    {
        destroyAction = new InputAction("Destroy", binding: "<Mouse>/leftButton");
        destroyAction.performed += ctx => TryDestroyCover();
        destroyAction.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        GenerateMap map = FindFirstObjectByType<GenerateMap>();
        if (map != null)
            coverTilemap = map.CoverTilemap;
        else
            Debug.LogError("GenerateMap not found!");
    }

    private void OnDestroy()
    {
        destroyAction.Disable();
        destroyAction.performed -= ctx => TryDestroyCover();
    }

    private void TryDestroyCover()
    {
        if (coverTilemap == null || mainCamera == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        worldPos.z = 0f;

        if (Vector2.Distance(transform.position, worldPos) > maxDestroyDistance)
        {
            Debug.Log("Too far");
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, coverLayerMask);
        if (hit.collider != null)
        {
            Tilemap hitTilemap = hit.collider.GetComponent<Tilemap>();
            if (hitTilemap == coverTilemap)
            {
                Vector3Int cellPos = coverTilemap.WorldToCell(hit.point);
                if (coverTilemap.HasTile(cellPos))
                {
                    coverTilemap.SetTile(cellPos, null);
                    Debug.Log($"Destroyed {cellPos}");
                }
            }
        }
    }
}