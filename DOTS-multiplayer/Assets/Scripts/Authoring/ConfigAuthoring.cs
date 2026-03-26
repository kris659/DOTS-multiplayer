using Unity.Entities;
using UnityEngine;

public class ConfigAuthoring : MonoBehaviour
{
    public ConfigSO ConfigSO;

    class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            DependsOn(authoring.ConfigSO);

            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
            AddComponent(entity, new EntityPrefabs
            {
                Player = GetEntity(authoring.ConfigSO.PlayerPrefab, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, authoring.ConfigSO.PlayerConfig);
        }
    }
}
