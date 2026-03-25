using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct PlayerTag : IComponentData
{

}

public struct PlayerJoinedTag : IComponentData
{

}

public struct PlayerCleanupComponent : ICleanupComponentData
{

}

public struct PlayerData : IComponentData
{
    [GhostField] public int NetworkId;
    [GhostField] public FixedString32Bytes Name;
}
