namespace DungeonLegacy.Player
{
    public interface IPlayerState
    {
        void Enter(PlayerContext ctx);
        void Update(PlayerContext ctx);
        void FixedUpdate(PlayerContext ctx);
        void Exit(PlayerContext ctx);
    }
}