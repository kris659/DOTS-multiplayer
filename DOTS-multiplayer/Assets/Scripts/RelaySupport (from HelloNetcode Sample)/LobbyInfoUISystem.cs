using System;
using Unity.Entities;
using Unity.NetCode;

namespace Samples.HelloNetcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [DisableAutoCreation]
    public partial class LobbyInfoUISystem : SystemBase
    {
        string m_PingText;
        DateTime m_LastDisconnectTime;

        protected override void OnUpdate()
        {
            if (UIManager.Instance == null || UIManager.Instance.LobbyInfoUI == null)
                return;

            CompleteDependency();

            if (!SystemAPI.TryGetSingletonEntity<NetworkStreamConnection>(out var connectionEntity))
            {
                if (m_LastDisconnectTime == default)
                {
                    m_LastDisconnectTime = DateTime.Now;
                }
                UIManager.Instance.LobbyInfoUI.ConnectionStatus = $"Not connected! [{DateTime.Now.Subtract(m_LastDisconnectTime)}]";
                m_PingText = default;
            }
            else
            {
                m_LastDisconnectTime = default;
                var connection = EntityManager.GetComponentData<NetworkStreamConnection>(connectionEntity);
                var address = SystemAPI.GetSingletonRW<NetworkStreamDriver>().ValueRO.GetRemoteEndPoint(connection).Address;
                if (EntityManager.HasComponent<NetworkId>(connectionEntity))
                {
                    if (string.IsNullOrEmpty(m_PingText) || UnityEngine.Time.frameCount % 30 == 0)
                    {
                        var networkSnapshotAck = EntityManager.GetComponentData<NetworkSnapshotAck>(connectionEntity);
                        m_PingText = networkSnapshotAck.EstimatedRTT > 0 ? $"{(int)networkSnapshotAck.EstimatedRTT}ms" : "Connected";
                    }

                    UIManager.Instance.LobbyInfoUI.ConnectionStatus = $"{address} | {m_PingText}";
                }
                else
                    UIManager.Instance.LobbyInfoUI.ConnectionStatus = $"{address} | Connecting";
            }
        }
    }
}
