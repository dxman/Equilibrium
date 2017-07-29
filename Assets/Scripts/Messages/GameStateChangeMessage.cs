namespace Equilibrium.Messages
{
    public class GameStateChangeMessage
    {
        public readonly GameState NewState;

        public GameStateChangeMessage(GameState newState)
        {
            NewState = newState;
        }
    }

    public class WinningGameStateChangeMessage : GameStateChangeMessage
    {
        public readonly int WinningPlayerId;

        public WinningGameStateChangeMessage(GameState newState, int winningPlayerId)
            : base(newState)
        {
            WinningPlayerId = winningPlayerId;
        }
    }
}
