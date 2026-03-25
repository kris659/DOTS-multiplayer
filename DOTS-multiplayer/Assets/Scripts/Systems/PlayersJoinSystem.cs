using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct PlayersJoinSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }


    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var localNetworkId = SystemAPI.GetSingleton<NetworkId>();

        foreach (var (playerData, ghostOwner, entity) in
                SystemAPI.Query<RefRW<PlayerData>, RefRO<GhostOwner>>()
                    .WithNone<PlayerJoinedTag>()
                    .WithEntityAccess()) {

            PlayersData.AddPlayer(entity, playerData.ValueRO);
            if (ghostOwner.ValueRO.NetworkId == localNetworkId.Value) {
                PlayersData.SetLocalPlayer(entity);
            }

            ecb.AddComponent<PlayerJoinedTag>(entity);
            ecb.AddComponent<PlayerCleanupComponent>(entity);
        }

        foreach (var (playerData, entity) in SystemAPI.Query<RefRO<PlayerData>>().WithChangeFilter<PlayerData>().WithEntityAccess()) {
            PlayersData.UpdatePlayerData(entity, playerData.ValueRO);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
