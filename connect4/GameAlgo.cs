
using System.Text.Json; 

namespace Connect4
{
    public class GameAlgo
    {
        public int Rows { get; }
        public int Columns { get; }
        public GameMode Mode { get; }
        private readonly char[,] board;
        
        private readonly Player[] players = new Player[2]; 
        public int CurrentPlayerNumber = 0;
        private readonly Random rng = new Random();

        public GameAlgo(int rows, int columns, GameMode mode)
        {
            Rows = rows;
            Columns = columns;
            Mode = mode;
            board = RenderGrid.GenBoard(Rows, Columns);

            int total_cells = rows * columns;
            int discsPerplayer = total_cells / 2;
            int ordinary = discsPerplayer - (4); // two of each special
            players[0] = new Player(PlayerId.One, false, ordinary, 2, 2);
            players[1] = new Player(PlayerId.Two, mode == GameMode.HumanVsComputer, ordinary, 2, 2);
        }

        public Player CurrentPlayer
        {
            get
            {
                return players[CurrentPlayerNumber];
            }
        }

        public Player OtherPlayer
        {
            get
            {
                return players[1 - CurrentPlayerNumber]; //this formula always returns the other player
            }
        }

        public char[,] Board => board;

        public bool DiscFalls(Player player, DiscType type, int column, bool wantsPrint)
        {
            if (column < 0 || column >= Columns) //boundary check on columns
                return false;
            
            if (!player.RemDisc(type)) return false; // checking if the player has remaining dics or not

            int row = -1; //assuming the column is already full by default
            
            for (int r = 0; r < Rows; r++)
            {
                if (board[r, column] == ' ')
                {
                    row = r; //found the empty row to put the disc in!
                    break;
                }
            }
            if (row == -1)
            {
                //disc won;t fall unless it is a boring disc
                if (type != DiscType.Boring) 
                    return false;
                row = Rows - 1; //if it is boring then we just remove the top disc and insert it there
            }

            char discChar = ' '; //need to be initialised before accessing
            if (type == DiscType.Ordinary)
            {
                discChar = player.Id == PlayerId.One ? '@' : '#'; //for player id one symbol = @ else #
            }
            else if (type == DiscType.Boring)
            {
                discChar = 'B';
            }
            else if (type == DiscType.Magnetic)
            {
                discChar = 'M';
            }

            board[row, column] = discChar; //placing that disc in the respective cell
            player.UseDisc(type); //subtracting the inventory of discs from the player
            if (wantsPrint) //if we want to print the grid or not
                RenderGrid.PrintBoard(board, Rows, Columns); 

            if (type == DiscType.Boring)
            {
                for (int r = 0; r < row; r++)
                {
                    char d = board[r, column];
                    //even though it will be changed in the next step, yet adding 'd', 'm', 'b' etc for safety
                    if (d == '@' || d == '#' || d == 'B' || d == 'b' || d == 'M' || d == 'm') 
                    {
                        PlayerId owner;
                             if(d == '@')
                                 owner = PlayerId.One;
                             else
                                 owner = PlayerId.Two;
                        players[(int)owner].Ordinary++; //only ordinary is returned, specials discs are not returned
                    }
                    board[r, column] = ' ';  //replacing each with blank spaces
                }
                board[row, column] = ' '; // it was where B landed now it is cleared for B to go down

                board[0, column] = 'B';
                if (wantsPrint) RenderGrid.PrintBoard(board, Rows, Columns); //prints with the 'B' disc at the bottom
                
                board[0, column] = player.Id == PlayerId.One ? '@' : '#'; //now the 'B' disc is converted to ordinary disc
                row = 0;
            }
            else if (type == DiscType.Magnetic)
            {
                int target = -1;
                for (int r = row - 1; r >= 0; r--)
                {
                    char ch = board[r, column];
                    //finding the respective disc
                    if (player.Id == PlayerId.One && ch == '@')
                    {
                        target = r;
                        break;
                    }
                    if (player.Id == PlayerId.Two && ch == '#')
                    {
                        target = r;
                        break;
                    }
                }
                if (target != -1 && target < row - 1)
                {
                    //algorithm for magnet
                    char below = board[target, column];
                    char above = board[target + 1, column];
                    board[target, column] = above;
                    board[target + 1, column] = below;
                }
                if (wantsPrint) RenderGrid.PrintBoard(board, Rows, Columns);
                board[row, column] = player.Id == PlayerId.One ? '@' : '#';
                //printing the grid then changing the special disc back to the ordinary
            }


            return (CheckWin(row, column, player.Id));
        }

        public bool CheckWin(int row, int column, PlayerId player)
        {
            int count = 1;
            int r = row + 1;
            while (r < Rows)
            {
                //currently it is not checking beyond the first one, it will not work for magnetic special case
                char ch = board[r, column];
                bool match = player == PlayerId.One ? ch == '@' : ch == '#';
                if (match) 
                    count++; 
                else 
                    break;
                r++;
            }
            // vertical check up to down
            r = row - 1;
            while (r >= 0)
            {
                char ch = board[r, column];
                bool match = player == PlayerId.One ? ch == '@' : ch == '#';
                if (match) count++; else break;
                r--;
            }
            if (count >= 4) return true; 
            // horizontal check RTL
            count = 1;
            int c = column + 1;
            while (c < Columns)
            {
                char ch = board[row, c];
                bool match = player == PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                c++;
            }
            //hc ltr
            c = column - 1;
            while (c >= 0)
            {
                char ch = board[row, c];
                bool match = player == PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                c--;
            }
            if (count >= 4) return true;
        // diag c down-right
            count = 1;
            r = row + 1;
            c = column + 1;
            while (r < Rows && c < Columns)
            {
                char ch = board[r, c];
                bool match = player == PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r++;
                c++;
            }
            // diag c up left
            r = row - 1;
            c = column - 1;
            while (r >= 0 && c >= 0)
            {
                char ch = board[r, c];
                bool match = player == PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r--;
                c--;
            }
            // diagg c down left
            if (count >= 4) return true;

            count = 1;
            r = row + 1;
            c = column - 1;
            while (r < Rows && c >= 0)
            {
                char ch = board[r, c];
                bool match = player == PlayerId.One ? ch == '@' || ch == 'B' || ch == 'M' : ch == '#' || ch == 'b' || ch == 'm';
                if (match) count++; else break;
                r++;
                c--;
            }
            // diag c up right
            r = row - 1;
            c = column + 1;
            while (r >= 0 && c < Columns)
            {
                char ch = board[r, c];
                bool match = player == PlayerId.One ? ch == '@' : ch == '#';
                if (match) count++; else break;
                r--;
                c++;
            }
            return count >= 4;
        }

        public bool BoardFull()
        {
            for (int c = 0; c < Columns; c++)
                if (board[Rows - 1, c] == ' ')
                    return false;
            return true;
        }

        public void Save(string filename)
        {
            
            string[] oneRow = new string[Rows];
            for (int r = 0; r < Rows; r++)
            {
                char[] chars = new char[Columns]; //creating array of characters
                for (int c = 0; c < Columns; c++)
                {
                    chars[c] = board[r, c]; //storing each element of the board in the array of characters
                }
                oneRow[r] = new string(chars); //using that array as a row entry for array of strings
            }

            PlayerStatusBar[] playerStatus = new PlayerStatusBar[2];
            for (int i = 0; i < 2; i++)
            {
                playerStatus[i] = new PlayerStatusBar
                {
                    Id = players[i].Id,
                    IsComputer = players[i].IsComputer,
                    Ordinary = players[i].Ordinary,
                    Boring = players[i].Boring,
                    Magnetic = players[i].Magnetic
                };
            }

            var state = new GameStatus
            {
                Rows = Rows,
                Columns = Columns,
                Board = oneRow,
                Players = playerStatus,
                CurrentPlayer = CurrentPlayerNumber,
                Mode = Mode
            };
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }

        public static GameAlgo Load(string filename)
        {
            var json = File.ReadAllText(filename);
            var state = JsonSerializer.Deserialize<GameStatus>(json);
            if (state == null) 
                throw new Exception("Invalid save file");
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
            game.CurrentPlayerNumber = state.CurrentPlayer;
            return game;
        }

    }
}
