using TurnBasedBoard;
using System;
using System.Collections.Generic;
using UnityEngine;

// Enemy AI: unlike the player (grid-stepped), this moves continuously
// towards the player. Once the player is within hit_range, it waits
// AttackDelay seconds and then attacks the position the player was standing
// at when the attack was triggered -- not wherever the player currently is
// -- so moving away during that window dodges the hit. attack_cd gates how
// often a new attack can be triggered.
// Deliberately no [RequireComponent(typeof(chatemplate))] here: that would
// force a bare chatemplate (hp = 0, immediately self-destructs) to exist the
// moment this is added, and then block Awake from swapping it out for the
// real EnemyTMP (Unity won't let you destroy a component another still
// requires).
public class EnemyBehavior : MonoBehaviour
{
    private static readonly HashSet<EnemyBehavior> AliveEnemies = new HashSet<EnemyBehavior>();

    public static event Action<EnemyBehavior, chatemplate> EnemyDefeated;
    public static int AliveCount => AliveEnemies.Count;

    public float enemyspeed = 2f;
    public float attack_cd = 1.5f;

    [Tooltip("Drag the board's CellScaleController here (e.g. Board_15x15) to scale this enemy to one grid cell. Leave empty to auto-find it in the scene.")]
    public CellScaleController board;

    private const float AttackDelay = 0.1f;

    private chatemplate self;
    private Transform player;
    private chatemplate playerCharacter;

    private bool attackQueued;
    private float attackReadyTime;
    private Vector3 queuedTargetPosition;
    private float nextAttackAllowedTime;
    private bool defeatReported;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        AliveEnemies.Clear();
        EnemyDefeated = null;
    }

    void Awake()
    {
        self = GetComponent<chatemplate>();
        if (self == null || self.GetType() != typeof(EnemyTMP))
        {
            if (self != null)
                DestroyImmediate(self);

            self = gameObject.AddComponent<EnemyTMP>();
        }

        self.Died += HandleDeath;
    }

    private void OnEnable()
    {
        AliveEnemies.Add(this);
    }

    private void OnDisable()
    {
        AliveEnemies.Remove(this);
    }

    private void OnDestroy()
    {
        AliveEnemies.Remove(this);
        if (self != null)
            self.Died -= HandleDeath;
    }

    private void HandleDeath(chatemplate defeated, chatemplate killer)
    {
        if (defeatReported) return;
        defeatReported = true;
        EnemyDefeated?.Invoke(this, killer);
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("EnemyBehavior found no GameObject tagged 'Player'.");
            return;
        }

        player = playerObject.transform;
        playerCharacter = playerObject.GetComponent<chatemplate>();

        CellScaleController scaler = board != null ? board : CellScaleController.Instance;
        if (scaler != null)
            scaler.AlignToCell(transform);
        else
            Debug.LogWarning($"EnemyBehavior on {name}: no CellScaleController found in the scene (and 'board' isn't assigned), so its scale was left untouched.");
    }

    void Update()
    {
        if (player == null || playerCharacter == null) return;

        // Stop at hit_range instead of walking onto/through the player --
        // MoveTowards alone has no stopping distance, and a collider by
        // itself doesn't block a script that's setting transform.position
        // directly (no physics/Rigidbody involved here).
        Vector3 toPlayer = player.position - transform.position;
        float stopDistance = self.hit_range * GameSetting.gridSize;
        float currentDistance = toPlayer.magnitude;

        if (currentDistance > stopDistance)
        {
            float moveDistance = Mathf.Min(enemyspeed * Time.deltaTime, currentDistance - stopDistance);
            transform.position += toPlayer.normalized * moveDistance;
        }

        int distanceToPlayer = RangeCheck.measureGrid(transform.position, player.position);

        if (!attackQueued && distanceToPlayer <= self.hit_range && Time.time >= nextAttackAllowedTime)
        {
            attackQueued = true;
            queuedTargetPosition = player.position;
            attackReadyTime = Time.time + AttackDelay;
            nextAttackAllowedTime = Time.time + attack_cd;
        }

        if (attackQueued && Time.time >= attackReadyTime)
        {
            attackQueued = false;

            // Resolve against where the player was when this attack was
            // triggered, not their current position.
            if (RangeCheck.measureGrid(player.position, queuedTargetPosition) <= self.hit_range)
            {
                bool isHit = attack.Hit(self, playerCharacter);
                Debug.Log(isHit
                    ? $"{self.character_name} hit the player."
                    : $"{self.character_name} missed the player.");
            }
            else
            {
                // The enemy still performed an attack, but the player left
                // the locked position before it resolved.
                AttackVFX.Play(transform.position, queuedTargetPosition);
                Debug.Log($"{self.character_name}'s attack missed -- player moved away.");
            }
        }
    }
}
