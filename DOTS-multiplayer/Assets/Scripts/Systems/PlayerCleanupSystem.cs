using Unity.Entities;

public partial struct PlayerCleanupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (cleanup, entity) in
                    SystemAPI.Query<RefRO<PlayerCleanupComponent>>().WithNone<PlayerTag>().WithEntityAccess())
        {
            PlayersData.RemovePlayer(entity);
            ecb.RemoveComponent<PlayerCleanupComponent>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
