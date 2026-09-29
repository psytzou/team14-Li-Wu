using System.Collections;
using TurnBasedBoard;
using UnityEngine;
using UnityEngine.InputSystem;

// Single activation point for the built hero. SelectedCharacter owns the
// character-specific part (which chatemplate subclass to attach, and
// building the live CharacterInstance from it). This controller owns the
// generic part every hero needs regardless of which character was picked:
// making sure chamoving/Playerattack exist, and toggling them together so
// enabling/disabling this one component turns all player actions on or off.
// Deliberately no [RequireComponent(typeof(chatemplate))] here: that would
// force a bare chatemplate (hp = 0) to exist the moment this is added, and
// then block SelectedCharacter.Register from swapping it out for the real
// subclass (Unity won't let you destroy a component another still requires).
public class PlayerController : MonoBehaviour
{
    [Tooltip("Only used if Playerattack has to be added fresh (no AttackHover wired yet).")]
    public GameObject attackHover;

    [Tooltip("Drag the board's CellScaleController here (e.g. Board_15x15) to scale the hero to one grid cell. Leave empty to auto-find it in the scene.")]
    public CellScaleController board;

    [Tooltip("Set true to start a 1s breathe: waits, then refreshes canAttack/speed (not hp, not ability state). Resets itself to false when done.")]
    public bool breathe;

    private const float BreatheDuration = 1f;
    private bool breathing;

    private chamoving movement;
    private Playerattack playerAttack;

    private CharacterInstance instance;

    public int CurrentHealth => instance != null ? instance.Hp : 0;
    public int MaxHealth => instance != null ? instance.HpMax : 0;
    public int RemainingSpeed => movement != null ? movement.speed : 0;
    public int MaxSpeed => instance != null ? instance.SpeedMax : 0;
    public bool CanAttack => playerAttack != null && playerAttack.canAttack;
    public bool IsBreathing => breathing || breathe;

    void Awake()
    {
        SelectedCharacter.Register(gameObject);

        movement = GetComponent<chamoving>();
        if (movement == null)
            movement = gameObject.AddComponent<chamoving>();

        playerAttack = GetComponent<Playerattack>();
        if (playerAttack == null)
        {
            playerAttack = gameObject.AddComponent<Playerattack>();
            if (attackHover != null)
                playerAttack.AttackHover = attackHover;
        }
    }

    void Start()
    {
        // Deferred to Start (not Awake) so GridMap.Instance is guaranteed to
        // already be set, regardless of Awake order between the two.
        if (GridMap.Instance != null)
            transform.position = GridMap.Instance.CenterWorldPosition;

        CellScaleController scaler = board != null ? board : CellScaleController.Instance;
        if (scaler != null)
            scaler.AlignToCell(transform);
        else
            Debug.LogWarning("PlayerController: no CellScaleController found in the scene (and 'board' isn't assigned), so the hero's scale was left untouched.");
    }

    void OnEnable()
    {
        if (movement != null) movement.enabled = true;
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
            playerAttack.canAttack = true;
        }
    }

    void OnDisable()
    {
        if (movement != null) movement.enabled = false;
        if (playerAttack != null) playerAttack.enabled = false;
    }

    void Update()
    {
        if (instance == null)
        {
            instance = SelectedCharacter.Active;
            if (instance == null) return;

            if (movement != null) movement.speed = instance.Speed;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            RequestBreath();

        if (breathe && !breathing)
            StartCoroutine(BreatheRoutine());
    }

    public void RequestBreath()
    {
        if (breathing || breathe) return;
        breathe = true;
    }

    private IEnumerator BreatheRoutine()
    {
        breathing = true;
        yield return new WaitForSeconds(BreatheDuration);

        if (playerAttack != null) playerAttack.canAttack = true;
        if (instance != null)
        {
            instance.Speed = instance.SpeedMax;
            if (movement != null) movement.speed = instance.Speed;
        }

        // hp is untouched, and ability-specific state (e.g. HammerTheGap's
        // bonus damage) only resets if that ability listens for this itself.
        if (instance != null)
            EventMap.TurnEnded(GetComponent<chatemplate>());

        breathing = false;
        breathe = false;
    }
}
