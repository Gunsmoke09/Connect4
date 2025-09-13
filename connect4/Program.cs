
using Connect4;

class Program
{
    const string SaveFile = "savedgame";

    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            string sequence;
            if (args.Length > 1)
                sequence = args[1];
            else
                sequence = Console.ReadLine();
            Testing.Run(sequence);
            return;
        }

        Console.WriteLine("____Welcome to Connect4!____");
        Console.WriteLine("Load saved game? (y/n)");
        string? choice = Console.ReadLine();
        GameAlgo gameAlgo;
        if (choice != null && choice.Trim().ToLower() == "y")
        {
            if (File.Exists(SaveFile))
            {
                gameAlgo = GameAlgo.Load(SaveFile);
            }
            else
            {
                Console.WriteLine("No saved game found. Starting new game.");
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
                int chosen = -1;
                _DiscType t = _DiscType.Ordinary;

                if (player.RemDisc(_DiscType.Ordinary))
                {
                    for (int c = 0; c < gameAlgo.Columns; c++)
                    {
                        if (gameAlgo.Board[gameAlgo.Rows - 1, c] != ' ') continue;
                        int r = 0;
                        while (r < gameAlgo.Rows && gameAlgo.Board[r, c] != ' ') r++;
                        char disc = player.Id == _PlayerId.One ? '@' : '#';
                        gameAlgo.Board[r, c] = disc;
                        bool wouldWin = gameAlgo.CheckWin(r, c, player.Id);
                        gameAlgo.Board[r, c] = ' ';
                        if (wouldWin)
                        {
                            chosen = c;
                            t = _DiscType.Ordinary;
                            break;
                        }
                    }
                }

                if (chosen == -1)
                {
                    var types = new List<_DiscType>();
                    if (player.RemDisc(_DiscType.Ordinary)) types.Add(_DiscType.Ordinary);
                    if (player.RemDisc(_DiscType.Boring)) types.Add(_DiscType.Boring);
                    if (player.RemDisc(_DiscType.Magnetic)) types.Add(_DiscType.Magnetic);
                    t = types[rng.Next(types.Count)];
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
                Console.WriteLine($"Player {(int)player.Id + 1} turn. Enter move (e.g., O4, B3, M5) or 'save' or 'help':");
                string? input = Console.ReadLine();
                if (input == null) continue;
                input = input.Trim();
                if (input == "save")
                {
                    gameAlgo.Save(SaveFile);
                    Console.WriteLine("Game saved.");
                    continue;
                }
                if (input == "help")
                {
                    Console.WriteLine("Commands:");
                    Console.WriteLine("O# or 0# - ordinary disc in column #");
                    Console.WriteLine("B# - boring disc in column # (removes column)");
                    Console.WriteLine("M# - magnetic disc in column # (lifts own disc)");
                    Console.WriteLine("save - save game");
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
                _DiscType t;
                if (up == 'O' || typeChar == '0') t = _DiscType.Ordinary;
                else if (up == 'B') t = _DiscType.Boring;
                else if (up == 'M') t = _DiscType.Magnetic;
                else
                {
                    Console.WriteLine("Invalid disc type. Use O, B, or M.");
                    continue;
                }
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
                if (!player.RemDisc(t))
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

}
