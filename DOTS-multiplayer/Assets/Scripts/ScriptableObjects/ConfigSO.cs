using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "ScriptableObjects/ConfigScriptableObject", order = 1)]
public class ConfigSO : ScriptableObject
{
    public PlayerConfig PlayerConfig;
    public GameObject PlayerPrefab;
}
