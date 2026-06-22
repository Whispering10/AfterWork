using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class StealthCoverHandler : MonoBehaviour
{
    [Header("Active Stealth (Shift)")]
    [SerializeField] private bool holdToStealth = false;
    [SerializeField] private float stealthSpeedMultiplier = 0.5f;

    [Header("Cover Stealth")]
    [SerializeField] private float coverFadeAlpha = 0.4f;        // конечная прозрачность
    [SerializeField] private float fadeDuration = 0.5f;          
    [SerializeField] private int coverRadius = 2;

    private Tilemap coverTilemap;
    private bool isActiveStealth = false;
    private bool isInCover = false;
    private bool isInitialized = false;

    private Dictionary<Vector3Int, Color> originalColors = new Dictionary<Vector3Int, Color>();
    private List<Vector3Int> affectedCells = new List<Vector3Int>();

    private Coroutine currentFadeCoroutine = null; 

    public bool IsHidden => isActiveStealth || isInCover;
    public bool IsActiveStealth => isActiveStealth;
    public bool IsInCover => isInCover;
    public float StealthSpeedMultiplier => stealthSpeedMultiplier;
    public float DetectionRadiusModifier => IsHidden ? 0.5f : 1f;

    private void Start()
    {
        if (!isInitialized) FindCoverTilemap();
    }

    public void Init() => FindCoverTilemap();

    public void FindCoverTilemap()
    {
        GenerateMap map = FindFirstObjectByType<GenerateMap>();
        if (map != null)
        {
            coverTilemap = map.CoverTilemap;
            isInitialized = coverTilemap != null;
            if (isInitialized)
                Debug.Log("[Stealth] Initialized: " + coverTilemap.name);
            else
                Debug.LogError("CoverTilemap is null in GenerateMap");
        }
        else
            Debug.LogError("GenerateMap not found!");
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (Keyboard.current != null)
        {
            if (holdToStealth)
            {
                bool newState = Keyboard.current.leftShiftKey.isPressed;
                if (newState != isActiveStealth)
                    isActiveStealth = newState;
            }
            else
            {
                if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                    isActiveStealth = !isActiveStealth;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (coverTilemap != null && other.GetComponent<Tilemap>() == coverTilemap)
        {
            if (!isInCover)
            {
                isInCover = true;
                if (currentFadeCoroutine != null)
                    StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = StartCoroutine(FadeCover(true));
                Debug.Log("[Stealth] Entered cover - fading transparent");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (coverTilemap != null && other.GetComponent<Tilemap>() == coverTilemap)
        {
            isInCover = false;
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = StartCoroutine(FadeCover(false));
            Debug.Log("[Stealth] Exited cover - fading back");
        }
    }

    // Плавное изменение прозрачности
    private IEnumerator FadeCover(bool fadeToTransparent)
    {
        if (fadeToTransparent)
        {
            RestoreCoverOpacityImmediate();

            affectedCells.Clear();
            originalColors.Clear();

            Vector3Int center = coverTilemap.WorldToCell(transform.position);
            for (int x = -coverRadius; x <= coverRadius; x++)
            {
                for (int y = -coverRadius; y <= coverRadius; y++)
                {
                    Vector3Int cell = center + new Vector3Int(x, y, 0);
                    if (coverTilemap.GetTile(cell) != null)
                    {
                        if (!originalColors.ContainsKey(cell))
                        {
                            originalColors[cell] = coverTilemap.GetColor(cell);
                        }
                        affectedCells.Add(cell);
                    }
                }
            }

            if (affectedCells.Count == 0)
            {
                currentFadeCoroutine = null;
                yield break;
            }
        }
        else
        {
            if (affectedCells.Count == 0)
            {
                currentFadeCoroutine = null;
                yield break;
            }
        }

        float startAlpha = fadeToTransparent ? 1f : coverFadeAlpha;
        float targetAlpha = fadeToTransparent ? coverFadeAlpha : 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration; 
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            foreach (var cell in affectedCells)
            {
                if (coverTilemap.HasTile(cell))
                {
                    Color col = coverTilemap.GetColor(cell);
                    col.a = currentAlpha;
                    coverTilemap.SetColor(cell, col);
                }
            }
            yield return null;
        }

        foreach (var cell in affectedCells)
        {
            if (coverTilemap.HasTile(cell))
            {
                Color col = coverTilemap.GetColor(cell);
                col.a = targetAlpha;
                coverTilemap.SetColor(cell, col);
            }
        }

        if (!fadeToTransparent)
        {
            RestoreCoverOpacityImmediate();
            affectedCells.Clear();
            originalColors.Clear();
        }

        currentFadeCoroutine = null;
    }

    private void RestoreCoverOpacityImmediate()
    {
        foreach (var cell in affectedCells)
        {
            if (originalColors.ContainsKey(cell))
                coverTilemap.SetColor(cell, originalColors[cell]);
        }
        affectedCells.Clear();
     
    }

}