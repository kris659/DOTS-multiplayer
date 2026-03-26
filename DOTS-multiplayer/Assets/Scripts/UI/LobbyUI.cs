using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : UIWindow
{
    public static FixedString32Bytes PlayerName { get; private set; }

    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private TMP_InputField _ipAddressInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;

    protected override void Awake()
    {
        base.Awake();
        _createGameButton.onClick.AddListener(OnCreateGameButtonPressed);
        _joinGameButton.onClick.AddListener(OnJoinGameButtonPressed);
    }


    private void OnCreateGameButtonPressed()
    {
        if (!TryGetEndpoint(out NetworkEndpoint networkEndpoint))
            return;

        var server = GameBootstrap.CreateServerWorld("ServerWorld");
        var client = GameBootstrap.CreateClientWorld("ClientWorld");


        using var serverDriverQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        ref var serverDriver = ref serverDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW;
        serverDriver.RequireConnectionApproval = false;
        serverDriver.Listen(NetworkEndpoint.AnyIpv4.WithPort(networkEndpoint.Port));
        using var clientDriverQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        clientDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, networkEndpoint);
        StartGame();

        Debug.Log("CreateButton");
    }

    private void OnJoinGameButtonPressed()
    {
        if (!TryGetEndpoint(out NetworkEndpoint networkEndpoint))
            return;

        var client = GameBootstrap.CreateClientWorld("ClientWorld");
        using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, networkEndpoint);
        StartGame();

        Debug.Log("JoinButton");
    }

    private bool TryGetEndpoint(out NetworkEndpoint networkEndpoint)
    {
        networkEndpoint = default;
        if (!ushort.TryParse(_portInputField.text.Trim(), out var port))
        {
            return false;
        }
        return NetworkEndpoint.TryParse(_ipAddressInputField.text, port, out networkEndpoint);
    }

    private void StartGame()
    {
        Close();
        UIManager.Instance.ChatUI.Open();
        SceneLoader.Instance.LoadGameScene();
        PlayerName = _playerNameInputField.text;
    }
}
