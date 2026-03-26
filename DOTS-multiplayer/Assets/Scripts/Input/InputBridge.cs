using UnityEngine;

public class InputBridge : MonoBehaviour
{
    public static PlayerInputActions Input;

    void Awake()
    {
        Input = new PlayerInputActions();
        Input.Enable();
    }
}