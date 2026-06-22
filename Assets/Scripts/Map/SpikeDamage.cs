using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    [SerializeField] private float damage = 1f;   // сколько урона

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthController health = other.GetComponent<HealthController>();
            if (health != null)
            {
                health.TryConsume(damage);
                Debug.Log($"Ўипы нанесли {damage} урона");
            }
        }
    }
}