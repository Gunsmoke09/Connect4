using System;
using System.Text;

namespace Connect4
{
    public static class GridHelper
    {
        public static char[,] CreateBoard(int rows, int columns)
        {
            var board = new char[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    board[r, c] = ' ';
                }
            }
            return board;
        }

        public static void Display(char[,] board, int rows, int columns)
        {
            for (int r = rows - 1; r >= 0; r--)
            {
                var sb = new StringBuilder();
                for (int c = 0; c < columns; c++)
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
    }
}
