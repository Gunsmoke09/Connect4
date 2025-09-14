
using System;
using System.Collections.Generic;
using Connect4;

class Program
{
    const string SaveFile = "savedgame.json";

    static void Main(string[] args)
    {
        // to enter test mode from the terminal
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

        Console.WriteLine("_____Welcome to Connect4!_____");
        Console.WriteLine("Load saved game? (y/n)");
        string choice = Console.ReadLine();
        GameAlgo gameAlgo;

        if (choice != null && choice.Trim().ToLower() == "y")
        {
            try
            {
                gameAlgo = GameAlgo.Load(SaveFile);
                Console.WriteLine($"Game loaded. Player {(int)gameAlgo.CurrentPlayer.Id + 1}'s turn.");
            }
            catch (Exception)
            {
                Console.WriteLine("⚠ Error: Could not load save file. Please try again.");
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
        int selectMode;
        
        while (true)
        {
            string s = Console.ReadLine();
            if (int.TryParse(s, out selectMode) && selectMode >= 1 && selectMode <= 2) // only accept valid input
                break;
            Console.WriteLine("Enter number between 1 and 2");
        }
        
        GameMode mode = selectMode == 1 ? GameMode.HumanVsHuman : GameMode.HumanVsComputer;

        Console.WriteLine("Enter rows (>=6):");
        int rows;
        while (true)
        {
            string s = Console.ReadLine();
            if (int.TryParse(s, out rows) && rows >= 6 && rows <= 100) //only accepting valid inputs
                break;
            Console.WriteLine("Enter number between 6 and 100");
        }
        Console.WriteLine("Enter columns (>=7 and >=rows):");
        int cols;
        while (true)
        {
            string s = Console.ReadLine();
            if (int.TryParse(s, out cols) && cols >= 7 && cols <= 100 && cols >= rows) //only accepting valid inputs
                break;
            Console.WriteLine("Columns must be >= rows and between 7 and 100");
        }
        return new GameAlgo(rows, cols, mode); //returning a new game object 
    }

    //after starting new game
    static void PlayGame(GameAlgo gameAlgo)
    {
        var rng = new Random();
        RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
        PrintStatus(gameAlgo);
        Console.WriteLine($"Connect {gameAlgo.ConnectN} to win!");
        Console.WriteLine("-------------");

        while (true)
        {
            var player = gameAlgo.CurrentPlayer;
            PlayerId? winner;

            if (player.IsComputer)
            {
                int chosen = -1;
                DiscType t = DiscType.Ordinary;

                if (player.RemDisc(DiscType.Ordinary))
                {
                    for (int c = 0; c < gameAlgo.Columns; c++)
                    {
                        if (gameAlgo.Board[gameAlgo.Rows - 1, c] != ' ') continue;
                        int r = 0;
                        while (r < gameAlgo.Rows && gameAlgo.Board[r, c] != ' ') r++;
                        char disc = player.Id == PlayerId.One ? '@' : '#';
                        gameAlgo.Board[r, c] = disc;
                        bool wouldWin = gameAlgo.CheckWin(r, c, player.Id);
                        gameAlgo.Board[r, c] = ' ';
                        if (wouldWin)
                        {
                            chosen = c;
                            t = DiscType.Ordinary;
                            break;
                        }
                    }
                }

                if (chosen == -1)
                {
                    var types = new List<DiscType>();
                    if (player.RemDisc(DiscType.Ordinary)) types.Add(DiscType.Ordinary);
                    if (player.RemDisc(DiscType.Boring)) types.Add(DiscType.Boring);
                    if (player.RemDisc(DiscType.Magnetic)) types.Add(DiscType.Magnetic);
                    t = types[rng.Next(types.Count)];
                    if (t == DiscType.Boring)
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

                winner = gameAlgo.DiscFalls(player, t, chosen);
            }
            else
            {
                DiscType type;
                int column;
                while (true)
                {
                    Console.WriteLine("Enter move (O# /B# /M# eg- O1, M7) or 'save' or 'help':");
                    try
                    {
                        string input = Console.ReadLine();
                        if (input == null) continue;
                        input = input.Trim();
                        if (input == "save")
                        {
                            try
                            {
                                gameAlgo.Save(SaveFile);
                                Console.WriteLine($"✔ Game saved as {SaveFile}.");
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("⚠ Error: Could not save game. Please try again.");
                            }
                            continue;
                        }
                        if (input == "help")
                        {
                            PrintHelp();
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
                        DiscType t;
                        if (up == 'O' || typeChar == '0') t = DiscType.Ordinary;
                        else if (up == 'B') t = DiscType.Boring;
                        else if (up == 'M') t = DiscType.Magnetic;
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
                        if (gameAlgo.Board[gameAlgo.Rows - 1, col] != ' ' && t != DiscType.Boring)
                        {
                            Console.WriteLine("!!!! That column is full. Choose another column !!!!");
                            continue;
                        }
                        if (!player.RemDisc(t))
                        {
                            Console.WriteLine($"!!!! You have no {t.ToString().ToUpperInvariant()} discs left. Choose a disc type you still have !!!!");
                            continue;
                        }
                        type = t;
                        column = col;
                        break;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("⚠ Error: Invalid input. Please try again.");
                    }
                }
                winner = gameAlgo.DiscFalls(player, type, column);
            }

            RenderGrid.PrintBoard(gameAlgo.Board, gameAlgo.Rows, gameAlgo.Columns);
            if (winner.HasValue)
            {
                Console.WriteLine($"Player {(int)winner.Value + 1} wins!");
                return;
            }
            if (gameAlgo.BoardFull())
            {
                Console.WriteLine("It's a draw.");
                return;
            }
            Console.WriteLine("------------------");
            gameAlgo.CurrentPlayerNumber = 1 - gameAlgo.CurrentPlayerNumber;
            PrintStatus(gameAlgo);
            Console.WriteLine("------------------");
            
        }
    }

    static void PrintStatus(GameAlgo gameAlgo)
    {
        Player p1 = gameAlgo.CurrentPlayer.Id == PlayerId.One ? gameAlgo.CurrentPlayer : gameAlgo.OtherPlayer;
        Player p2 = gameAlgo.CurrentPlayer.Id == PlayerId.Two ? gameAlgo.CurrentPlayer : gameAlgo.OtherPlayer;
        Console.WriteLine($"Player 1 (@) — Ordinary: {p1.Ordinary}, Boring: {p1.Boring}, Magnetic: {p1.Magnetic}");
        Console.WriteLine($"Player 2 (#) — Ordinary: {p2.Ordinary}, Boring: {p2.Boring}, Magnetic: {p2.Magnetic}");
    }

    static void PrintHelp()
    {
        Console.WriteLine("┌─────────┬──────────────────────────────────────────┐");
        Console.WriteLine("│ Command │ Description                              │");
        Console.WriteLine("├─────────┼──────────────────────────────────────────┤");
        Console.WriteLine("│ O# or 0#│ Drop ordinary disc in column #           │");
        Console.WriteLine("│ B#      │ Boring disc in column # (removes column) │");
        Console.WriteLine("│ M#      │ Magnetic disc in column # (lifts own)    │");
        Console.WriteLine("│ save    │ Save game                                │");
        Console.WriteLine("│ help    │ Show this help                           │");
        Console.WriteLine("│ exit    │ Quit                                     │");
        Console.WriteLine("└─────────┴──────────────────────────────────────────┘");
    }

}
