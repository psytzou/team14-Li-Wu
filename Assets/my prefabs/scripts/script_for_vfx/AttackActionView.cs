using UnityEngine;
using UnityEngine.UI;

// Visual references and colours are configured on the prefab in Inspector.
public class AttackActionView : MonoBehaviour
{
    [SerializeField] private Image indicator;
    [SerializeField] private Color availableFillColor;
    [SerializeField] private Color spentFillColor;

    public void SetAvailable(bool available)
    {
        if (indicator != null)
            indicator.color = available ? availableFillColor : spentFillColor;
    }
}
