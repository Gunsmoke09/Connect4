namespace Connect4
{
    public class GameStatus
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string[] Board { get; set; }
        public PlayerStatusBar[] Players { get; set; }
        public int CurrentPlayer { get; set; }
        public GameMode Mode { get; set; }
    }
}

