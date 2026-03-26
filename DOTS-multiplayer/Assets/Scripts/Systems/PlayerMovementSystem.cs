using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{

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
                    SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInput>>().WithAll<PlayerTag>())
        {
            float3 direction = new(input.ValueRO.Move.x, 0, input.ValueRO.Move.y);
            transform.ValueRW.Position += playerConfig.BaseMovementSpeed * deltaTime * direction;
        }
    }
}
