using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public ChatUI ChatUI { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ChatUI = GetComponentInChildren<ChatUI>();
    }
}
