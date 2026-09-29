using UnityEngine;

public class rollCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static int Roll(int sides)
    {
        // Unity's integer Random.Range excludes the maximum value.
        int rollnum = Random.Range(1, sides + 1);
        Debug.Log("Roll check output: " + rollnum);
        return rollnum;
    }
}
