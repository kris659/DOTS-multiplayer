using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct PlayerInput : IInputComponentData
{
    public float2 Move;
}
