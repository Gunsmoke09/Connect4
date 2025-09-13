using System;

namespace Connect4
{
    public static class Testing
    {
        public static void Run(string sequence)
        {
            var game = new GameAlgo(6, 7, _GameMode.HumanVsHuman);
            var moves = sequence.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var mv in moves)
            {
                var trimmed = mv.Trim();
                if (trimmed.Length < 2) continue;
                char typeChar = trimmed[0];
                if (!int.TryParse(trimmed.Substring(1), out int col)) continue;
                var player = game.CurrentPlayer;
                char up = char.ToUpperInvariant(typeChar);
                _DiscType type;
                if (up == 'O' || typeChar == '0') type = _DiscType.Ordinary;
                else if (up == 'B') type = _DiscType.Boring;
                else if (up == 'M') type = _DiscType.Magnetic;
                else continue;
                bool win = game.DropDisc(player, type, col - 1, false);
                if (win)
                {
                    RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
                    Console.WriteLine($"Player {(int)player.Id + 1} wins.");
                    return;
                }
                if (game.BoardFull())
                {
                    RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
                    Console.WriteLine("Draw.");
                    return;
                }
                game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
            }
            RenderGrid.PrintBoard(game.Board, game.Rows, game.Columns);
        }
    }
}
