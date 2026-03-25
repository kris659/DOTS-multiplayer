using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ChatServerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (message, source, entity) in
                    SystemAPI.Query<RefRO<SendMessageRequest>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess()) {
            Debug.Log($"Server received message: {message.ValueRO.Message}");

            var sourceNetworkId = SystemAPI.GetComponent<NetworkId>(source.ValueRO.SourceConnection);
            var newMessage = ecb.CreateEntity();
            ecb.AddComponent(newMessage, new ReceiveMessageRequest { Message = message.ValueRO.Message, SenderNetworkId = sourceNetworkId.Value, ReceiverNetworkId = message.ValueRO.DestinationNetworkId });

            var targetConnection = Entity.Null;
            if (message.ValueRO.DestinationNetworkId != 0) {
                foreach (var (netId, connEntity) in
                        SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess()) {
                    if (netId.ValueRO.Value == message.ValueRO.DestinationNetworkId) {
                        targetConnection = connEntity;
                        break;
                    }
                }
            }

            ecb.AddComponent(newMessage, new SendRpcCommandRequest { TargetConnection = targetConnection });

            // Send message back to sender
            if(targetConnection != Entity.Null) {
                newMessage = ecb.CreateEntity();
                ecb.AddComponent(newMessage, new ReceiveMessageRequest { Message = message.ValueRO.Message, SenderNetworkId = sourceNetworkId.Value, ReceiverNetworkId = message.ValueRO.DestinationNetworkId });
                ecb.AddComponent(newMessage, new SendRpcCommandRequest { TargetConnection = source.ValueRO.SourceConnection });
            }

            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ChatClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (message, entity) in
                    SystemAPI.Query<RefRO<ReceiveMessageRequest>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess()) {
            Debug.Log($"Client received message: {message.ValueRO.Message}");
            ChatManager.ReceiveMessage(message.ValueRO);
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}
