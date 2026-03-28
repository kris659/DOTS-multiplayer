
public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public GameplayUI GameplayUI { get; private set; }
    public ChatUI ChatUI { get; private set; }
    public LobbyInfoUI LobbyInfoUI { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        GameplayUI = GetComponentInChildren<GameplayUI>();
        ChatUI = GetComponentInChildren<ChatUI>();
        LobbyInfoUI = GetComponentInChildren<LobbyInfoUI>();
    }
}
