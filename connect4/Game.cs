using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Connect4
{
    public class Game
    {
        public int Rows { get; }
        public int Columns { get; }
        public int ConnectN { get; }
        public GameMode Mode { get; }

        private readonly char[,] board;
        private readonly Player[] players = new Player[2];
        private int currentPlayerIndex = 0;
        private readonly Random rng = new Random();

        public Game(int rows, int columns, GameMode mode)
        {
            Rows = rows;
            Columns = columns;
            Mode = mode;
            ConnectN = 4;
            board = new char[Rows, Columns];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    board[r, c] = ' ';

            int totalCells = rows * columns;
            int perPlayer = totalCells / 2;
            int specials = 2; // Boring and Magnetic
            int ordinary = perPlayer - specials * 2; // two of each special
            players[0] = new Player(PlayerId.One, false, ordinary, 2, 2);
            players[1] = new Player(PlayerId.Two, mode == GameMode.HumanVsComputer, ordinary, 2, 2);
        }

        public Player CurrentPlayer => players[currentPlayerIndex];
        public Player OtherPlayer => players[1 - currentPlayerIndex];

        public void SwitchPlayer() => currentPlayerIndex = 1 - currentPlayerIndex;

        public void DisplayBoard()
        {
            for (int r = Rows - 1; r >= 0; r--)
            {
                var sb = new StringBuilder();
                for (int c = 0; c < Columns; c++)
                {
                    sb.Append('|');
                    sb.Append(' ');
                    sb.Append(board[r, c]);
                    sb.Append(' ');
                }
                sb.Append('|');
                Console.WriteLine(sb.ToString());
            }
        }

        public bool ColumnFull(int column)
        {
            return board[Rows - 1, column] != ' ';
        }

        public bool DropDisc(Player player, DiscType type, int column, bool showFrames)
        {
            if (column < 0 || column >= Columns) return false;
            if (column < 0 || column >= Columns) return false;
            if (!player.HasDisc(type)) return false;

            int row = -1;
            for (int r = 0; r < Rows; r++)
            {
                if (board[r, column] == ' ')
                {
                    row = r;
                    break;
                }
            }
            if (row == -1)
            {
                if (type != DiscType.Boring) return false;
                row = Rows - 1;
            }

            char discChar = ' ';
            if (type == DiscType.Ordinary)
            {
                discChar = player.Id == PlayerId.One ? 'X' : 'O';
            }
            else if (type == DiscType.Boring)
            {
                discChar = player.Id == PlayerId.One ? 'B' : 'b';
            }
            else if (type == DiscType.Magnetic)
            {
                discChar = player.Id == PlayerId.One ? 'M' : 'm';
            }

            board[row, column] = discChar;
            player.UseDisc(type);
            if (showFrames) DisplayBoard();

            if (type == DiscType.Boring)
            {
                ApplyBoring(player, column, row);
                if (showFrames) DisplayBoard();
                board[0, column] = player.Id == PlayerId.One ? 'X' : 'O';
                row = 0;
            }
            else if (type == DiscType.Magnetic)
            {
                ApplyMagnetic(player, column, row);
                if (showFrames) DisplayBoard();
                board[row, column] = player.Id == PlayerId.One ? 'X' : 'O';
            }

            bool win = CheckWin(row, column, player.Id);
            return win;
        }

        private void ApplyBoring(Player player, int column, int row)
        {
            for (int r = 0; r < row; r++)
            {
                char existing = board[r, column];
                if (existing == 'X' || existing == 'O' || existing == 'B' || existing == 'b' || existing == 'M' || existing == 'm')
                {
                    PlayerId owner = existing == 'X' || existing == 'B' || existing == 'M' ? PlayerId.One : PlayerId.Two;
                    players[(int)owner].Ordinary++;
                }
                board[r, column] = ' ';
            }
            board[row, column] = ' ';
            char boringChar = player.Id == PlayerId.One ? 'B' : 'b';
            board[0, column] = boringChar;
        }

        private void ApplyMagnetic(Player player, int column, int row)
        {
            int target = -1;
            for (int r = row - 1; r >= 0; r--)
            {
                char ch = board[r, column];
                bool mine = player.Id == PlayerId.One ? (ch == 'X' || ch == 'B' || ch == 'M') : (ch == 'O' || ch == 'b' || ch == 'm');
                if (mine)
                {
                    target = r;
                    break;
                }
            }
            if (target != -1 && target < row - 1)
            {
                char below = board[target, column];
                char above = board[target + 1, column];
                board[target, column] = above;
                board[target + 1, column] = below;
            }
        }

        public bool CheckWin(int row, int column, PlayerId player)
        {
            int[][] dirs = new int[][]
            {
                new[] {1, 0}, // vertical
                new[] {0, 1}, // horizontal
                new[] {1, 1}, // diag1
                new[] {1, -1} // diag2
            };
            foreach (var d in dirs)
            {
                int count = 1;
                int r = row + d[0];
                int c = column + d[1];
                while (r >= 0 && r < Rows && c >= 0 && c < Columns)
                {
                    char ch = board[r, c];
                    bool match = player == PlayerId.One ? ch == 'X' || ch == 'B' || ch == 'M' : ch == 'O' || ch == 'b' || ch == 'm';
                    if (match) count++; else break;
                    r += d[0];
                    c += d[1];
                }
                r = row - d[0];
                c = column - d[1];
                while (r >= 0 && r < Rows && c >= 0 && c < Columns)
                {
                    char ch = board[r, c];
                    bool match = player == PlayerId.One ? ch == 'X' || ch == 'B' || ch == 'M' : ch == 'O' || ch == 'b' || ch == 'm';
                    if (match) count++; else break;
                    r -= d[0];
                    c -= d[1];
                }
                if (count >= ConnectN) return true;
            }
            return false;
        }

        public bool BoardFull()
        {
            for (int c = 0; c < Columns; c++)
                if (board[Rows - 1, c] == ' ') return false;
            return true;
        }

        public void Save(string filename)
        {
            var state = new GameState
            {
                Rows = Rows,
                Columns = Columns,
                Board = Enumerable.Range(0, Rows).Select(r =>
                    new string(Enumerable.Range(0, Columns).Select(c => board[r, c]).ToArray())).ToArray(),
                Players = players.Select(p => new PlayerState
                {
                    Id = p.Id,
                    IsComputer = p.IsComputer,
                    Ordinary = p.Ordinary,
                    Boring = p.Boring,
                    Magnetic = p.Magnetic
                }).ToArray(),
                CurrentPlayer = currentPlayerIndex,
                Mode = Mode
            };
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }

        public static Game Load(string filename)
        {
            var json = File.ReadAllText(filename);
            var state = JsonSerializer.Deserialize<GameState>(json);
            if (state == null) throw new Exception("Invalid save file");
            var game = new Game(state.Rows, state.Columns, state.Mode);
            for (int r = 0; r < state.Rows; r++)
                for (int c = 0; c < state.Columns; c++)
                    game.board[r, c] = state.Board[r][c];
            for (int i = 0; i < 2; i++)
            {
                game.players[i].Ordinary = state.Players[i].Ordinary;
                game.players[i].Boring = state.Players[i].Boring;
                game.players[i].Magnetic = state.Players[i].Magnetic;
                game.players[i] = new Player(state.Players[i].Id, state.Players[i].IsComputer, state.Players[i].Ordinary, state.Players[i].Boring, state.Players[i].Magnetic);
            }
            game.currentPlayerIndex = state.CurrentPlayer;
            return game;
        }

        // Computer move choosing
        public bool ComputerTurn()
        {
            var player = CurrentPlayer;
            var validCols = new List<int>();
            for (int c = 0; c < Columns; c++)
            {
                if (board[Rows - 1, c] == ' ')
                {
                    if (player.HasDisc(DiscType.Ordinary)) validCols.Add(c);
                }
            }
            int chosen = -1;
            foreach (var col in validCols)
            {
                int row = -1;
                for (int r = 0; r < Rows; r++)
                {
                    if (board[r, col] == ' ')
                    {
                        row = r;
                        break;
                    }
                }
                char tmp = player.Id == PlayerId.One ? 'X' : 'O';
                board[row, col] = tmp;
                bool win = CheckWin(row, col, player.Id);
                board[row, col] = ' ';
                if (win)
                {
                    chosen = col;
                    break;
                }
            }
            if (chosen == -1 && validCols.Count > 0)
            {
                chosen = validCols[rng.Next(validCols.Count)];
            }
            if (chosen != -1)
            {
                return DropDisc(player, DiscType.Ordinary, chosen, true);
            }
            return false;
        }

    }
}
