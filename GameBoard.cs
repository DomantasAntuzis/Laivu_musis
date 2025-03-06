using System;
using System.Collections.Generic;

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
						if (char.IsDigit(board[newRow, newCol]))
						{
							board[newRow, newCol] = 'X';
							result.DestroyedShips.Add($"{ConvertToCoordinates(newRow, newCol)}");
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

		private string ConvertToCoordinates(int row, int col)
		{
			var letter = (char)('A' + col);
			return $"{letter}{row + 1}";
		}

		public int Size => size;
		public char[,] Board => board;
	}
}