using UnityEngine;
using UnityEngine.UI;

// All visual references and the player are assigned through the Inspector.
public class TurnSwitchButtonView : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Button button;

    private void Update()
    {
        if (button != null)
            button.interactable = player != null && !player.IsBreathing;
    }
}
