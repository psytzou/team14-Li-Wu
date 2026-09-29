using UnityEngine;
using UnityEngine.UIElements;

public class hp_modifier : MonoBehaviour
{
    public PlayerController PlayerControl;
    public UIDocument UIDoc;

    private Label healthLabel;
    private VisualElement healthFill;

    private void Start()
    {
        if (PlayerControl == null)
            PlayerControl = FindFirstObjectByType<PlayerController>();
        if (UIDoc == null)
            UIDoc = GetComponentInChildren<UIDocument>();

        if (PlayerControl == null || UIDoc == null)
        {
            Debug.LogWarning("Game HUD could not find PlayerController or UIDocument.");
            enabled = false;
            return;
        }

        VisualElement root = UIDoc.rootVisualElement;
        healthLabel = root.Q<Label>("healthlabel");
        healthFill = root.Q<VisualElement>("HP_fill");
    }

    private void Update()
    {
        if (PlayerControl == null) return;

        UpdateHealth();
    }

    private void UpdateHealth()
    {
        int current = Mathf.Max(0, PlayerControl.CurrentHealth);
        int maximum = Mathf.Max(0, PlayerControl.MaxHealth);
        float percentage = maximum > 0 ? current * 100f / maximum : 0f;

        if (healthLabel != null)
            healthLabel.text = $"{current}/{maximum}";
        if (healthFill != null)
            healthFill.style.width = Length.Percent(Mathf.Clamp(percentage, 0f, 100f));
    }

}
