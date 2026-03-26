using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[BurstCompile(CompileSynchronously = false, DisableDirectCall = true, FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard, DisableSafetyChecks = false)]
public partial struct InputCollectionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkId = GetSingleton<NetworkId>().Value;
        if (InputBridge.Input == null)
            return;

        foreach (var (input, owner, playerInput) in Query<RefRW<PlayerInput>, RefRO<GhostOwner>, RefRO<PlayerInputHandle>>())
        {
            if (owner.ValueRO.NetworkId != networkId)
            {
                continue;
            }

            input.ValueRW.Move = default;

            var inputData = playerInput.ValueRO.GetInput();

            input.ValueRW.Move = inputData.Move.ReadValue<Vector2>();
            playerInput.ValueRO.Free();

        }
    }
}
