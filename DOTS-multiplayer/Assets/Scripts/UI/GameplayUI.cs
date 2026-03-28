using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUI : UIWindow
{
    protected override void Awake() { }

    public override void Open()
    {
        UIManager.Instance.ChatUI.Open();
        UIManager.Instance.LobbyInfoUI.Open();
    }
}
