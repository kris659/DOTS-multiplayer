using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct PlayerMovementSystem : ISystem { 

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var playerConfig = SystemAPI.GetSingleton<PlayerConfig>();

        foreach (var (transform, input) in
                    SystemAPI.Query<RefRW<LocalTransform>, RefRO <PlayerInput>>().WithAll<Player>()) {
            float3 direction = new(input.ValueRO.Move, 0);
            transform.ValueRW.Position += playerConfig.BaseMovementSpeed * deltaTime * direction;
        }
    }
}
