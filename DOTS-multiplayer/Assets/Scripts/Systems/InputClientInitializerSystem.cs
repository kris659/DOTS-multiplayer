using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InputClientInitializerSystem : SystemBase
{
    private PlayerInputActions _playerInputActions;
    private GCHandle _handle;

    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(
            ComponentType.ReadOnly<PlayerInput>(),
            ComponentType.ReadOnly<GhostOwnerIsLocal>(),
            ComponentType.Exclude<PlayerInputHandle>()));

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();
        _playerInputActions.Player.Enable();
        _handle = GCHandle.Alloc(_playerInputActions, GCHandleType.Pinned);
    }

    protected override void OnUpdate()
    {
        var cmd = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(World.Unmanaged);

        foreach (var (input, entity) in SystemAPI
                        .Query<RefRO<PlayerInput>>()
                        .WithEntityAccess()
                        .WithNone<PlayerInputHandle>())
        {
            cmd.AddComponent(entity, new PlayerInputHandle
            {
                Value = GCHandle.ToIntPtr(_handle)
            });
        }
    }

    protected override void OnDestroy()
    {
        if (_handle.IsAllocated)
            _handle.Free();
    }
}
