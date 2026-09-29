using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Attach to a GameObject that stays active/enabled for the whole gameplay
// scene (so its subscription is live from the start -- not the hidden panel
// itself, which would never receive OnEnable until shown). Assign `panel`
// to your own game-over UI (starts hidden); wire its two buttons to
// BackToMenu() and Restart(). No visuals built here -- PlayingSequence owns
// the GameOver detection, GameSequence owns the actual scene loads.
public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;

    [Tooltip("Exact name of the menu scene to load. Must be added in File > Build Settings.")]
    [SerializeField] private string menuSceneName = "gaming_scene";

    private bool transitionRequested;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()
    {
        PlayingSequence.GameOverEvent += Show;
    }

    void OnDisable()
    {
        PlayingSequence.GameOverEvent -= Show;
    }

    void Update()
    {
        if (transitionRequested
            || panel == null
            || !panel.activeInHierarchy
            || Mouse.current == null
            || !Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return;
        }

        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        if (ContainsPointer(restartButton, pointerPosition))
        {
            Restart();
            return;
        }

        if (ContainsPointer(backButton, pointerPosition))
            BackToMenu();
    }

    private static bool ContainsPointer(Button button, Vector2 pointerPosition)
    {
        if (button == null || !button.IsActive() || !button.interactable)
            return false;

        RectTransform buttonRect = button.transform as RectTransform;
        if (buttonRect == null)
            return false;

        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, pointerPosition, eventCamera);
    }

    private void Show()
    {
        transitionRequested = false;
        if (panel != null) panel.SetActive(true);
        SetButtonsInteractable(true);
    }

    public void BackToMenu()
    {
        if (transitionRequested) return;

        transitionRequested = true;
        SetButtonsInteractable(false);
        GameSequence.BackToMenu(menuSceneName);
    }

    public void Restart()
    {
        if (transitionRequested) return;

        transitionRequested = true;
        SetButtonsInteractable(false);
        if (panel != null) panel.SetActive(false);
        GameSequence.Restart();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (restartButton != null) restartButton.interactable = interactable;
        if (backButton != null) backButton.interactable = interactable;
    }
}
