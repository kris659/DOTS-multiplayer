using System;
using Unity.Entities;

[Serializable]
public struct PlayerConfig : IComponentData
{
    public float BaseMovementSpeed;
}
