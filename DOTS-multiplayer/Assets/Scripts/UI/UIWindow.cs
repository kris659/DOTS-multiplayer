using UnityEngine;

public class UIWindow : MonoBehaviour
{
    private GameObject _uiWindow;

    protected virtual void Awake()
    {
        if (transform.childCount != 1)
        {
            Debug.LogError("UIWindow should have only one child");
        }
        _uiWindow = transform.GetChild(0).gameObject;
    }

    public virtual void Open()
    {
        _uiWindow.SetActive(true);
    }

    public virtual void Close()
    {
        _uiWindow.SetActive(false);
    }
}
