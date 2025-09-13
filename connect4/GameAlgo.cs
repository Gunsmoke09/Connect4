using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Connect4
{
    public class GameAlgo
    {
        public int Rows { get; }
        public int Columns { get; }
        public int ConnectN { get; }
        public _GameMode Mode { get; }

        private readonly char[,] board;
        private readonly Player[] players = new Player[2];
        public int CurrentPlayerIndex = 0;
        private readonly Random rng = new Random();

        public GameAlgo(int rows, int columns, _GameMode mode)
        {
            Rows = rows;
            Columns = columns;
            Mode = mode;
            ConnectN = 4;
            board = RenderGrid.GenBoard(Rows, Columns);

            int totalCells = rows * columns;
            int perPlayer = totalCells / 2;
            int specials = 2; // Boring and Magnetic
            int ordinary = perPlayer - specials * 2; // two of each special
            players[0] = new Player(_PlayerId.One, false, ordinary, 2, 2);
            players[1] = new Player(_PlayerId.Two, mode == _GameMode.HumanVsComputer, ordinary, 2, 2);
        }

        public Player CurrentPlayer => players[CurrentPlayerIndex];
        public Player OtherPlayer => players[1 - CurrentPlayerIndex];
        public char[,] Board => board;

        public bool DropDisc(Player player, _DiscType type, int column, bool showFrames)
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
                if (type != _DiscType.Boring) return false;
                row = Rows - 1;
            }

            char discChar = ' ';
            if (type == _DiscType.Ordinary)
            {
                discChar = player.Id == _PlayerId.One ? '@' : '#';
            }
            else if (type == _DiscType.Boring)
            {
                discChar = player.Id == _PlayerId.One ? 'B' : 'b';
            }
            else if (type == _DiscType.Magnetic)
            {
                discChar = player.Id == _PlayerId.One ? 'M' : 'm';
            }

            board[row, column] = discChar;
            player.UseDisc(type);
            if (showFrames) RenderGrid.Display(board, Rows, Columns);

            if (type == _DiscType.Boring)
            {
                for (int r = 0; r < row; r++)
                {
                    char existing = board[r, column];
                    if (existing == '@' || existing == '#' || existing == 'B' || existing == 'b' || existing == 'M' || existing == 'm')
                    {
                        _PlayerId owner = existing == '@' || existing == 'B' || existing == 'M' ? _PlayerId.One : _PlayerId.Two;
                        players[(int)owner].Ordinary++;
                    }
                    board[r, column] = ' ';
                }
                board[row, column] = ' ';
                char boringChar = player.Id == _PlayerId.One ? 'B' : 'b';
                board[0, column] = boringChar;
                if (showFrames) RenderGrid.Display(board, Rows, Columns);
                board[0, column] = player.Id == _PlayerId.One ? '@' : '#';
                row = 0;
            }
            else if (type == _DiscType.Magnetic)
            {
                int target = -1;
                for (int r = row - 1; r >= 0; r--)
                {
                    char ch = board[r, column];
                    bool mine = player.Id == _PlayerId.One ? (ch == '@' || ch == 'B' || ch == 'M') : (ch == '#' || ch == 'b' || ch == 'm');
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
                if (showFrames) RenderGrid.Display(board, Rows, Columns);
                board[row, column] = player.Id == _PlayerId.One ? '@' : '#';
            }

            bool win = CheckWin(row, column, player.Id);
            return win;
        }

        public bool CheckWin(int row, int column, _PlayerId player)
        {
            int count = 1;
            int r = row + 1;
            while (r < Rows)
            {
                char ch = board[r, column];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r++;
            }
            r = row - 1;
            while (r >= 0)
            {
                char ch = board[r, column];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r--;
            }
            if (count >= ConnectN) return true;

            count = 1;
            int c = column + 1;
            while (c < Columns)
            {
                char ch = board[row, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                c++;
            }
            c = column - 1;
            while (c >= 0)
            {
                char ch = board[row, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                c--;
            }
            if (count >= ConnectN) return true;

            count = 1;
            r = row + 1;
            c = column + 1;
            while (r < Rows && c < Columns)
            {
                char ch = board[r, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r++;
                c++;
            }
            r = row - 1;
            c = column - 1;
            while (r >= 0 && c >= 0)
            {
                char ch = board[r, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r--;
                c--;
            }
            if (count >= ConnectN) return true;

            count = 1;
            r = row + 1;
            c = column - 1;
            while (r < Rows && c >= 0)
            {
                char ch = board[r, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r++;
                c--;
            }
            r = row - 1;
            c = column + 1;
            while (r >= 0 && c < Columns)
            {
                char ch = board[r, c];
                bool match = player == _PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r--;
                c++;
            }
            return count >= ConnectN;
        }

        public bool BoardFull()
        {
            for (int c = 0; c < Columns; c++)
                if (board[Rows - 1, c] == ' ') return false;
            return true;
        }

        public void Save(string filename)
        {
            string[] boardStrings = new string[Rows];
            for (int r = 0; r < Rows; r++)
            {
                char[] chars = new char[Columns];
                for (int c = 0; c < Columns; c++)
                {
                    chars[c] = board[r, c];
                }
                boardStrings[r] = new string(chars);
            }

            PlayerState[] playerStates = new PlayerState[2];
            for (int i = 0; i < 2; i++)
            {
                playerStates[i] = new PlayerState
                {
                    Id = players[i].Id,
                    IsComputer = players[i].IsComputer,
                    Ordinary = players[i].Ordinary,
                    Boring = players[i].Boring,
                    Magnetic = players[i].Magnetic
                };
            }

            var state = new GameState
            {
                Rows = Rows,
                Columns = Columns,
                Board = boardStrings,
                Players = playerStates,
                CurrentPlayer = CurrentPlayerIndex,
                Mode = Mode
            };
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }

        public static GameAlgo Load(string filename)
        {
            var json = File.ReadAllText(filename);
            var state = JsonSerializer.Deserialize<GameState>(json);
            if (state == null) throw new Exception("Invalid save file");
            var game = new GameAlgo(state.Rows, state.Columns, state.Mode);
            for (int r = 0; r < state.Rows; r++)
            {
                for (int c = 0; c < state.Columns; c++)
                {
                    game.board[r, c] = state.Board[r][c];
                }
            }
            for (int i = 0; i < 2; i++)
            {
                game.players[i] = new Player(
                    state.Players[i].Id,
                    state.Players[i].IsComputer,
                    state.Players[i].Ordinary,
                    state.Players[i].Boring,
                    state.Players[i].Magnetic);
            }
            game.CurrentPlayerIndex = state.CurrentPlayer;
            return game;
        }

    }
}
