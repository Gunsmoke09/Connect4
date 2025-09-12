using System;
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
        int modeSel = ReadInt(1, 2);
        GameMode mode = modeSel == 1 ? GameMode.HumanVsHuman : GameMode.HumanVsComputer;

        Console.WriteLine("Enter rows (>=6):");
        int rows = ReadInt(6, 100);
        Console.WriteLine("Enter columns (>=7 and >=rows):");
        int cols;
        while (true)
        {
            cols = ReadInt(7, 100);
            if (cols >= rows) break;
            Console.WriteLine("Columns must be >= rows");
        }
        return new Game(rows, cols, mode);
    }

    static int ReadInt(int min, int max)
    {
        while (true)
        {
            string? s = Console.ReadLine();
            int v = 0;
            int.TryParse(s, out v);
            if (int.TryParse(s, out int v2)) v = v2;
            if (v >= min && v <= max)
                return v;
            Console.WriteLine($"Enter number between {min} and {max}");
        }
    }

    static void PlayGame(Game game)
    {
        while (true)
        {
            var player = game.CurrentPlayer;
            Console.WriteLine();
            game.DisplayBoard();
            ShowHud(game);
            bool win;
            if (player.IsComputer)
            {
                Console.WriteLine("Computer thinking...");
                win = game.ComputerTurn();
                if (win)
                {
                    game.DisplayBoard();
                    Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                    return;
                }
                if (game.BoardFull())
                {
                    game.DisplayBoard();
                    Console.WriteLine("It's a draw.");
                    return;
                }
                game.SwitchPlayer();
                continue;
            }

            DiscType type;
            int column;
            if (!TryGetValidMove(game, player, out type, out column)) return;
            win = game.DropDisc(player, type, column, true);
            if (!win && game.BoardFull())
            {
                game.DisplayBoard();
                Console.WriteLine("It's a draw.");
                return;
            }
            if (win)
            {
                game.DisplayBoard();
                Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                return;
            }
            game.SwitchPlayer();
        }
    }

    static bool TryGetValidMove(Game game, Player player, out DiscType disc, out int column)
    {
        while (true)
        {
            Console.WriteLine($"Player {(int)player.Id + 1} turn. Enter move (e.g., O4, B3, M5) or 'save <file>' or 'help':");
            string? input = Console.ReadLine();
            if (input == null)
            {
                disc = DiscType.Ordinary; column = 0; continue;
            }
            input = input.Trim();
            if (input.StartsWith("save"))
            {
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    game.Save(parts[1]);
                    Console.WriteLine("Game saved.");
                }
                else Console.WriteLine("Usage: save filename");
                continue;
            }
            if (input == "help")
            {
                ShowHelp();
                continue;
            }
            if (input == "exit")
            {
                disc = DiscType.Ordinary; column = 0; return false;
            }
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
            DiscType type = ParseDiscType(typeChar);
            int col = colInput - 1;
            if (col < 0 || col >= game.Columns)
            {
                Console.WriteLine($"⚠️ Invalid column. Please enter a number between 1 and {game.Columns}.");
                continue;
            }
            if (game.ColumnFull(col) && type != DiscType.Boring)
            {
                Console.WriteLine("⚠️ That column is full. Choose another column.");
                continue;
            }
            if (!player.HasDisc(type))
            {
                Console.WriteLine($"⚠️ You have no {type.ToString().ToUpperInvariant()} discs left. Choose a disc type you still have.");
                continue;
            }
            disc = type;
            column = col;
            return true;
        }
    }

    static char SymbolFor(PlayerId id) => id == PlayerId.One ? 'X' : 'O';

    static void ShowHud(Game game)
    {
        Player p1 = game.CurrentPlayer.Id == PlayerId.One ? game.CurrentPlayer : game.OtherPlayer;
        Player p2 = game.CurrentPlayer.Id == PlayerId.Two ? game.CurrentPlayer : game.OtherPlayer;
        Console.WriteLine($"Player 1 ({SymbolFor(PlayerId.One)}) — Ordinary: {p1.Ordinary}, Boring: {p1.Boring}, Magnetic: {p1.Magnetic}");
        Console.WriteLine($"Player 2 ({SymbolFor(PlayerId.Two)}) — Ordinary: {p2.Ordinary}, Boring: {p2.Boring}, Magnetic: {p2.Magnetic}");
    }

    static DiscType ParseDiscType(char c)
    {
        char up = char.ToUpperInvariant(c);
        DiscType t = DiscType.Ordinary;
        if (up == 'O')
        {
            t = DiscType.Ordinary;
        }
        else if (up == 'B')
        {
            t = DiscType.Boring;
        }
        else if (up == 'M')
        {
            t = DiscType.Magnetic;
        }
        return t;
    }

    static void ShowHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("O# - ordinary disc in column #");
        Console.WriteLine("B# - boring disc in column # (removes column)");
        Console.WriteLine("M# - magnetic disc in column # (lifts own disc)");
        Console.WriteLine("save <file> - save game");
        Console.WriteLine("help - show this help");
        Console.WriteLine("exit - quit");
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
            DiscType type = ParseDiscType(typeChar);
            bool win = game.DropDisc(player, type, col - 1, false);
            if (win)
            {
                game.DisplayBoard();
                Console.WriteLine($"Player {(int)player.Id + 1} wins.");
                return;
            }
            if (game.BoardFull())
            {
                game.DisplayBoard();
                Console.WriteLine("Draw.");
                return;
            }
            game.SwitchPlayer();
        }
        game.DisplayBoard();
    }
}
