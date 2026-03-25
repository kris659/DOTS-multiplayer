using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct PlayerInput : IInputComponentData
{
    public float2 Move;
}

public struct PlayerInputHandle : IComponentData
{
    public IntPtr Value;
    public GCHandle gCHandle;
    public PlayerInputActions.PlayerActions GetInput()
    {
        gCHandle = GCHandle.FromIntPtr(Value);
        var gameInput = gCHandle.Target as PlayerInputActions;
        return gameInput.Player;
    }

    public void Free()
    {
        if (gCHandle.IsAllocated)
            gCHandle.Free();
    }
}
