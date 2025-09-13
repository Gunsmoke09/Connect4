
using Connect4;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            string sequence;
            if (args.Length > 1)
                sequence = args[1];
            else
                sequence = Console.ReadLine();
            // passing the sequence of moves as a test suite in the program
            RunTest(sequence);
            return;
        }

        Console.WriteLine("____Welcome to Connect4!____");
        Console.WriteLine("Load game? (y/n)");
        string? choice = Console.ReadLine(); //variable might hold null and that's ok!
        GameAlgo gameAlgo;
        if (choice != null && choice.Trim().ToLower() == "y") //strip whitespace and force it to lowercase
        {
            Console.WriteLine("Enter filename:");
            string? fname = Console.ReadLine();
            if (fname != null && File.Exists(fname))
                gameAlgo = GameAlgo.Load(fname);
            else 
            {
                Console.WriteLine("File not found. Starting new game.");
                gameAlgo = StartNew();
            }
        }
        else
        {
            gameAlgo = StartNew();
        }

        PlayGame(gameAlgo);
    }

    static GameAlgo StartNew()
    {
        Console.WriteLine("Select mode: 1) Human vs Human 2) Human vs Computer");
        int modeSel;
        while (true)
        {
            string? s = Console.ReadLine();
            if (int.TryParse(s, out modeSel) && modeSel >= 1 && modeSel <= 2) break;
            Console.WriteLine("Enter number between 1 and 2");
        }
        _GameMode mode = modeSel == 1 ? _GameMode.HumanVsHuman : _GameMode.HumanVsComputer;

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
        return new GameAlgo(rows, cols, mode);
    }

    static void PlayGame(GameAlgo gameAlgo)
    {
        var rng = new Random();
        while (true)
        {
            var player = gameAlgo.CurrentPlayer;
            Console.WriteLine();
            RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
            Player p1 = gameAlgo.CurrentPlayer.Id == _PlayerId.One ? gameAlgo.CurrentPlayer : gameAlgo.OtherPlayer;
            Player p2 = gameAlgo.CurrentPlayer.Id == _PlayerId.Two ? gameAlgo.CurrentPlayer : gameAlgo.OtherPlayer;
            Console.WriteLine($"Player 1 (@) — Ordinary: {p1.Ordinary}, Boring: {p1.Boring}, Magnetic: {p1.Magnetic}");
            Console.WriteLine($"Player 2 (#) — Ordinary: {p2.Ordinary}, Boring: {p2.Boring}, Magnetic: {p2.Magnetic}");
            bool win;
            if (player.IsComputer)
            {
                Console.WriteLine("Computer thinking...");
                var types = new List<_DiscType>();
                if (player.HasDisc(_DiscType.Ordinary)) types.Add(_DiscType.Ordinary);
                if (player.HasDisc(_DiscType.Boring)) types.Add(_DiscType.Boring);
                if (player.HasDisc(_DiscType.Magnetic)) types.Add(_DiscType.Magnetic);
                _DiscType t = types[rng.Next(types.Count)];
                int chosen;
                if (t == _DiscType.Boring)
                {
                    chosen = rng.Next(gameAlgo.Columns);
                }
                else
                {
                    var cols = new List<int>();
                    for (int c = 0; c < gameAlgo.Columns; c++)
                    {
                        if (gameAlgo.Board[gameAlgo.Rows - 1, c] == ' ') cols.Add(c);
                    }
                    if (cols.Count == 0) return;
                    chosen = cols[rng.Next(cols.Count)];
                }
                win = gameAlgo.DropDisc(player, t, chosen, true);
                if (win)
                {
                    RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
                    Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                    return;
                }
                if (gameAlgo.BoardFull())
                {
                    RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
                    Console.WriteLine("It's a draw.");
                    return;
                }
                gameAlgo.CurrentPlayerIndex = 1 - gameAlgo.CurrentPlayerIndex;
                continue;
            }

            _DiscType type;
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
                        gameAlgo.Save(parts[1]);
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
                _DiscType t = _DiscType.Ordinary;
                if (up == 'B') t = _DiscType.Boring; else if (up == 'M') t = _DiscType.Magnetic;
                int col = colInput - 1;
                if (col < 0 || col >= gameAlgo.Columns)
                {
                    Console.WriteLine($"⚠️ Invalid column. Please enter a number between 1 and {gameAlgo.Columns}.");
                    continue;
                }
                if (gameAlgo.Board[gameAlgo.Rows - 1, col] != ' ' && t != _DiscType.Boring)
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
            win = gameAlgo.DropDisc(player, type, column, true);
            if (!win && gameAlgo.BoardFull())
            {
                RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
                Console.WriteLine("It's a draw.");
                return;
            }
            if (win)
            {
                RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
                Console.WriteLine($"Player {(int)player.Id + 1} wins!");
                return;
            }
            gameAlgo.CurrentPlayerIndex = 1 - gameAlgo.CurrentPlayerIndex;
        }
    }

    static void RunTest(string sequence)
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
            _DiscType type = _DiscType.Ordinary;
            if (up == 'B') type = _DiscType.Boring; else if (up == 'M') type = _DiscType.Magnetic;
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
