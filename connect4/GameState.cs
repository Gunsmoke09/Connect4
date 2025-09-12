namespace Connect4
{
    public class GameState
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string[] Board { get; set; }
        public PlayerState[] Players { get; set; }
        public int CurrentPlayer { get; set; }
        public GameMode Mode { get; set; }
    }
}

