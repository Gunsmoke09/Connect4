using System; // only need System now

namespace Connect4
{
    public static class RenderGrid
    {
        public static char[,] GenBoard(int rows, int cols) // two-dimensional array of chars
        {
            var board = new char[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    board[r, c] = ' '; // empty cells to start with
                }
            }
            return board;
        }

        public static void Display(char[,] board, int rows, int columns)
        {
            for (int r = rows - 1; r >= 0; r--) // print from top row to bottom
            {
                for (int c = 0; c < columns; c++)
                {
                    Console.Write("| ");
                    Console.Write(board[r, c]);
                    Console.Write(" ");
                }
                Console.WriteLine("|"); // end of row
            }

            // column number at the bottom for easy placement 
            Console.Write("  "); 
            for (int c = 1; c <= columns; c++)
            {
                Console.Write(c + "   ");
            }
            Console.WriteLine();
        }
    }
}