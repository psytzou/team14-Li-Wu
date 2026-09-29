using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reusable visual-only health bar. The Image and text references are wired
// in the prefab Inspector; this component never creates UI or materials.
public class HealthBarView : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text valueLabel;

    public void SetHealth(int current, int maximum)
    {
        current = Mathf.Max(0, current);
        maximum = Mathf.Max(0, maximum);

        if (fillImage != null)
            fillImage.fillAmount = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f;

        if (valueLabel != null)
            valueLabel.text = $"{current}/{maximum}";
    }
}
