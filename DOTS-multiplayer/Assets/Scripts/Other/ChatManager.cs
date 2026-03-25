using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public static class ChatManager 
{
    public static void ReceiveMessage(ReceiveMessageRequest message)
    {
        Debug.Log($"Receiving message: {message.Message}");
        if(UIManager.Instance != null && UIManager.Instance.ChatUI != null)
        {
            UIManager.Instance.ChatUI.AddMessage(message);
        }
    }

    public static void SendMessage(string message, int destinationNetworkId)
    {
        var entityManager = GameBootstrap.ClientWorld.EntityManager;
        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, new SendMessageRequest { Message = message, DestinationNetworkId = destinationNetworkId });
        entityManager.AddComponentData(entity, new SendRpcCommandRequest());
    }
}

public struct SendMessageRequest : IRpcCommand
{
    public FixedString32Bytes Message;
    public int DestinationNetworkId;
}

public struct ReceiveMessageRequest : IRpcCommand
{
    public FixedString32Bytes Message;
    public int SenderNetworkId;
    public int ReceiverNetworkId;
}
