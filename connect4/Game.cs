using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace LineUp
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
            players[0] = new Player(PlayerId.One, mode == GameMode.HumanVsComputer ? false : false, ordinary, 2, 2);
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

        private char CharFor(Player player, DiscType type)
        {
            bool p1 = player.Id == PlayerId.One;
            return type switch
            {
                DiscType.Ordinary => p1 ? '@' : '#',
                DiscType.Boring => p1 ? 'B' : 'b',
                DiscType.Magnetic => p1 ? 'M' : 'm',
                _ => ' '
            };
        }

        private bool IsPlayerChar(char c, PlayerId player)
        {
            return player == PlayerId.One ? c == '@' || c == 'B' || c == 'M' : c == '#' || c == 'b' || c == 'm';
        }

        public bool ColumnFull(int column)
        {
            return board[Rows - 1, column] != ' ';
        }

        private int FindRow(int column)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (board[r, column] == ' ')
                    return r;
            }
            return -1;
        }

        public bool DropDisc(Player player, DiscType type, int column, bool showFrames)
        {
            if (column < 0 || column >= Columns) return false;
            if (!player.HasDisc(type)) return false;
            if (ColumnFull(column)) return false;

            int row = FindRow(column);
            char discChar = CharFor(player, type);
            board[row, column] = discChar;
            player.UseDisc(type);
            if (showFrames) DisplayBoard();

            if (type == DiscType.Boring)
            {
                ApplyBoring(player, column, row);
                if (showFrames) DisplayBoard();
                row = 0;
            }
            else if (type == DiscType.Magnetic)
            {
                ApplyMagnetic(player, column, row);
                if (showFrames) DisplayBoard();
            }

            bool win = CheckWin(row, column, player.Id);
            return win;
        }

        private void ApplyBoring(Player player, int column, int row)
        {
            for (int r = 0; r < row; r++)
            {
                char existing = board[r, column];
                if (existing == '@' || existing == '#' || existing == 'B' || existing == 'b' || existing == 'M' || existing == 'm')
                {
                    PlayerId owner = existing == '@' || existing == 'B' || existing == 'M' ? PlayerId.One : PlayerId.Two;
                    players[(int)owner].Ordinary++;
                }
                board[r, column] = ' ';
            }
            board[row, column] = ' ';
            board[0, column] = CharFor(player, DiscType.Boring);
        }

        private void ApplyMagnetic(Player player, int column, int row)
        {
            int target = -1;
            for (int r = row - 1; r >= 0; r--)
            {
                if (IsPlayerChar(board[r, column], player.Id))
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
                count += CountDir(row, column, d[0], d[1], player);
                count += CountDir(row, column, -d[0], -d[1], player);
                if (count >= ConnectN) return true;
            }
            return false;
        }

        private int CountDir(int row, int col, int dr, int dc, PlayerId player)
        {
            int r = row + dr;
            int c = col + dc;
            int count = 0;
            while (r >= 0 && r < Rows && c >= 0 && c < Columns)
            {
                if (IsPlayerChar(board[r, c], player)) count++; else break;
                r += dr; c += dc;
            }
            return count;
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
                if (!ColumnFull(c) && player.HasDisc(DiscType.Ordinary)) validCols.Add(c);
            }
            int chosen = -1;
            foreach (var col in validCols)
            {
                int row = FindRow(col);
                board[row, col] = CharFor(player, DiscType.Ordinary);
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
