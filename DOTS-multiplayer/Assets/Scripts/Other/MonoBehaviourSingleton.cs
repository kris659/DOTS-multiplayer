using UnityEngine;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance;


    protected virtual void Awake()
	{
		if (Instance == null)
		{
            Instance = this as T;
		}
		else if (Instance != this)
		{
            Debug.LogError($"Multiple instances of: [{typeof(T).Name}]");
            Destroy(this.gameObject);
		}
	}
}
