using UnityEngine;

// Connects PlayerController state to prefab-based action indicators. All
// visual references are assigned through the Inspector.
public class ActionHUDPresenter : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private AttackActionView attackIndicator;
    [SerializeField] private SpeedActionView speedIndicator;

    private void Update()
    {
        if (player == null) return;

        if (attackIndicator != null)
            attackIndicator.SetAvailable(player.CanAttack);

        if (speedIndicator != null)
            speedIndicator.SetRemaining(player.RemainingSpeed);
    }
}
