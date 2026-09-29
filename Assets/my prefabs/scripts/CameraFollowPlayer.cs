using TurnBasedBoard;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private CellScaleController board;
    [SerializeField] private bool clampToBoard;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.50f;
    [SerializeField, Min(0.1f)] private float maximumFollowSpeed = 12f;

    private Camera attachedCamera;
    private Vector3 followVelocity;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            target = player.transform;
            followVelocity = Vector3.zero;
        }

        Vector3 desiredPosition = target.position + offset;
        if (clampToBoard && board != null && attachedCamera != null && attachedCamera.orthographic)
        {
            desiredPosition = ClampToBoard(desiredPosition);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            Mathf.Max(0.01f, smoothTime),
            Mathf.Max(0.1f, maximumFollowSpeed),
            Time.deltaTime);
    }

    private Vector3 ClampToBoard(Vector3 desiredPosition)
    {
        Bounds bounds = board.GetWorldBounds();
        float halfHeight = attachedCamera.orthographicSize;
        float halfWidth = halfHeight * attachedCamera.aspect;

        desiredPosition.x = ClampAxis(desiredPosition.x, bounds.min.x, bounds.max.x, halfWidth, bounds.center.x);
        desiredPosition.y = ClampAxis(desiredPosition.y, bounds.min.y, bounds.max.y, halfHeight, bounds.center.y);
        return desiredPosition;
    }

    private static float ClampAxis(float value, float minimum, float maximum, float viewHalfSize, float center)
    {
        if (maximum - minimum <= viewHalfSize * 2f)
            return center;

        return Mathf.Clamp(value, minimum + viewHalfSize, maximum - viewHalfSize);
    }
}
