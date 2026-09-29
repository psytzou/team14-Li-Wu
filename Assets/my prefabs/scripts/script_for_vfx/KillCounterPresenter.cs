using UnityEngine;

public sealed class KillCounterPresenter : MonoBehaviour
{
    [SerializeField] private EndlessGameSession session;
    [SerializeField] private KillCounterView view;

    private void OnEnable()
    {
        if (session == null)
            session = EndlessGameSession.Instance;

        if (session == null) return;
        session.KillCountChanged += HandleKillCountChanged;
        HandleKillCountChanged(session.KillCount);
    }

    private void OnDisable()
    {
        if (session != null)
            session.KillCountChanged -= HandleKillCountChanged;
    }

    private void HandleKillCountChanged(int kills)
    {
        if (view != null)
            view.SetKills(kills);
    }
}
