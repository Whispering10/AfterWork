using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class VisibilityChangedEventArgs : EventArgs
{
    public Transform Target { get; }
    public bool IsVisible { get; }

    public VisibilityChangedEventArgs(Transform target, bool isVisible)
    {
        Target = target;
        IsVisible = isVisible;
    }
}

public class DetectorPlayerInCircle : MonoBehaviour
{
    [SerializeField] private float visionRange;
    [SerializeField] private CircleCollider2D col;

    public bool IsPlayerInSight {  get; private set; }
    public Transform PlayerTransform {  get; private set; }

    public event EventHandler<VisibilityChangedEventArgs> playerVisibilityChanged;

    private void Awake()
    {
        if (!TryGetComponent<CircleCollider2D>(out col))
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = visionRange;
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IsPlayerInSight = true;
            PlayerTransform = collision.transform;
            playerVisibilityChanged?.Invoke(this,
                new VisibilityChangedEventArgs(PlayerTransform, IsPlayerInSight));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IsPlayerInSight = false;
            PlayerTransform = collision.transform;
            playerVisibilityChanged?.Invoke(this,
                new VisibilityChangedEventArgs(PlayerTransform, IsPlayerInSight));
        }
    }
}
