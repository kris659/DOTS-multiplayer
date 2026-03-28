using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ChatClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (message, entity) in
                    SystemAPI.Query<RefRO<ReceiveMessageRequest>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ChatManager.ReceiveMessage(message.ValueRO);
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}
