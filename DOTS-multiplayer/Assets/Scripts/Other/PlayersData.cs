using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class PlayersData
{
    public static event Action PlayersDataChanged;
    public static Entity LocalPlayer { get; private set; }
    public static IReadOnlyDictionary<Entity, PlayerData> PlayerDataDictionary => _playerDataDictionary;

    private static readonly Dictionary<Entity, PlayerData> _playerDataDictionary = new();

    public static PlayerData GetPlayerDataByNetworkId(int networkId)
    {
        foreach(var data in _playerDataDictionary.Values) {
            if (data.NetworkId == networkId) {
                return data;
            }
        }
        return default;
    }

    public static void AddPlayer(Entity player, PlayerData playerData)
    {
        if(_playerDataDictionary.ContainsKey(player)) {
            Debug.LogWarning($"Trying to add player {player} that is already in the dictionary");
            return;
        }
        _playerDataDictionary.Add(player, playerData);
        Debug.Log($"Player {player} joined the game");
        PlayersDataChanged?.Invoke();
    }

    public static void RemovePlayer(Entity player)
    {
        if (_playerDataDictionary.ContainsKey(player))
            _playerDataDictionary.Remove(player);
        else
            Debug.LogWarning($"Trying to remove player {player} that is not in the dictionary");
        Debug.Log($"Player {player} left the game");
        PlayersDataChanged?.Invoke();
    }

    public static void SetLocalPlayer(Entity player)
    {
        Debug.Log("Local player set");
        LocalPlayer = player;
    }

    public static void UpdatePlayerData(Entity player, PlayerData playerData)
    {
        Debug.Log($"Player data updated. ID: {playerData.NetworkId} NAME: {playerData.Name}");
        if (_playerDataDictionary.ContainsKey(player))
            _playerDataDictionary[player] = playerData;
        else
            Debug.LogWarning("Player is not in dictionary!");
        PlayersDataChanged?.Invoke();
    }
}
