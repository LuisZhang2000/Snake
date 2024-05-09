using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; } // number of rows in the grid
        public int Cols { get; } // number of columns in the grid
        public GridValue[,] Grid { get; } // the grid itself (2D rectangular array of grid values)
        public Direction Dir { get; private set; } // direction of snake which determines where it moves next
        public int Score { get; private set; } 
        public bool GameOver { get; private set; }
        
        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();

        // a list of positions currently occupied by the snake
        // linked list allows us to add/delete from both ends of the list
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        
        private Random random = new Random(); // random object is used to spawn the food in a random position

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }

        /** 
         * AddSnake() is a method used to add the snake to the grid when the game starts
         * The snake initially starts in the middle row from column 1 to 3 (a 3 block long snake) 
         */
        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        /**
         * EmptyPositions() loops through all rows and columns and checks if the position is empty
         * and yield returns the empty positions (i.e. returns the sequence of empty positions one at a time)
         */
        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        /** 
         * AddFood() is a method used to add food to a random empty position within the grid 
         */
        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            // if there are no empty positions, that means the player has beat the game
            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;

        }

        /* 
         * More snake related helper methods - the public methods will be used for visual purposes
         * HeadPosition() to add eyes to the snake
         * TailPosition() to add tail to the snake
         * SnakePositions() to change the colour of the snake when it dies
         */

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        /**
         * Helper methods for moving the snake
         * AddHead() and RemoveTail() will be useful for when moving the snake
         * OutsideGrid() checks if a given position is outside the grid
         * WillHit() takes the position of the new head and returns the grid value of the position that the new head will hit 
         */


        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;   // set the GridValue enum of the new head position to snake
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty; // set the GridValue enum of the tail position to empty
            snakePositions.RemoveLast();
        }

        // GetLastDirection() returns the snake's last predetermined direction
        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            // if there are already 2 direction changes stored in the buffer, the buffer is considered to be full
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            // if a change can be made, add the change to a buffer
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Snake || hit == GridValue.Outside)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);       
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }


        }
    }
}
