using UnityEngine;
using UnityEngine.SceneManagement;

public class KillerWall : MonoBehaviour
{
    [Header("Following Settings")]
    [SerializeField] private float desiredOffset = 5f;
    [SerializeField] private float followStrength = 2f;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Dark Zone Settings")]
    [SerializeField] private GameObject darkZonePrefab;
    [SerializeField] private float darkZoneWidth = 20f;

    [Header("Reset Settings")]
    [SerializeField] private Vector3 startPosition = new Vector3(-20f, 0f, 0f);

    private Transform player;
    private bool hasPlayer = false;
    private GameObject darkZone;

    private float baseMinSpeed;
    private float baseMaxSpeed;
    private float speedMultiplier = 1f;

    private bool isMovementEnabled = true;

    private void Awake()
    {
        baseMinSpeed = minSpeed;
        baseMaxSpeed = maxSpeed;
    }

    private void Start()
    {
        FindPlayer();
        CreateDarkZone();
        ResetToStartPosition();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            hasPlayer = true;
        }
        else
        {
            hasPlayer = false;
        }
    }

    private void CreateDarkZone()
    {
        if (darkZonePrefab == null) return;

        if (darkZone != null)
            Destroy(darkZone);

        darkZone = Instantiate(darkZonePrefab, transform);

        SpriteRenderer sr = darkZone.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            darkZone.transform.localScale = new Vector3(darkZoneWidth, sr.sprite.bounds.size.y * 10f, 1f);
        }

        UpdateDarkZonePosition();
    }

    private void ResetToStartPosition()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = startPosition;
        UpdateDarkZonePosition();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public void ResetWall(Transform playerTransform = null)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }


        if (playerTransform != null)
        {
            player = playerTransform;
            hasPlayer = true;
        }
        else
        {
            FindPlayer();
        }

        if (hasPlayer && player != null)
        {
            float targetX = player.position.x - desiredOffset;
            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = startPosition;
        }

        UpdateDarkZonePosition();
        isMovementEnabled = true;
        Debug.Log($"KillerWall reset. Position: {transform.position}");
    }

    public void EnableMovement() => isMovementEnabled = true;
    public void DisableMovement() => isMovementEnabled = false;

    private void Update()
    {
        if (!isMovementEnabled) return;

        // Применяем множитель к скоростям
        float currentMin = baseMinSpeed * speedMultiplier;
        float currentMax = baseMaxSpeed * speedMultiplier;

        if (!hasPlayer || player == null)
        {
            transform.Translate(Vector2.right * currentMin * Time.deltaTime);
            UpdateDarkZonePosition();
            return;
        }

        float targetX = player.position.x - desiredOffset;
        float error = targetX - transform.position.x;

        float desiredSpeed = Mathf.Clamp(error * followStrength, currentMin, currentMax);

        transform.Translate(Vector2.right * desiredSpeed * Time.deltaTime);
        UpdateDarkZonePosition();
    }

    private void UpdateDarkZonePosition()
    {
        if (darkZone == null) return;

        float halfWidth = darkZone.transform.localScale.x / 2f;
        darkZone.transform.position = new Vector3(
            transform.position.x - halfWidth,
            transform.position.y,
            transform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}