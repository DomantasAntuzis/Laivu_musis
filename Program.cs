using System;
using System.IO;

namespace LaivuMusis
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Welcome to Laivų Mūšis!");

			// Read board size from configuration file
			int boardSize = ReadBoardSizeFromConfig("taisykles.txt");
			if (boardSize == 0)
			{
				Console.WriteLine("Error: Could not read board size from configuration file.");
				return;
			}

			var game = new Game();
			game.InitializeGame(boardSize);

			// Load player configurations
			Console.WriteLine("\nLoading player configurations...");
			game.LoadPlayerConfigurations("player1_cfg.txt", "player2_cfg.txt");

			Console.WriteLine("\nGame started! Players will take turns attacking.");
			Console.WriteLine("Enter coordinates in format 'A1', 'B2', etc.");
			Console.WriteLine("To use special bombs, enter 'RADAR' or 'EXPLOSIVE' followed by coordinates.");

			while (!game.IsGameOver)
			{
				var currentPlayer = game.CurrentPlayer;
				if (currentPlayer == null)
				{
					Console.WriteLine("Error: Current player is null");
					break;
				}

				Console.WriteLine($"\n{currentPlayer.Name}'s turn");

				var input = Console.ReadLine()?.ToUpper();
				if (string.IsNullOrEmpty(input))
				{
					Console.WriteLine("Invalid input. Please try again.");
					continue;
				}

				try
				{
					if (input.StartsWith("RADAR") || input.StartsWith("EXPLOSIVE"))
					{
						var parts = input.Split(' ');
						if (parts.Length != 2)
						{
							Console.WriteLine("Invalid special bomb format. Use 'RADAR A1' or 'EXPLOSIVE A1'");
							continue;
						}

						var bombType = parts[0] == "RADAR" ? BombType.Radar : BombType.Explosive;
						game.UseSpecialBomb(parts[1], bombType);
					}
					else
					{
						game.MakeMove(input);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}

			var winner = game.CurrentPlayer;
			if (winner != null)
			{
				Console.WriteLine($"\nGame Over! {winner.Name} wins!");
				Console.WriteLine("Check game_log.txt for detailed game history.");
			}
		}

		private static int ReadBoardSizeFromConfig(string configPath)
		{
			try
			{
				var lines = File.ReadAllLines(configPath);
				foreach (var line in lines)
				{
					if (line.StartsWith("Lentos dydis:"))
					{
						var sizeStr = line.Split(':')[1].Trim();
						if (int.TryParse(sizeStr, out int size))
						{
							return size;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading configuration file: {ex.Message}");
			}
			return 0;
		}
	}
}