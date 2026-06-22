using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private bool useSmoothDamp = true;

    [Header("Boundaries")]
    [SerializeField] public bool limitLeft = false;   // теперь публичные для доступа из портала
    [SerializeField] public float leftLimit = 0f;

    private Transform player;
    private Vector3 velocityRef = Vector3.zero;
    private bool isInitialized = false;

    private void Awake()
    {
        enabled = false;
    }

    public void Init(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogError("CameraMove: playerTransform is null!");
            return;
        }
        player = playerTransform;
        isInitialized = true;
        enabled = true;

        Vector3 startPos = transform.position;
        startPos.x = player.position.x;
        if (limitLeft && startPos.x < leftLimit) startPos.x = leftLimit;
        transform.position = startPos;
        Debug.Log("CameraMove initialized");
    }

    public void ResetSmoothDamp()
    {
        velocityRef = Vector3.zero;
    }

    private void Update()
    {
        if (!isInitialized || player == null) return;

        float targetX = player.position.x;
        if (limitLeft && targetX < leftLimit) targetX = leftLimit;

        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);

        if (useSmoothDamp)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocityRef, smoothTime);
        }
        else
        {
            float step = followSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        }
    }
}