using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
}

class PlayerBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerTag { });
        AddComponent(entity, new PlayerInput { });
        AddComponent(entity, new PlayerData { });
    }
}
