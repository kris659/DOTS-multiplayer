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

        foreach (var (input, owner) in Query<RefRW<PlayerInput>, RefRO<GhostOwner>>())
        {
            if (owner.ValueRO.NetworkId != networkId)
            {
                continue;
            }

            float2 move = InputBridge.Input.Player.Move.ReadValue<Vector2>();
            input.ValueRW.Move = move;
        }
    }
}
