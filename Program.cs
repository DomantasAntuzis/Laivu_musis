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
			int boardSize = ReadBoardSizeFromConfig("game_rules_config.txt");
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

			Console.WriteLine("\nInitial Game State:");
			Console.WriteLine("------------------");
			Console.WriteLine("Game state information has been saved to the following files:");
			Console.WriteLine("- player1_board.txt - Shows Player1's board (with opponent's hits) and their attacks on Player2");
			Console.WriteLine("- player2_board.txt - Shows Player2's board (with opponent's hits) and their attacks on Player1");
			Console.WriteLine("Each file shows:");
			Console.WriteLine("  * YOUR BOARD: your ships (with any opponent's hits marked as X)");
			Console.WriteLine("  * OPPONENT'S BOARD: your hits (X) and misses (O) on the opponent's ships");

			Console.WriteLine("\nPress any key to start the game...");
			Console.ReadKey();
			Console.Clear();

			Console.WriteLine("\nGame started! Players will take turns attacking.");
			Console.WriteLine("Enter coordinates in format 'A1', 'B2', etc.");
			Console.WriteLine("To use special bombs, enter 'RADAR' or 'EXPLOSIVE' followed by coordinates.");
			Console.WriteLine("Board states will be updated in the board files after each move.");

			while (!game.IsGameOver)
			{
				var currentPlayer = game.CurrentPlayer;
				if (currentPlayer == null)
				{
					Console.WriteLine("Error: Current player is null");
					break;
				}

				// Show current player info
				Console.WriteLine($"\n{currentPlayer.Name}'s turn");
				Console.WriteLine($"Check {currentPlayer.Name.ToLower()}_board.txt to see your board with opponent's hits and your attacks on the opponent.");

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
				Console.WriteLine("Final board states are saved to the board files.");
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