using System;

namespace Connect4
{
    public class GameStatus
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string[] Board { get; set; } = Array.Empty<string>();
        public PlayerStatusBar[] Players { get; set; } = Array.Empty<PlayerStatusBar>();
        public int CurrentPlayer { get; set; }
        public GameMode Mode { get; set; }
    }
}

