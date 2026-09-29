using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class chamoving : MonoBehaviour
{
    [Header("Movement")]
    public int speed;

    private const float BumpOutDuration = 0.06f;
    private const float BumpBackDuration = 0.06f;

    private Collider selfCollider;
    private bool isBumping;

    void Start()
    {
        selfCollider = GetComponent<Collider>();
    }

    // Update is called once per keydown, and use wasd to move 1 distance along x,y axis
    void Update()
    {
        if (isBumping || Keyboard.current == null) return;

        Vector3 direction = Vector3.zero;
        if (Keyboard.current.wKey.wasPressedThisFrame) direction = Vector3.up;
        else if (Keyboard.current.sKey.wasPressedThisFrame) direction = Vector3.down;
        else if (Keyboard.current.aKey.wasPressedThisFrame) direction = Vector3.left;
        else if (Keyboard.current.dKey.wasPressedThisFrame) direction = Vector3.right;

        if (direction == Vector3.zero) return;

        if (speed <= 0)
        {
            Debug.Log("Speed is 0");
            return;
        }

        Vector3 destination = transform.position + direction * GameSetting.gridSize;

        if (MoveChecker.IsValidMove(destination, selfCollider))
        {
            transform.position = destination;
            speed--;
        }
        else
        {
            // Invalid move: bump halfway towards it, then back, to show the
            // move was rejected. speed is only spent on a move that actually
            // succeeds, never on a rejected one.
            StartCoroutine(BumpRoutine(destination));
        }
    }

    private IEnumerator BumpRoutine(Vector3 rejectedDestination)
    {
        isBumping = true;

        Vector3 origin = transform.position;
        Vector3 halfway = Vector3.Lerp(origin, rejectedDestination, 0.5f);

        yield return MoveOverTime(origin, halfway, BumpOutDuration);
        yield return MoveOverTime(halfway, origin, BumpBackDuration);

        isBumping = false;
    }

    private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.position = to;
    }
}
