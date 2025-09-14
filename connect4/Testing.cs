using System;

namespace Connect4
{
    public static class Testing
    {
        public static void Run(string sequence)
        {
            var game = new GameAlgo(6, 7, GameMode.HumanVsHuman);
            var moves = sequence.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var mv in moves)
            {
                var trimmed = mv.Trim();
                if (trimmed.Length < 2) continue;
                char typeChar = trimmed[0];
                if (!int.TryParse(trimmed.Substring(1), out int col))
                    continue;
                var player = game.CurrentPlayer;
                char up = char.ToUpperInvariant(typeChar);
                DiscType type;
                
                if (up == 'O' || typeChar == '0') type = DiscType.Ordinary;
                else if (up == 'B') 
                    type = DiscType.Boring;
                else if (up == 'M') 
                    type = DiscType.Magnetic;
                else continue;
                var winner = game.DiscFalls(player, type, col - 1);
                if (winner.HasValue)
                {
                    RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
                    Console.WriteLine($"Player {(int)winner.Value + 1} wins.");
                    return;
                }
                if (game.BoardFull())
                {
                    RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
                    Console.WriteLine("Draw.");
                    return;
                }
                game.CurrentPlayerNumber = 1 - game.CurrentPlayerNumber;
            }
            RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
        }
    }
}
