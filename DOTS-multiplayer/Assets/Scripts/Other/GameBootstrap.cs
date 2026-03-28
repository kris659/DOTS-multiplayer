using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        //return base.Initialize(defaultWorldName);
        CreateLocalWorld(defaultWorldName);
        return true;
    }
}


