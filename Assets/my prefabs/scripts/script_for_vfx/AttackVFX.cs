using UnityEngine;

// Attack flash: spawns attackPrefab stretched from attacker to target, shows
// it for visibleFrames frames, then destroys it. Played for every attack
// roll, hit or miss (attack.Hit), plus an enemy attack whose locked cell the
// player already left (EnemyBehavior).
// Put this on any one GameObject in the gameplay scene and drag your prefab
// (e.g. hitweapon) into attackPrefab. The prefab's mesh is stretched along
// its local X so its ends land exactly on attacker and target, whatever the
// mesh's own size; Y/Z keep the prefab's own scale as the thickness.
public class AttackVFX : MonoBehaviour
{
    [Tooltip("Prefab to flash between attacker and target (e.g. hitweapon).")]
    public GameObject attackPrefab;
    [Tooltip("How many rendered frames the flash stays on screen before it disappears.")]
    public int visibleFrames = 1;
    [Tooltip("Added to z so the flash draws in front of the board and characters (camera looks along +z).")]
    public float zOffset = -0.2f;

    private static AttackVFX instance;

    void Awake()
    {
        instance = this;
    }

    public static void Play(Vector3 from, Vector3 to)
    {
        if (instance == null || instance.attackPrefab == null) return;
        instance.Spawn(from, to);
    }

    private void Spawn(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        if (distance < 0.0001f) return;

        // Mesh extent along local X, in the prefab's unscaled units. Falls
        // back to a 1-unit, centred mesh if the prefab has no MeshFilter.
        MeshFilter meshFilter = attackPrefab.GetComponent<MeshFilter>();
        Bounds bounds = meshFilter != null && meshFilter.sharedMesh != null
            ? meshFilter.sharedMesh.bounds
            : new Bounds(Vector3.zero, Vector3.one);
        float meshLength = bounds.size.x > 0.0001f ? bounds.size.x : 1f;

        Vector3 prefabScale = attackPrefab.transform.localScale;
        Vector3 scale = new Vector3(distance / meshLength, prefabScale.y, prefabScale.z);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        // Place the mesh's centre (not the pivot) on the midpoint, so an
        // off-centre pivot doesn't shift the flash off the attack line.
        Vector3 midpoint = (from + to) / 2f + Vector3.forward * zOffset;
        Vector3 position = midpoint - rotation * Vector3.Scale(bounds.center, scale);

        GameObject flash = Instantiate(attackPrefab, position, rotation);
        flash.transform.localScale = scale;
        // The prefab may be saved inactive; the flash itself must be visible.
        flash.SetActive(true);
        // The flash removes itself, so it's still cleaned up even if this
        // holder object is destroyed first (e.g. it sits on the player).
        flash.AddComponent<BlinkOnce>().framesLeft = Mathf.Max(1, visibleFrames);
    }

    // Added at runtime to each spawned flash. Its first Update runs on the
    // frame after spawning, and Destroy removes the object before that frame
    // renders -- so the flash is drawn for exactly framesLeft frames.
    private class BlinkOnce : MonoBehaviour
    {
        public int framesLeft;

        void Update()
        {
            framesLeft--;
            if (framesLeft <= 0)
                Destroy(gameObject);
        }
    }
}
