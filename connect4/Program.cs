using System;
using System.Collections.Generic;
using System.IO;
using Connect4;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            string sequence = args.Length > 1 ? args[1] : Console.ReadLine() ?? string.Empty;
            RunTest(sequence);
            return;
        }

        Console.WriteLine("Welcome to Connect4!");
        Console.WriteLine("Load game? (y/n)");
        string? choice = Console.ReadLine();
        Game game;
        if (choice != null && choice.Trim().ToLower() == "y")
        {
            Console.WriteLine("Enter filename:");
            string? fname = Console.ReadLine();
            if (fname != null && File.Exists(fname))
                game = Game.Load(fname);
            else
            {
                Console.WriteLine("File not found. Starting new game.");
                game = StartNew();
            }
        }
        else
        {
            game = StartNew();
        }

        PlayGame(game);
    }

    static Game StartNew()
    {
        Console.WriteLine("Select mode: 1) Human vs Human 2) Human vs Computer");
        int modeSel;
        while (true)
        {
            string? s = Console.ReadLine();
            if (int.TryParse(s, out modeSel) && modeSel >= 1 && modeSel <= 2) break;
            Console.WriteLine("Enter number between 1 and 2");
        }
        GameMode mode = modeSel == 1 ? GameMode.HumanVsHuman : GameMode.HumanVsComputer;

        Console.WriteLine("Enter rows (>=6):");
        int rows;
        while (true)
        {
            string? s = Console.ReadLine();
            if (int.TryParse(s, out rows) && rows >= 6 && rows <= 100) break;
            Console.WriteLine("Enter number between 6 and 100");
        }
        Console.WriteLine("Enter columns (>=7 and >=rows):");
        int cols;
        while (true)
        {
            string? s = Console.ReadLine();
            if (int.TryParse(s, out cols) && cols >= 7 && cols <= 100 && cols >= rows) break;
            Console.WriteLine("Columns must be >= rows and between 7 and 100");
        }
        return new Game(rows, cols, mode);
    }

    static void PlayGame(Game game)
    {
        var rng = new Random();
        while (true)
        {
            var player = game.CurrentPlayer;
            Console.WriteLine();
            GridHelper.Display(game.Board, game.Rows, game.Columns);
            Player p1 = game.CurrentPlayer.Id == PlayerId.One ? game.CurrentPlayer : game.OtherPlayer;
            Player p2 = game.CurrentPlayer.Id == PlayerId.Two ? game.CurrentPlayer : game.OtherPlayer;
            Console.WriteLine($"Player 1 (X) — Ordinary: {p1.Ordinary}, Boring: {p1.Boring}, Magnetic: {p1.Magnetic}");
            Console.WriteLine($"Player 2 (O) — Ordinary: {p2.Ordinary}, Boring: {p2.Boring}, Magnetic: {p2.Magnetic}");
            bool win;
            if (player.IsComputer)
            {
                Console.WriteLine("Computer thinking...");
                var types = new List<DiscType>();
                if (player.HasDisc(DiscType.Ordinary)) types.Add(DiscType.Ordinary);
                if (player.HasDisc(DiscType.Boring)) types.Add(DiscType.Boring);
                if (player.HasDisc(DiscType.Magnetic)) types.Add(DiscType.Magnetic);
                DiscType t = types[rng.Next(types.Count)];
                int chosen;
                if (t == DiscType.Boring)
                {
                    chosen = rng.Next(game.Columns);
                }
                else
                {
                    var cols = new List<int>();
                    for (int c = 0; c < game.Columns; c++)
                    {
                        if (game.Board[game.Rows - 1, c] == ' ') cols.Add(c);
                    }
                    if (cols.Count == 0) return;
                    chosen = cols[rng.Next(cols.Count)];
                }
                win = game.DropDisc(player, t, chosen, true);
                if (win)
                {
                    GridHelper.Display(game.Board, game.Rows, game.Columns);
                    Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                    return;
                }
                if (game.BoardFull())
                {
                    GridHelper.Display(game.Board, game.Rows, game.Columns);
                    Console.WriteLine("It's a draw.");
                    return;
                }
                game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
                continue;
            }

            DiscType type;
            int column;
            while (true)
            {
                Console.WriteLine($"Player {(int)player.Id + 1} turn. Enter move (e.g., O4, B3, M5) or 'save <file>' or 'help':");
                string? input = Console.ReadLine();
                if (input == null) continue;
                input = input.Trim();
                if (input.StartsWith("save"))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        game.Save(parts[1]);
                        Console.WriteLine("Game saved.");
                    }
                    else
                    {
                        Console.WriteLine("Usage: save filename");
                    }
                    continue;
                }
                if (input == "help")
                {
                    Console.WriteLine("Commands:");
                    Console.WriteLine("O# - ordinary disc in column #");
                    Console.WriteLine("B# - boring disc in column # (removes column)");
                    Console.WriteLine("M# - magnetic disc in column # (lifts own disc)");
                    Console.WriteLine("save <file> - save game");
                    Console.WriteLine("help - show this help");
                    Console.WriteLine("exit - quit");
                    continue;
                }
                if (input == "exit") return;
                if (input.Length < 2)
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }
                char typeChar = input[0];
                if (!int.TryParse(input.Substring(1), out int colInput))
                {
                    Console.WriteLine("Invalid column");
                    continue;
                }
                char up = char.ToUpperInvariant(typeChar);
                DiscType t = DiscType.Ordinary;
                if (up == 'B') t = DiscType.Boring; else if (up == 'M') t = DiscType.Magnetic;
                int col = colInput - 1;
                if (col < 0 || col >= game.Columns)
                {
                    Console.WriteLine($"⚠️ Invalid column. Please enter a number between 1 and {game.Columns}.");
                    continue;
                }
                if (game.Board[game.Rows - 1, col] != ' ' && t != DiscType.Boring)
                {
                    Console.WriteLine("⚠️ That column is full. Choose another column.");
                    continue;
                }
                if (!player.HasDisc(t))
                {
                    Console.WriteLine($"⚠️ You have no {t.ToString().ToUpperInvariant()} discs left. Choose a disc type you still have.");
                    continue;
                }
                type = t;
                column = col;
                break;
            }
            win = game.DropDisc(player, type, column, true);
            if (!win && game.BoardFull())
            {
                GridHelper.Display(game.Board, game.Rows, game.Columns);
                Console.WriteLine("It's a draw.");
                return;
            }
            if (win)
            {
                GridHelper.Display(game.Board, game.Rows, game.Columns);
                Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                return;
            }
            game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
        }
    }

    static void RunTest(string sequence)
    {
        var game = new Game(6, 7, GameMode.HumanVsHuman);
        var moves = sequence.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var mv in moves)
        {
            var trimmed = mv.Trim();
            if (trimmed.Length < 2) continue;
            char typeChar = trimmed[0];
            if (!int.TryParse(trimmed.Substring(1), out int col)) continue;
            var player = game.CurrentPlayer;
            char up = char.ToUpperInvariant(typeChar);
            DiscType type = DiscType.Ordinary;
            if (up == 'B') type = DiscType.Boring; else if (up == 'M') type = DiscType.Magnetic;
            bool win = game.DropDisc(player, type, col - 1, false);
            if (win)
            {
                GridHelper.Display(game.Board, game.Rows, game.Columns);
                Console.WriteLine($"Player {(int)player.Id + 1} wins.");
                return;
            }
            if (game.BoardFull())
            {
                GridHelper.Display(game.Board, game.Rows, game.Columns);
                Console.WriteLine("Draw.");
                return;
            }
            game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
        }
        GridHelper.Display(game.Board, game.Rows, game.Columns);
    }
}
