using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct GoInGameRequest : IRpcCommand
{
    public FixedString32Bytes PlayerName;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntityPrefabs>();
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (id, entity) in
                 SystemAPI.Query<RefRO<NetworkId>>()
                     .WithNone<NetworkStreamInGame>()
                     .WithEntityAccess())
        {
            ecb.AddComponent<NetworkStreamInGame>(entity);
            var req = ecb.CreateEntity();
            ecb.AddComponent(req, new GoInGameRequest { PlayerName = LobbyUI.PlayerName });
            ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }
        ecb.Playback(state.EntityManager);
    }
}
