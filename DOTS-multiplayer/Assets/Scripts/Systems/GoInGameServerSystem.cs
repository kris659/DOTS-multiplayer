using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntityPrefabs>();
        var query = SystemAPI.QueryBuilder().WithAll<GoInGameRequest, ReceiveRpcCommandRequest>().Build();
        state.RequireForUpdate(query);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerPrefab = SystemAPI.GetSingleton<EntityPrefabs>().Player;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (requestSource, requestData, requestEntity) in
                 SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoInGameRequest>>()
                     .WithEntityAccess())
        {
            ecb.AddComponent<NetworkStreamInGame>(requestSource.ValueRO.SourceConnection);

            {
                var networkId = SystemAPI.GetComponent<NetworkId>(requestSource.ValueRO.SourceConnection);

                var player = ecb.Instantiate(playerPrefab);
                ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
                ecb.SetComponent(player, new PlayerData { NetworkId = networkId.Value, Name = requestData.ValueRO.PlayerName });

                ecb.AppendToBuffer(requestSource.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            }

            ecb.DestroyEntity(requestEntity);
        }
        ecb.Playback(state.EntityManager);
    }
}
