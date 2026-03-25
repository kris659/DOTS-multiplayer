using Unity.NetCode;

// A custom bootstrap which enables auto-connect and creates the Client and Server worlds.
[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        //AutoConnectPort = 7979; // Enabled auto connect
        //return base.Initialize(defaultWorldName); // Use the regular bootstrap

        CreateLocalWorld(defaultWorldName);
        return true;
    }
}


