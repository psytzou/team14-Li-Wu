using UnityEngine;
using UnityEngine.UI;

// The five block Images and their colours are configured in Inspector.
public class SpeedActionView : MonoBehaviour
{
    [SerializeField] private Image[] blocks;
    [SerializeField] private Color availableColor;
    [SerializeField] private Color spentColor;

    public void SetRemaining(int remaining)
    {
        if (blocks == null) return;

        remaining = Mathf.Clamp(remaining, 0, blocks.Length);
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
                blocks[i].color = i < remaining ? availableColor : spentColor;
        }
    }
}
