using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InputClientInitializerSystem : SystemBase
{
    private PlayerInputActions m_GameInput;
    private GCHandle m_Handle;

    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(
            ComponentType.ReadOnly<PlayerInput>(),
            ComponentType.ReadOnly<GhostOwnerIsLocal>(),
            ComponentType.Exclude<PlayerInputHandle>()));

        m_GameInput = new PlayerInputActions();
        m_GameInput.Enable();
        m_GameInput.Player.Enable();
        m_Handle = GCHandle.Alloc(m_GameInput, GCHandleType.Pinned);
    }

    protected override void OnUpdate()
    {
        var cmd = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(World.Unmanaged);

        foreach (var (input, entity) in SystemAPI
                        .Query<RefRO<PlayerInput>>()
                        .WithEntityAccess()
                        .WithNone<PlayerInputHandle>()) {
            cmd.AddComponent(entity, new PlayerInputHandle {
                Value = GCHandle.ToIntPtr(m_Handle)
            });
        }
    }

    protected override void OnDestroy()
    {
        if (m_Handle.IsAllocated)
            m_Handle.Free();
    }
}
