using Unity.Collections;
using Unity.NetCode;

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
