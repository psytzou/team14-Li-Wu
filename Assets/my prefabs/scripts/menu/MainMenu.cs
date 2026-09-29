using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Attach to any GameObject in the menu scene; wire a "Start Game" Button's
// OnClick to StartGame(). No visuals here. Set gameplaySceneName in the
// Inspector to the exact name of your gameplay scene (must also be added
// in File > Build Settings) -- not hardcoded, so it can't silently mismatch
// whatever you actually named your scene.
public class MainMenu : MonoBehaviour
{
    [Tooltip("Exact name of the gameplay scene to load. Must be added in File > Build Settings.")]
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private Button startButton;

    private bool startRequested;

    private void Update()
    {
        if (startRequested
            || startButton == null
            || !startButton.IsActive()
            || !startButton.interactable
            || Mouse.current == null
            || !Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return;
        }

        RectTransform buttonRect = startButton.transform as RectTransform;
        if (buttonRect == null)
            return;

        Canvas parentCanvas = startButton.GetComponentInParent<Canvas>();
        Camera eventCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        if (RectTransformUtility.RectangleContainsScreenPoint(
                buttonRect,
                Mouse.current.position.ReadValue(),
                eventCamera))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (startRequested)
            return;

        startRequested = true;
        if (startButton != null)
            startButton.interactable = false;

        GameSequence.StartGame(SelectedCharacter.CharacterType.Fighter, gameplaySceneName);
    }
}
