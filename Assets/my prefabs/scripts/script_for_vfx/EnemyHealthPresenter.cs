using UnityEngine;

// Connects one enemy's health component to a HealthBarView assigned in the
// Inspector. The visual hierarchy lives in the scene/prefab, not in code.
public class EnemyHealthPresenter : MonoBehaviour
{
    [SerializeField] private chatemplate target;
    [SerializeField] private HealthBarView healthBar;

    private int maximumHealth;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<chatemplate>();
    }

    private void LateUpdate()
    {
        if (target == null)
            target = GetComponent<chatemplate>();
        if (target == null || healthBar == null) return;

        // EnemyTMP assigns its base stats in Start, so capture the maximum
        // on the first frame where its HP is initialized.
        if (maximumHealth <= 0 && target.hp > 0)
            maximumHealth = target.hp;

        healthBar.SetHealth(target.hp, maximumHealth);
    }
}
