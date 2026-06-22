using UnityEngine;

public class AttackAbility : MonoBehaviour
{
    private CombatSystem combatSystem;
    private PlayerStatsRecorder statsRecorder; // <-- добавили

    private void Awake()
    {
        enabled = false;
        statsRecorder = GetComponent<PlayerStatsRecorder>(); // <-- получили ссылку
    }

    public void Init()
    {
        combatSystem = GetComponent<CombatSystem>();
        enabled = true;
    }

    public bool Attack(Weapon weapon, Vector2 direction)
    {
        // Запись статистики (каждый вызов атаки)
        if (statsRecorder != null) statsRecorder.OnAttack();

        return combatSystem.TryAttack(weapon, direction);
    }
}