using System;
using System.Collections.Generic;

namespace LaivuMusis
{
	public class GameRules
	{
		private readonly Dictionary<int, int> requiredShips;
		private readonly int minBoardSize;
		private readonly int maxBoardSize;

		public GameRules()
		{
			requiredShips = new Dictionary<int, int>
						{
								{ 5, 1 },  // 1 ship of size 5
                { 4, 2 },  // 2 ships of size 4
                { 3, 7 },  // 7 ships of size 3
                { 2, 5 }   // 5 ships of size 2
            };

			minBoardSize = 8;
			maxBoardSize = 20;
		}

		public bool ValidateBoardSize(int size)
		{
			return size >= minBoardSize && size <= maxBoardSize;
		}

		public bool ValidateShipPlacement(List<Ship> ships)
		{
			var shipCounts = new Dictionary<int, int>();

			foreach (var ship in ships)
			{
				if (!shipCounts.ContainsKey(ship.Size))
				{
					shipCounts[ship.Size] = 0;
				}
				shipCounts[ship.Size]++;
			}

			foreach (var required in requiredShips)
			{
				if (!shipCounts.ContainsKey(required.Key) || shipCounts[required.Key] != required.Value)
				{
					return false;
				}
			}

			return true;
		}

		public bool ValidateCoordinates(string coordinates, int boardSize)
		{
			if (string.IsNullOrEmpty(coordinates) || coordinates.Length < 2)
			{
				return false;
			}

			var letter = char.ToUpper(coordinates[0]);
			if (!char.IsLetter(letter) || letter - 'A' >= boardSize)
			{
				return false;
			}

			if (!int.TryParse(coordinates.Substring(1), out int number))
			{
				return false;
			}

			return number >= 1 && number <= boardSize;
		}

		public Dictionary<int, int> RequiredShips => requiredShips;
		public int MinBoardSize => minBoardSize;
		public int MaxBoardSize => maxBoardSize;
	}
}