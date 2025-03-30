using System;
using System.Collections.Generic;
using System.IO;

namespace LaivuMusis
{
	public class GameBoard
	{
		private readonly int size;
		private readonly char[,] board;
		private readonly List<Ship> ships;

		public GameBoard(int size)
		{
			if (size < 8 || size > 20)
			{
				throw new ArgumentException("Board size must be between 8 and 20");
			}

			this.size = size;
			this.board = new char[size, size];
			this.ships = new List<Ship>();
			InitializeBoard();
		}

		private void InitializeBoard()
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					board[i, j] = '.';
				}
			}
		}

		public bool CanPlaceShip(Ship ship)
		{
			var coordinates = ship.GetAllCoordinates();

			foreach (var coord in coordinates)
			{
				if (!IsValidCoordinate(coord))
				{
					return false;
				}

				var (row, col) = ParseCoordinates(coord);
				if (board[row, col] != '.')
				{
					return false;
				}

				// Check surrounding cells for other ships
				if (HasAdjacentShip(row, col))
				{
					return false;
				}
			}

			return true;
		}

		public void PlaceShip(Ship ship)
		{
			if (!CanPlaceShip(ship))
			{
				throw new InvalidOperationException($"Cannot place ship at position {ship.StartCoordinates} with size {ship.Size}");
			}

			var coordinates = ship.GetAllCoordinates();
			foreach (var coord in coordinates)
			{
				var (row, col) = ParseCoordinates(coord);
				board[row, col] = ship.Size.ToString()[0];
			}

			ships.Add(ship);
		}

		public AttackResult ReceiveAttack(string coordinates)
		{
			if (!IsValidCoordinate(coordinates))
			{
				throw new ArgumentException("Invalid coordinates");
			}

			var (row, col) = ParseCoordinates(coordinates);

			if (board[row, col] == 'X' || board[row, col] == 'O')
			{
				return new AttackResult { IsHit = false, IsShipDestroyed = false };
			}

			if (char.IsDigit(board[row, col]))
			{
				board[row, col] = 'X';
				return new AttackResult { IsHit = true, IsShipDestroyed = false };
			}

			board[row, col] = 'O';
			return new AttackResult { IsHit = false, IsShipDestroyed = false };
		}

		public SpecialBombResult RevealArea(string centerCoordinates)
		{
			var (row, col) = ParseCoordinates(centerCoordinates);
			var result = new SpecialBombResult();

			// Reveal 3x3 area
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					var newRow = row + i;
					var newCol = col + j;

					if (IsValidPosition(newRow, newCol))
					{
						if (char.IsDigit(board[newRow, newCol]))
						{
							result.RevealedShips.Add($"{ConvertToCoordinates(newRow, newCol)}");
						}
					}
				}
			}

			return result;
		}

		public SpecialBombResult ExplodeArea(string centerCoordinates)
		{
			var (row, col) = ParseCoordinates(centerCoordinates);
			var result = new SpecialBombResult();

			// Explode 3x3 area
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					var newRow = row + i;
					var newCol = col + j;

					if (IsValidPosition(newRow, newCol))
					{
						// If there's a ship, mark it as hit
						if (char.IsDigit(board[newRow, newCol]))
						{
							board[newRow, newCol] = 'X';
							result.DestroyedShips.Add($"{ConvertToCoordinates(newRow, newCol)}");
						}
						// Mark misses only if the position hasn't been hit already
						else if (board[newRow, newCol] == '.')
						{
							board[newRow, newCol] = 'O';
							result.MissedPositions.Add($"{ConvertToCoordinates(newRow, newCol)}");
						}
					}
				}
			}

			return result;
		}

		private bool IsValidCoordinate(string coordinates)
		{
			if (string.IsNullOrEmpty(coordinates) || coordinates.Length < 2)
			{
				return false;
			}

			try
			{
				var (row, col) = ParseCoordinates(coordinates);
				return IsValidPosition(row, col);
			}
			catch
			{
				return false;
			}
		}

		private bool IsValidPosition(int row, int col)
		{
			return row >= 0 && row < size && col >= 0 && col < size;
		}

		private bool HasAdjacentShip(int row, int col)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					var newRow = row + i;
					var newCol = col + j;

					if (IsValidPosition(newRow, newCol) && char.IsDigit(board[newRow, newCol]))
					{
						return true;
					}
				}
			}
			return false;
		}

		private (int row, int col) ParseCoordinates(string coordinates)
		{
			var letter = coordinates[0];
			var number = int.Parse(coordinates.Substring(1));
			var col = char.ToUpper(letter) - 'A';
			var row = number - 1;
			return (row, col);
		}

		public string ConvertToCoordinates(int row, int col)
		{
			var letter = (char)('A' + col);
			return $"{letter}{row + 1}";
		}

		// Method to display the board
		public void DisplayBoard(bool hideShips = false)
		{
			// Print column headers
			Console.Write("   ");
			for (int col = 0; col < size; col++)
			{
				Console.Write($" {(char)('A' + col)} ");
			}
			Console.WriteLine();

			// Print rows
			for (int row = 0; row < size; row++)
			{
				// Print row number with proper padding
				Console.Write($"{row + 1,2} ");

				// Print board cells
				for (int col = 0; col < size; col++)
				{
					char cell = board[row, col];

					// If hideShips is true, don't show ship locations (for opponent's board)
					if (hideShips && char.IsDigit(cell))
					{
						Console.Write(" . ");
					}
					else if (cell == 'X')
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write(" X ");
						Console.ResetColor();
					}
					else if (cell == 'O')
					{
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.Write(" O ");
						Console.ResetColor();
					}
					else if (char.IsDigit(cell))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write($" {cell} ");
						Console.ResetColor();
					}
					else
					{
						Console.Write(" . ");
					}
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		// Method to save board state to a file
		public void SaveBoardToFile(string filePath, bool hideShips = false)
		{
			using (StreamWriter writer = new StreamWriter(filePath, true))
			{
				// Write column headers
				writer.Write("   ");
				for (int col = 0; col < size; col++)
				{
					writer.Write($" {(char)('A' + col)} ");
				}
				writer.WriteLine();

				// Write rows
				for (int row = 0; row < size; row++)
				{
					// Write row number with proper padding
					writer.Write($"{row + 1,2} ");

					// Write board cells
					for (int col = 0; col < size; col++)
					{
						char cell = board[row, col];

						// If hideShips is true, don't show ship locations (for opponent's board)
						if (hideShips && char.IsDigit(cell))
						{
							writer.Write(" . "); // Empty space (hiding opponent's ships)
						}
						else if (cell == 'X')
						{
							writer.Write(" X "); // Hit
						}
						else if (cell == 'O')
						{
							writer.Write(" O "); // Miss
						}
						else if (char.IsDigit(cell))
						{
							writer.Write($" {cell} "); // Ship
						}
						else
						{
							writer.Write(" . "); // Empty space
						}
					}
					writer.WriteLine();
				}

				// Add legend at the bottom
				writer.WriteLine();
				if (!hideShips)
				{
					writer.WriteLine("Legend:");
					writer.WriteLine("  . = empty water");
					writer.WriteLine("  2-5 = your ships (number indicates ship size)");
					writer.WriteLine("  X = hit on a ship");
					writer.WriteLine("  O = missed shot");
				}
				else
				{
					writer.WriteLine("Legend:");
					writer.WriteLine("  . = unknown (either empty water or undiscovered ship)");
					writer.WriteLine("  X = your hit on opponent's ship");
					writer.WriteLine("  O = your missed shot");
				}
				writer.WriteLine();
			}
		}

		public int Size => size;
		public char[,] Board => board;
	}
}