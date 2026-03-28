using Samples.HelloNetcode;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyInfoUI : UIWindow
{
    [SerializeField] private TMP_Text _connectionInfoText;
    [SerializeField] private TMP_Text _joinCodeText;

    public string ConnectionStatus
    {
        get { return _connectionInfoText.text; }
        set { if (!_connectionInfoText.IsDestroyed()) _connectionInfoText.text = value; }
    }

    public override void Open()
    {
        base.Open();
        UpdateJoinCode();

        if (ClientServerBootstrap.ClientWorld != null)
        {
            var sys = ClientServerBootstrap.ClientWorld.GetOrCreateSystemManaged<LobbyInfoUISystem>();
            var simGroup = ClientServerBootstrap.ClientWorld.GetExistingSystemManaged<SimulationSystemGroup>();
            simGroup.AddSystemToUpdateList(sys);
        }
    }

    private void UpdateJoinCode()
    {
        var world = World.All[0];
        var joinQuery = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<JoinCode>());
        if (joinQuery.HasSingleton<JoinCode>())
        {
            var joinCode = joinQuery.GetSingleton<JoinCode>().Value;
            _joinCodeText.text = $"Join code: {joinCode}";
        }
    }
    private void ReturnToFrontend()
    {
        Debug.Log("[ReturnToFrontend] Called.");

        if (MultiplayerService.Instance != null)
        {
            foreach (var session in MultiplayerService.Instance.Sessions)
            {
                Debug.Log($"[ReturnToFrontend] Leaving {session.Value.Id}");
                session.Value.LeaveAsync();
            }
        }

        // Session destroys the server world but server destruction will also be covered here when not using sessions.
        // Client world will always need to be destroyed
        var clientServerWorlds = new List<World>();
        foreach (var world in World.All)
        {
            if (world.IsClient() || world.IsServer())
                clientServerWorlds.Add(world);
        }

        foreach (var world in clientServerWorlds)
        {
            Debug.Log($"Disposing World {world.Name}");
            world.Dispose();
        }

        if (string.IsNullOrEmpty(LobbyUI.OldFrontendWorldName))
            LobbyUI.OldFrontendWorldName = "DefaultWorld";
        ClientServerBootstrap.CreateLocalWorld(LobbyUI.OldFrontendWorldName);
        SceneManager.LoadScene(LobbyUI.SceneName, LoadSceneMode.Single);
    }
}
