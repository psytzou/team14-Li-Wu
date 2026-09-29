using TMPro;
using UnityEngine;

public sealed class KillCounterView : MonoBehaviour
{
    [SerializeField] private TMP_Text valueLabel;

    public void SetKills(int kills)
    {
        if (valueLabel != null)
            valueLabel.text = Mathf.Max(0, kills).ToString();
    }
}
