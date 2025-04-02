using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaivuMusis
{
	public class GameRules
	{
		private readonly Dictionary<int, int> requiredShips;
		private readonly Dictionary<BombType, int> specialBombs;
		private readonly int minBoardSize;
		private readonly int maxBoardSize;

		public GameRules()
		{
			requiredShips = new Dictionary<int, int>();
			specialBombs = new Dictionary<BombType, int>();
			minBoardSize = 8;
			maxBoardSize = 20;
			LoadConfiguration();
		}

		private void LoadConfiguration()
		{
			try
			{
				var lines = File.ReadAllLines("game_rules_config.txt");
				bool readingShips = false;
				bool readingBombs = false;

				foreach (var line in lines)
				{
					var trimmedLine = line.Trim();
					if (string.IsNullOrEmpty(trimmedLine)) continue;

					if (trimmedLine.StartsWith("Lentos dydis:"))
					{
						// Board size is handled separately in Program.cs
						continue;
					}
					else if (trimmedLine.StartsWith("Laivų kiekis:"))
					{
						readingShips = true;
						readingBombs = false;
						continue;
					}
					else if (trimmedLine.StartsWith("Bombų kiekis:"))
					{
						readingShips = false;
						readingBombs = true;
						continue;
					}

					if (readingShips && trimmedLine.StartsWith("-"))
					{
						// Parse format: "- 1x 5 ilgio"
						var parts = trimmedLine.Split(new[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length >= 3 && int.TryParse(parts[1].Trim(), out int count) && int.TryParse(parts[2].Trim(), out int size))
						{
							requiredShips[size] = count;
						}
					}
					else if (readingBombs && trimmedLine.StartsWith("-"))
					{
						// Parse format: "- 3x Radar" or "- 3x Explosive wave"
						var parts = trimmedLine.Split(new[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int count))
						{
							var bombType = string.Join(" ", parts.Skip(2)).Trim().ToUpper();
							Console.WriteLine($"Debug: Parsing bomb type '{bombType}' with count {count}");

							if (bombType.Contains("RADAR"))
							{
								specialBombs[BombType.Radar] = count;
								Console.WriteLine($"Debug: Set Radar bombs to {count}");
							}
							else if (bombType.Contains("EXPLOSIVE") || bombType.Contains("WAVE"))
							{
								specialBombs[BombType.Explosive] = count;
								Console.WriteLine($"Debug: Set Explosive bombs to {count}");
							}
						}
					}
				}

				// Print final bomb counts for verification
				Console.WriteLine("\nFinal bomb counts:");
				foreach (var bomb in specialBombs)
				{
					Console.WriteLine($"{bomb.Key}: {bomb.Value}");
				}

				// Validate that we have at least one ship and one bomb type
				if (requiredShips.Count == 0)
				{
					Console.WriteLine("Warning: No ships configured, using default values");
					SetDefaultValues();
				}
				if (specialBombs.Count == 0)
				{
					Console.WriteLine("Warning: No bombs configured, using default values");
					SetDefaultValues();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading configuration: {ex.Message}");
				// Set default values if configuration fails to load
				SetDefaultValues();
			}
		}

		private void SetDefaultValues()
		{
			requiredShips.Clear();
			requiredShips[5] = 1;
			requiredShips[4] = 2;
			requiredShips[3] = 7;
			requiredShips[2] = 5;

			specialBombs.Clear();
			specialBombs[BombType.Radar] = 2;
			specialBombs[BombType.Explosive] = 2;
		}

		public bool ValidateBoardSize(int size)
		{
			return size >= minBoardSize && size <= maxBoardSize;
		}

		public bool ValidateShipPlacement(List<Ship> ships)
		{
			if (ships == null) return false;

			var shipCounts = new Dictionary<int, int>();

			// Count ships by size
			foreach (var ship in ships)
			{
				if (!shipCounts.ContainsKey(ship.Size))
				{
					shipCounts[ship.Size] = 0;
				}
				shipCounts[ship.Size]++;
			}

			// Check if we have the exact number of ships required
			foreach (var required in requiredShips)
			{
				if (!shipCounts.ContainsKey(required.Key) || shipCounts[required.Key] != required.Value)
				{
					Console.WriteLine($"Invalid ship count for size {required.Key}. Expected {required.Value}, got {shipCounts.GetValueOrDefault(required.Key, 0)}");
					return false;
				}
			}

			// Check if we have any extra ships
			foreach (var actual in shipCounts)
			{
				if (!requiredShips.ContainsKey(actual.Key))
				{
					Console.WriteLine($"Unexpected ship size {actual.Key} found");
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
		public Dictionary<BombType, int> SpecialBombs => specialBombs;
		public int MinBoardSize => minBoardSize;
		public int MaxBoardSize => maxBoardSize;
	}
}