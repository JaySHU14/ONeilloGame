using ONeilloGame.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic; // import List type
using System.Diagnostics; // import diagnostics for debugging. invalid move fix

namespace ONeilloGame
{
    public partial class Form1 : Form
    {
        private const int BoardSize = 8; // define the size of the game board. in our case it is 8x8
        private const int CellSize = 126; // define the size of every cell in pixels
        private PictureBox[,] board; // define a 2D array of picture box controls for the board

        public Form1()
        {
            InitializeComponent(); // initialise the form itself
            InitialiseBoard(); // then, initialise the board
        }

        private void InitialiseBoard()
        {
            board = new PictureBox[BoardSize, BoardSize]; // use pictureboxes to set every cell of the board. this means that i can add the images provided for the squares
            tableLayoutPanel.RowStyles.Clear(); // clear any rowstyles that may currently be active on the board
            tableLayoutPanel.ColumnStyles.Clear(); // clear any columnstyles that may currently be active on the board
            tableLayoutPanel.RowCount = BoardSize; // set the number of rows in the table layout
            tableLayoutPanel.ColumnCount = BoardSize; // set the number of columns in the table layout
            tableLayoutPanel.AutoSize = true; // automatically adjust the size of the table lauout

            for (int row = 0; row < BoardSize; row++) // create the game board, implementing the picture box controls throughout
            {
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, CellSize)); // add a new row style that will be implemented for the specific cell size
                for (int col = 0; col < BoardSize; col++) // loop through each column in the same way we are looping through each row
                {
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CellSize)); 
                    board[row, col] = new PictureBox // create a picture box control for the board
                    {
                        Size = new Size(CellSize, CellSize), // manually set the size for the picture box
                        Dock = DockStyle.Fill, // dock the picture box to fit its' container
                        Tag = new Tuple<int, int>(row, col), // store the information for the rows and columns as a tag
                        Image = Resources.empty_square, // set the initial image to an empty square for the picture box
                        Cursor = Cursors.Hand // set the cursor to the hand icon while hovering over the box
                    };
                    board[row, col].Click += BoardCellClick; // attach the click event handler to the box upon click
                    tableLayoutPanel.Controls.Add(board[row, col], col, row); // add the picture box to the table layout
                }
            }

            // INITIALISE STARTING COUNTERS
            SetCounter(BoardSize / 2 - 1, BoardSize / 2 - 1, Resources.white_on_square);
            SetCounter(BoardSize / 2, BoardSize / 2 - 1, Resources.black_on_square);
            SetCounter(BoardSize / 2 - 1, BoardSize / 2, Resources.black_on_square);
            SetCounter(BoardSize / 2, BoardSize / 2, Resources.white_on_square);
        }

        private void SetCounter(int row, int col, Image image)
        {
            if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize)
            {
                board[row, col].Image = image;
            }
        }

        private void BoardCellClick(object sender, EventArgs e)
        {
            PictureBox clickedCell = (PictureBox)sender;
            Tuple<int, int> cellPosition = (Tuple<int, int>)clickedCell.Tag;
            int row = cellPosition.Item1;
            int col = cellPosition.Item2;

            Debug.WriteLine($"Clicked cell: Row {row}, Col {col}");

            if (ValidMove(row, col))
            {
                Debug.WriteLine("Valid move detected. ");
                MakeMove(row, col); // if a valid move, move the counter 
                // SwitchTurn();
                // UpdateScores();

                // BlackTurn = !BlackTurn; // switch players
            }
            else
            {
                Debug.WriteLine("Invalid move detected. ");
                MessageBox.Show("Invalid move. Try again."); // if the move isn't valid, inform the user
            }
        }

        // DECLARE WHETHER MOVE IS VALID
        private bool ValidMove(int row, int col)
        {
            if (board[row, col].Image != Resources.empty_square)
            {
                return false; // if the current position of the move is not empty then do not make the move
            }

            int[] directionalRow = { -1, -1, -1, 0, 1, 1, 1, 0 }; // this will check every direction a counter can move to see if it can actually move there or not
            int[] directionalCol = { -1, 0, 1, 1, 1, 0, -1, -1 };

            for (int i = 0; i < 8; i++)
            {
                int r = row + directionalRow[i];
                int c = col + directionalCol[i];
                bool opponentFound = false;

                while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize) // iterate over every element of the board array
                {
                    if (board[r, c].Image == Resources.empty_square)
                    {
                        break; // if the current iteration contains no counter before your own then the move is not valid
                    }

                    else if (board[r, c].Image == OpponentCounter())
                    {
                        opponentFound = true; // if you find an opponent's counter before your own then continue the search as you cannot outright capture the counter next to you if it is an opposition colour
                    }

                    else if (opponentFound && board[r, c].Image == CurrentPCounter()) // if you find your own counter after you have found an opponent counter, it is a valid move
                    {
                        return true;
                    }

                    r += directionalRow[i]; //
                    c += directionalCol[i]; //
                }
            }

            return false;
        }

        private Image OpponentCounter()
        {
            return BlackTurn ? Resources.white_on_square : Resources.black_on_square;
        }

        private Image CurrentPCounter()
        {
            return BlackTurn ? Resources.black_on_square : Resources.white_on_square;
        }

        private bool BlackTurn = true; // set black to be first to play, since mattel states that black should always start first

        // MAKING THE COUNTERS MOVE
        private void MakeMove(int row, int col)
        {
            board[row, col].Image = CurrentPCounter(); // place the current player's counter to the cell that has been clicked

            int[] directionRow = { -1, -1, -1, 0, 1, 1, 1, 0 };
            int[] directionCol = { -1, 0, 1, 1, 1, 0, -1, -1 };

            for (int i = 0; i < 8; i++)
            {
                int r = row  + directionRow[i];
                int c = col + directionCol[i];
                bool foundOpponent = false;
                List<PictureBox> flipCounter = new List<PictureBox>();

                while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize)
                {
                    if (board[r, c].Image == Resources.empty_square)
                    {
                        break; //
                    }
                    else if (board[r, c].Image == OpponentCounter())
                    {
                        foundOpponent = true;
                        flipCounter.Add(board[r, c]);
                    }
                    else if (foundOpponent && board[r, c].Image == CurrentPCounter())
                    {
                        foreach(var counterFlip in flipCounter)
                        {
                            counterFlip.Image = CurrentPCounter();
                        }
                        break;
                    }

                    r += directionRow[i];
                    c += directionCol[i];
                }
            }
        }

        

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            InitialiseBoard();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // this.Size = new Size(800, 500);
            this.Size = new Size(BoardSize * CellSize + 20, BoardSize * CellSize + 60); // set the form window size, allow for some space 
        }

    }
}

// view properties min max box set to false

// TO DO - change pixels of images, try to get them to match the size of the box and hopefully therefore fix last row stretching
//         fix valid move error. 