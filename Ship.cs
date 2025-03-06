using System;
using System.Collections.Generic;

namespace LaivuMusis
{
	public class Ship
	{
		private readonly string startCoordinates;
		private readonly int size;
		private int hits;
		private readonly bool isHorizontal;

		public Ship(string startCoordinates, int size, bool isHorizontal = true)
		{
			if (string.IsNullOrEmpty(startCoordinates) || startCoordinates.Length < 2)
			{
				throw new ArgumentException("Invalid coordinates format");
			}

			if (size < 1 || size > 5)
			{
				throw new ArgumentException($"Invalid ship size: {size}. Ship size must be between 1 and 5.");
			}

			this.startCoordinates = startCoordinates;
			this.size = size;
			this.hits = 0;
			this.isHorizontal = isHorizontal;
		}

		public List<string> GetAllCoordinates()
		{
			var coordinates = new List<string>();
			var (startRow, startCol) = ParseCoordinates(startCoordinates);

			for (int i = 0; i < size; i++)
			{
				if (isHorizontal)
				{
					coordinates.Add($"{ConvertToCoordinates(startRow, startCol + i)}");
				}
				else
				{
					coordinates.Add($"{ConvertToCoordinates(startRow + i, startCol)}");
				}
			}

			return coordinates;
		}

		public bool ContainsCoordinate(string coordinates)
		{
			return GetAllCoordinates().Contains(coordinates);
		}

		public void Hit()
		{
			hits++;
		}

		public bool IsDestroyed => hits >= size;

		private (int row, int col) ParseCoordinates(string coordinates)
		{
			try
			{
				var letter = coordinates[0];
				var number = int.Parse(coordinates.Substring(1));
				var col = char.ToUpper(letter) - 'A';
				var row = number - 1;
				return (row, col);
			}
			catch (Exception)
			{
				throw new ArgumentException($"Invalid coordinates format: {coordinates}");
			}
		}

		private string ConvertToCoordinates(int row, int col)
		{
			var letter = (char)('A' + col);
			return $"{letter}{row + 1}";
		}

		public string StartCoordinates => startCoordinates;
		public int Size => size;
		public bool IsHorizontal => isHorizontal;
	}
}