using Samples.HelloNetcode;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public struct JoinCode : IComponentData
{
    public FixedString64Bytes Value;
}

public class LobbyUI : UIWindow
{
    public static FixedString32Bytes PlayerName { get; private set; }
    public static string SceneName { get; private set; }

    public string HostConnectionStatus
    {
        get => _hostConnectionLabel.text;
        set => _hostConnectionLabel.text = value;
    }
    public string ClientConnectionStatus
    {
        get => _clientConnectionLabel.text;
        set => _clientConnectionLabel.text = value;
    }

    internal static string OldFrontendWorldName = string.Empty;

    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private Button _createGameButton;
    [SerializeField] private TMP_InputField _joinCodeInputField;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private TMP_Text _hostConnectionLabel;
    [SerializeField] private TMP_Text _clientConnectionLabel;

    private bool m_IsHosting;
    private ConnectionState m_State;
    private HostServer m_HostServerSystem;
    private ConnectingPlayer m_HostClientSystem;

    enum ConnectionState
    {
        Unknown,
        SetupHost,
        SetupClient,
        JoinGame,
        JoinLocalGame,
    }

    protected override void Awake()
    {
        base.Awake();
        _createGameButton.onClick.AddListener(OnCreateGameButtonPressed);
        _joinGameButton.onClick.AddListener(OnJoinGameButtonPressed);
    }

    private void Start()
    {
        _createGameButton.gameObject.SetActive(ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer);
        SceneName = "RelayFrontend";
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(_joinCodeInputField.text))
            _createGameButton.interactable = false;
        else
            _createGameButton.interactable = true;

        switch (m_State)
        {
            case ConnectionState.SetupHost:
                {
                    m_IsHosting = true;
                    HostServer();
                    m_State = ConnectionState.SetupClient;
                    goto case ConnectionState.SetupClient;
                }
            case ConnectionState.SetupClient:
                {
                    var isServerHostedLocally = m_HostServerSystem?.RelayServerData.Endpoint.IsValid;
                    var enteredJoinCode = !string.IsNullOrEmpty(_joinCodeInputField.text);
                    if (isServerHostedLocally.GetValueOrDefault())
                    {
                        SetupClient();
                        m_HostClientSystem.GetJoinCodeFromHost();

                        m_State = ConnectionState.JoinLocalGame;
                        goto case ConnectionState.JoinLocalGame;
                    }

                    if (enteredJoinCode)
                    {
                        JoinAsClient();
                        m_State = ConnectionState.JoinGame;
                        goto case ConnectionState.JoinGame;
                    }

                    if (!m_IsHosting)
                    {
                        _clientConnectionLabel.text = "Join Code field is empty!";
                        m_State = ConnectionState.Unknown;
                    }
                    break;
                }
            case ConnectionState.JoinGame:
                {
                    var hasClientConnectedToRelayService = m_HostClientSystem?.RelayClientData.Endpoint.IsValid;
                    if (hasClientConnectedToRelayService.GetValueOrDefault())
                    {
                        ConnectToRelayServer();
                        m_State = ConnectionState.Unknown;
                    }
                    break;
                }
            case ConnectionState.JoinLocalGame:
                {
                    var hasClientConnectedToRelayService = m_HostClientSystem?.RelayClientData.Endpoint.IsValid;
                    if (hasClientConnectedToRelayService.GetValueOrDefault())
                    {
                        SetupRelayHostedServerAndConnect();
                        m_State = ConnectionState.Unknown;
                    }
                    break;
                }
            case ConnectionState.Unknown:
                {
                    m_IsHosting = false;
                    break;
                }
            default: return;
        }
    }

    private void OnCreateGameButtonPressed()
    {
        m_State = ConnectionState.SetupHost;
    }

    private void OnJoinGameButtonPressed()
    {
        m_State = ConnectionState.SetupClient;
    }

    private void HostServer()
    {
        var world = World.All[0];
        m_HostServerSystem = world.GetOrCreateSystemManaged<HostServer>();
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServer>());
        world.EntityManager.AddComponent<EnableRelayServer>(enableRelayServerEntity);

        m_HostServerSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(m_HostServerSystem);
    }

    private void SetupClient()
    {
        var world = World.All[0];
        m_HostClientSystem = world.GetOrCreateSystemManaged<ConnectingPlayer>();
        m_HostClientSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(m_HostClientSystem);
    }

    private void JoinAsClient()
    {
        SetupClient();
        var world = World.All[0];
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServer>());
        world.EntityManager.AddComponent<EnableRelayServer>(enableRelayServerEntity);
        m_HostClientSystem.JoinUsingCode(_joinCodeInputField.text);
    }

    /// <summary>
    /// Collect relay server end point from completed systems. Set up server with relay support and connect client
    /// to hosted server through relay server.
    /// Both client and server world is manually created to allow us to override the <see cref="DriverConstructor"/>.
    ///
    /// Two singleton entities are constructed with listen and connect requests. These will be executed asynchronously.
    /// Connecting to relay server will not be bound immediately. The Request structs will ensure that we
    /// continuously poll until the connection is established.
    /// </summary>
    private void SetupRelayHostedServerAndConnect()
    {
        if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
        {
            UnityEngine.Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
            return;
        }

        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<ConnectingPlayer>()?.RelayClientData;
        var relayServerData = world.GetExistingSystemManaged<HostServer>().RelayServerData;
        var joinCode = world.GetExistingSystemManaged<HostServer>().JoinCode;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(relayServerData, relayClientData.GetValueOrDefault());
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        DestroyLocalSimulationWorld();
        World.DefaultGameObjectInjectionWorld = server;

        var joinCodeEntity = server.EntityManager.CreateEntity(ComponentType.ReadOnly<JoinCode>());
        server.EntityManager.SetComponentData(joinCodeEntity, new JoinCode { Value = joinCode });

        using var serverDriverQuery = server.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver));
        var serverDriver = serverDriverQuery.GetSingletonRW<NetworkStreamDriver>();
        serverDriver.ValueRW.RequireConnectionApproval = false;
        serverDriver.ValueRW.Listen(NetworkEndpoint.AnyIpv4);
        var ipcLocalEndPoint = serverDriver.ValueRW.DriverStore.GetDriverInstanceRO(1).driver.GetLocalEndpoint();

        using var clientDriverQuery = client.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver));
        var clientDriver = clientDriverQuery.GetSingletonRW<NetworkStreamDriver>();
        if (relayClientData.HasValue)
        {
            clientDriver.ValueRW.Connect(client.EntityManager, relayClientData.Value.Endpoint);
        }
        else
        {
            clientDriver.ValueRW.Connect(client.EntityManager, ipcLocalEndPoint);
        }

        LoadGame();
    }

    private void ConnectToRelayServer()
    {
        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<ConnectingPlayer>().RelayClientData;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(new RelayServerData(), relayClientData);
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        DestroyLocalSimulationWorld();
        World.DefaultGameObjectInjectionWorld = client;

        var networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");
        // For IPC this will not work and give an error in the transport layer. For this sample we force the client to connect through the relay service.
        // For a locally hosted server, the client would need to connect to NetworkEndpoint.AnyIpv4, and the relayClientData.Endpoint in all other cases.
        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });

        LoadGame();
    }

    private void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                OldFrontendWorldName = world.Name;
                world.Dispose();
                break;
            }
        }
    }

    private void LoadGame()
    {
        SceneLoader.Instance.LoadGameScene();
        PlayerName = _playerNameInputField.text;
        Close();
        UIManager.Instance.GameplayUI.Open();
    }


    [RuntimeInitializeOnLoadMethod]
    private static void AddQuitHandler()
    {
        Application.quitting += OnQuit;
    }

    private static void OnQuit()
    {
        Application.quitting -= OnQuit;
        if (MultiplayerService.Instance != null)
        {
            foreach (var session in MultiplayerService.Instance.Sessions)
                session.Value.LeaveAsync();
        }
    }


}
