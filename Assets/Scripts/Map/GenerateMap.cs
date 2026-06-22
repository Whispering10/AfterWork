using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateMap : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private RuleTile tile;

    [Header("Spike Settings")]
    [SerializeField] private Tilemap spikeTilemap;
    [SerializeField] private TileBase spikeTile;
    [SerializeField] private float spikeProbability = 0.3f; 

    [Header("Decoration Settings - Layer 1")]
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private RuleTile decorationTile;
    [SerializeField] private float decorationProbability = 0.25f;
    [SerializeField] private bool decorateUpperPlatforms = false;

    [Header("Decoration Settings - Layer 2")]
    [SerializeField] private Tilemap decorationTilemap2;
    [SerializeField] private RuleTile decorationTile2;
    [SerializeField] private float decorationProbability2 = 0.2f;
    [SerializeField] private bool decorateUpperPlatforms2 = false;

    [Header("Cover Settings (Stealth Elements)")]
    [SerializeField] private Tilemap coverTilemap;
    [SerializeField] private TileBase coverTile;
    [SerializeField] private float coverProbability = 0.2f;
    [SerializeField] private int coverMinHeight = 2;
    [SerializeField] private int coverMaxHeight = 3;
    [SerializeField] private bool coverOnUpperPlatforms = false;

    [Header("Portal Settings")]
    [SerializeField] private Tilemap portalTilemap;
    [SerializeField] private TileBase portalTile;
    [SerializeField] private int portalHeightOffset = 3;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Camera cam;

    [Header("KillerWall Settings")]
    [SerializeField] private KillerWall killerWall;

    [Header("Generation Settings")]
    [SerializeField] private int safePlatformLength = 10;
    [SerializeField] private int numberOfPlatforms = 100;
    [SerializeField] private int minPlatformLength = 1;
    [SerializeField] private int maxPlatformLength = 5;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private CMSEntity enemyModel;
    [SerializeField] private float enemySpawnChance = 40f; 

    private float trapDensityMult = 1f;
    private float shelterDensityMult = 1f;
    private float enemyDensityMult = 1f;
    private float locationSpeedMult = 1f;

    private static float staticTrapMult = 1f;
    private static float staticShelterMult = 1f;
    private static float staticEnemyMult = 1f;
    private static float staticSpeedMult = 1f;

    private float baseSpikeProbability;
    private float baseCoverProbability;
    private float baseEnemySpawnChance;

    private float currentSpikeProbability;
    private float currentCoverProbability;
    private float currentEnemySpawnChance;

    private List<Vector2Int> downFloor = new List<Vector2Int>();
    private List<Vector2Int> upperFloor = new List<Vector2Int>();

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private PlayerStatsRecorder statsRecorder;

    private float fullHeight;
    private float bottomMaxY;
    private float topMaxY;

    public Tilemap CoverTilemap => coverTilemap;
    public TileBase CoverTile => coverTile;

    struct PlayerParameters
    {
        public float g, m, jumpPower, jumpTime, jumpHeight, speed, dashLength;
    }
    private PlayerParameters parameters = new PlayerParameters();

    private void Awake()
    {
        Debug.Log("[GenerateMap] Awake вызван");

        // Восстанавливаем множители из статических полей
        trapDensityMult = staticTrapMult;
        shelterDensityMult = staticShelterMult;
        enemyDensityMult = staticEnemyMult;
        locationSpeedMult = staticSpeedMult;

        // Сохраняем базовые значения
        baseSpikeProbability = spikeProbability;
        baseCoverProbability = coverProbability;
        baseEnemySpawnChance = enemySpawnChance;

        UpdateCurrentValues();
        Debug.Log($"[GenerateMap] После Awake: trapMult={trapDensityMult:F2}, shelterMult={shelterDensityMult:F2}, enemyMult={enemyDensityMult:F2}, speedMult={locationSpeedMult:F2}");
    }

    public void Init(GameObject player)
    {
        this.player = player;

        transform.position = new Vector3(
            -cam.orthographicSize * Screen.width / Screen.height,
            -cam.orthographicSize - 1, 0
        );

        parameters.g = player.GetComponent<Rigidbody2D>().gravityScale * Physics2D.gravity.y;
        parameters.m = player.GetComponent<Rigidbody2D>().mass;
        parameters.jumpPower = player.GetComponent<JumpAbility>().Power;
        parameters.jumpTime = Mathf.Abs(parameters.jumpPower / (parameters.m * parameters.g));
        parameters.jumpHeight = ((parameters.jumpPower / parameters.m) * parameters.jumpTime)
                                + (parameters.g * (parameters.jumpTime * parameters.jumpTime)) / 2;
        parameters.speed = player.GetComponent<MoveAbility>().Speed;
        parameters.dashLength = player.GetComponent<DashAbility>().Length;

        fullHeight = cam.orthographicSize * 2f;
        bottomMaxY = 0.3f * fullHeight;
        topMaxY = 0.3f * fullHeight;
    }

    public void SetEnemyData(GameObject prefab, CMSEntity model, float chance)
    {
        enemyPrefab = prefab;
        enemyModel = model;
        enemySpawnChance = chance;
        baseEnemySpawnChance = chance;
        UpdateCurrentValues();
    }
    public void ApplyDefaultMultipliers()
    {
        Debug.Log("[GenerateMap] ApplyDefaultMultipliers вызван");
        trapDensityMult = 1f;
        shelterDensityMult = 1f;
        enemyDensityMult = 1f;
        locationSpeedMult = 1f;

        // Сохраняем в статику
        staticTrapMult = trapDensityMult;
        staticShelterMult = shelterDensityMult;
        staticEnemyMult = enemyDensityMult;
        staticSpeedMult = locationSpeedMult;

        UpdateCurrentValues();
    }

    public void ApplyAdjustments(float trapMult, float shelterMult, float enemyMult, float speedMult)
    {
        Debug.Log($"[GenerateMap] ApplyAdjustments: trap={trapMult:F2}, shelter={shelterMult:F2}, enemy={enemyMult:F2}, speed={speedMult:F2}");
        trapDensityMult = trapMult;
        shelterDensityMult = shelterMult;
        enemyDensityMult = enemyMult;
        locationSpeedMult = speedMult;

        // Сохраняем в статику
        staticTrapMult = trapDensityMult;
        staticShelterMult = shelterDensityMult;
        staticEnemyMult = enemyDensityMult;
        staticSpeedMult = locationSpeedMult;

        UpdateCurrentValues();
    }

    private void UpdateCurrentValues()
    {
        currentSpikeProbability = Mathf.Clamp01(baseSpikeProbability * trapDensityMult);
        currentCoverProbability = Mathf.Clamp01(baseCoverProbability * shelterDensityMult);
        currentEnemySpawnChance = Mathf.Clamp01(baseEnemySpawnChance * enemyDensityMult);

        if (killerWall != null)
            killerWall.SetSpeedMultiplier(locationSpeedMult);

        Debug.Log($"[GenerateMap] UpdateCurrentValues: spike={currentSpikeProbability:F4}, cover={currentCoverProbability:F4}, enemyChance={currentEnemySpawnChance:F4}");
    }

    public void GenerateFullMap()
    {
        Debug.Log($"[GenerateFullMap] Множители: traps={trapDensityMult:F2}, shelters={shelterDensityMult:F2}, enemies={enemyDensityMult:F2}, speed={locationSpeedMult:F2}");
        Debug.Log($"[GenerateFullMap] currentSpikeProb={currentSpikeProbability:F4}, currentCoverProb={currentCoverProbability:F4}, currentEnemyChance={currentEnemySpawnChance:F4}");

        ClearAllTilemaps();
        ClearEnemies();
        ResetLevelData();

        Vector3 camPos = cam ? cam.transform.position : Vector3.zero;
        Quaternion camRot = cam ? cam.transform.rotation : Quaternion.identity;

        Generate();
        AddSpikes();
        AddDecorations();
        AddDecorations2();
        AddCover();
        AddPortal();

        if (player != null && downFloor.Count > 0)
        {
            Vector2Int startPos = downFloor[0];
            player.transform.position = new Vector3(startPos.x + 0.5f, startPos.y + 1f, 0);
        }

        ResetDynamicObjects();

        statsRecorder = player?.GetComponent<PlayerStatsRecorder>();
        if (statsRecorder != null)
            statsRecorder.ResetForNewRun();
        else
            Debug.LogWarning("PlayerStatsRecorder не найден на игроке!");

        if (enemyPrefab != null && enemyModel != null)
        {
            AddEnemy(enemyPrefab, enemyModel, currentEnemySpawnChance);
        }
        else
        {
            Debug.LogWarning("Enemy prefab or model not assigned in GenerateMap!");
        }

        if (cam != null)
        {
            cam.transform.position = camPos;
            cam.transform.rotation = camRot;
            cam.enabled = true;
            cam.gameObject.SetActive(true);

            CameraMove cameraMove = cam.GetComponent<CameraMove>();
            if (cameraMove != null && player != null)
                cameraMove.Init(player.transform);
        }

        Debug.Log("=== Полная генерация уровня завершена ===");
    }

    private void ResetLevelData()
    {
        downFloor.Clear();
        downFloor.Add(new Vector2Int(0, 0));
        upperFloor.Clear();
        upperFloor.Add(new Vector2Int(0, 0));
    }

    private void ClearAllTilemaps()
    {
        tilemap?.ClearAllTiles();
        spikeTilemap?.ClearAllTiles();
        decorationTilemap?.ClearAllTiles();
        decorationTilemap2?.ClearAllTiles();
        coverTilemap?.ClearAllTiles();
        portalTilemap?.ClearAllTiles();
    }

    private void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
            if (enemy != null) Destroy(enemy);
        spawnedEnemies.Clear();
    }

    public void ResetDynamicObjects()
    {
        if (killerWall != null)
        {
            Transform currentPlayer = player != null ? player.transform :
                                    GameObject.FindGameObjectWithTag("Player")?.transform;
            killerWall.ResetWall(currentPlayer);
            Debug.Log("KillerWall успешно сброшен.");
        }
        else
        {
            Debug.LogWarning("KillerWall не назначен в GenerateMap!");
        }
    }

    public void AddEnemy(GameObject enemyPfb, CMSEntity enemyModel, float chance)
    {
        Debug.Log($"[Enemy] Начинаем создание врагов с вероятностью {chance:F4}");
        int count = 0;
        for (int i = 0; i < downFloor.Count - 1; i++)
        {
            if (downFloor[i][1] == downFloor[i + 1][1])
            {
                if (Random.Range(0, 100) <= chance * 100f)
                {
                    GameObject enemy = Factory.Create(enemyPfb, enemyModel);
                    enemy.transform.parent = gameObject.transform;
                    enemy.transform.localPosition = new Vector3(
                        Random.Range(downFloor[i][0], downFloor[i + 1][0]),
                        downFloor[i][1] + 3, 0
                    );

                    EnemyDeathRecorder deathRecorder = enemy.GetComponent<EnemyDeathRecorder>();
                    if (deathRecorder != null)
                        deathRecorder.SetStatsRecorder(statsRecorder);

                    spawnedEnemies.Add(enemy);
                    count++;
                }
            }
        }
        Debug.Log($"[Enemy] Создано {count} врагов");
    }

    public void Generate()
    {
        if (downFloor.Count == 0) downFloor.Add(new Vector2Int(0, 0));
        if (upperFloor.Count == 0) upperFloor.Add(new Vector2Int(0, 0));

        AddSafePlatform(downFloor);
        for (int i = 0; i < numberOfPlatforms; i++)
            AddRandomPlatform(downFloor, false);

        BuildPlatforms(downFloor);

        AddSafePlatform(upperFloor);
        for (int i = 0; i < numberOfPlatforms; i++)
            AddRandomPlatform(upperFloor, true);

        BuildPlatforms(upperFloor, true);
    }

    public void AddSpikes()
    {
        if (spikeTilemap == null || spikeTile == null) return;
        spikeTilemap.ClearAllTiles();
        Debug.Log($"[Spikes] Начинаем добавление шипов с вероятностью {currentSpikeProbability:F4}");
        int count = 0;
        for (int i = 0; i < downFloor.Count - 1; i++)
        {
            if (downFloor[i].y != downFloor[i + 1].y) continue;
            int startX = downFloor[i].x;
            int endX = downFloor[i + 1].x;
            int surfaceY = downFloor[i].y + 1;
            for (int x = startX; x < endX; x++)
            {
                if (Random.value <= currentSpikeProbability)
                {
                    Vector3Int tilePos = new Vector3Int(x, surfaceY, 0);
                    spikeTilemap.SetTile(tilePos, spikeTile);
                    count++;
                }
            }
        }
        Debug.Log($"[Spikes] Установлено {count} шипов");
    }

    public void AddDecorations()
    {
        if (decorationTilemap == null || decorationTile == null) return;
        decorationTilemap.ClearAllTiles();
        DecoratePlatformList(downFloor, false, 1, decorationTilemap, decorationTile, decorationProbability);
        if (decorateUpperPlatforms)
            DecoratePlatformList(upperFloor, true, -1, decorationTilemap, decorationTile, decorationProbability);
    }

    public void AddDecorations2()
    {
        if (decorationTilemap2 == null || decorationTile2 == null) return;
        decorationTilemap2.ClearAllTiles();
        DecoratePlatformList(downFloor, false, 1, decorationTilemap2, decorationTile2, decorationProbability2);
        if (decorateUpperPlatforms2)
            DecoratePlatformList(upperFloor, true, -1, decorationTilemap2, decorationTile2, decorationProbability2);
    }

    public void AddCover()
    {
        if (coverTilemap == null || coverTile == null) return;
        coverTilemap.ClearAllTiles();
        Debug.Log($"[Cover] Начинаем добавление укрытий с вероятностью {currentCoverProbability:F4}");
        int count = 0;
        GenerateCoverOnPlatformList(downFloor, false, ref count);
        if (coverOnUpperPlatforms)
            GenerateCoverOnPlatformList(upperFloor, true, ref count);
        Debug.Log($"[Cover] Установлено {count} укрытий");
    }

    private void GenerateCoverOnPlatformList(List<Vector2Int> platformPoints, bool isUpper, ref int coverCount)
    {
        for (int i = 0; i < platformPoints.Count - 1; i++)
        {
            Vector2Int start = platformPoints[i];
            Vector2Int end = platformPoints[i + 1];
            if (start.y != end.y) continue;

            int xStart = start.x;
            int xEnd = end.x;
            int platformY = start.y;

            for (int x = xStart; x < xEnd; x++)
            {
                if (Random.value <= currentCoverProbability)
                {
                    int height = Random.Range(coverMinHeight, coverMaxHeight + 1);
                    for (int h = 1; h <= height; h++)
                    {
                        int tileY = isUpper
                            ? (int)(cam.orthographicSize * 2) - (platformY + h)
                            : platformY + h;

                        Vector3Int tilePos = new Vector3Int(x, tileY, 0);
                        coverTilemap.SetTile(tilePos, coverTile);
                        coverCount++;
                    }
                }
            }
        }
    }

    private void DecoratePlatformList(List<Vector2Int> platformPoints, bool isUpper, int yOffset,
                                      Tilemap targetTilemap, RuleTile targetTile, float probability)
    {
        for (int i = 0; i < platformPoints.Count - 1; i++)
        {
            Vector2Int start = platformPoints[i];
            Vector2Int end = platformPoints[i + 1];
            if (start.y != end.y) continue;

            int xStart = start.x;
            int xEnd = end.x;
            int surfaceY = start.y;
            int decorationY = surfaceY + yOffset;

            for (int x = xStart; x < xEnd; x++)
            {
                if (Random.value <= probability)
                {
                    Vector3Int tilePos = new Vector3Int(x, decorationY, 0);
                    targetTilemap.SetTile(tilePos, targetTile);
                }
            }
        }
    }

    public void AddPortal()
    {
        if (portalTilemap == null || portalTile == null) return;
        portalTilemap.ClearAllTiles();
        if (downFloor.Count < 2) return;

        Vector2Int startPoint = downFloor[downFloor.Count - 2];
        Vector2Int endPoint = downFloor[downFloor.Count - 1];
        if (startPoint.y != endPoint.y) return;

        int centerX = (startPoint.x + endPoint.x) / 2;
        int portalY = startPoint.y + portalHeightOffset;

        Vector3Int portalPos = new Vector3Int(centerX, portalY, 0);
        portalTilemap.SetTile(portalPos, portalTile);
    }

    private void AddRandomPlatform(List<Vector2Int> points, bool isUpper)
    {
        int last = points.Count - 1;
        Vector2Int point = GetRandomPossibilityPoint(points[last], isUpper);
        point.x = points[last].x;
        int length = Random.Range(minPlatformLength, maxPlatformLength + 1);
        points.Add(new Vector2Int(point.x, point.y));
        points.Add(new Vector2Int(point.x + length, point.y));
    }

    private Vector2Int GetRandomPossibilityPoint(Vector2Int previousPoint, bool isUpper)
    {
        float yPossibility = parameters.jumpHeight + parameters.dashLength;
        float dyMin, dyMax;

        if (!isUpper)
        {
            float minY = 0f, maxY = bottomMaxY;
            dyMin = minY - previousPoint.y;
            dyMax = maxY - previousPoint.y;
        }
        else
        {
            float minY = 0f, maxY = topMaxY;
            dyMin = minY - previousPoint.y;
            dyMax = maxY - previousPoint.y;
        }

        dyMin = Mathf.Max(dyMin, -yPossibility);
        dyMax = Mathf.Min(dyMax, yPossibility);
        if (dyMax < dyMin) dyMax = dyMin;

        int dy = (int)Random.Range(dyMin, dyMax + 1);
        return new Vector2Int(previousPoint.x, previousPoint.y + dy);
    }

    private void AddSafePlatform(List<Vector2Int> points)
    {
        int last = points.Count - 1;
        Vector2Int nextPoint = new Vector2Int(points[last].x + safePlatformLength, points[last].y);
        points.Add(nextPoint);
    }

    private void BuildPlatforms(List<Vector2Int> points, bool reverse = false)
    {
        Vector2Int previousPoint = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            if (previousPoint.y != points[i].y)
            {
                previousPoint = points[i];
                continue;
            }

            int y = points[i].y;
            for (int x = previousPoint.x; x < points[i].x; x++)
            {
                for (int y1 = y; y1 >= 0; y1--)
                {
                    Vector3Int position = new Vector3Int(
                        x,
                        reverse ? (int)(cam.orthographicSize * 2) - y1 : y1,
                        0
                    );
                    tilemap.SetTile(position, tile);
                }
            }
            previousPoint = points[i];
        }
    }

    public List<Vector2Int> GetDownFloor() => downFloor;
}