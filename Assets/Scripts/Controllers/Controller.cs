using UnityEngine;

public abstract class Controller : MonoBehaviour 
{
    protected float value;

    public float Value { get { return value; } }
    public abstract float MaxValue { get; }

    private void Awake()
    {
        enabled = false;
    }

    public abstract bool TryConsume(float amount);

    public void OnTryConsume(float amount)
    {
        TryConsume(amount);
    }
}
