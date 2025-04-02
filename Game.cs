using System;
using System.Collections.Generic;
using System.IO;

namespace LaivuMusis
{
	public class Game
	{
		private Player? player1;
		private Player? player2;
		private Player? currentPlayer;
		private GameBoard? gameBoard;
		private readonly GameLogger logger;
		private readonly GameRules rules;
		private readonly GameState gameState;
		private bool isGameOver;

		// File paths for board states - only two files now (one for each player)
		private const string Player1BoardFile = "player1_board.txt";
		private const string Player2BoardFile = "player2_board.txt";

		public Game()
		{
			logger = new GameLogger();
			rules = new GameRules();
			gameState = new GameState();
			isGameOver = false;
		}

		public void InitializeGame(int boardSize)
		{
			player1 = new Player("Player1", boardSize);
			player2 = new Player("Player2", boardSize);
			gameBoard = new GameBoard(boardSize);

			// Randomly select starting player
			currentPlayer = new Random().Next(2) == 0 ? player1 : player2;

			logger.LogGameStart(currentPlayer.Name);
		}

		public void LoadPlayerConfigurations(string player1ConfigPath, string player2ConfigPath)
		{
			if (player1 == null || player2 == null)
			{
				throw new InvalidOperationException("Game must be initialized before loading configurations");
			}

			player1.LoadConfiguration(player1ConfigPath);
			player2.LoadConfiguration(player2ConfigPath);

			// Save initial game state after loading configurations
			gameState.SaveInitialState(player1.Board, player2.Board);

			// Save initial board states to files
			SaveBoardStates();
		}

		// Method to save both players' board states to their respective files
		private void SaveBoardStates()
		{
			if (player1 != null && player2 != null)
			{
				// Player1's board file
				using (StreamWriter writer = new StreamWriter(Player1BoardFile, false))
				{
					writer.WriteLine($"=== {player1.Name}'s GAME STATE ===");
					writer.WriteLine();
					writer.WriteLine("YOUR BOARD (showing opponent's hits on your ships):");
					writer.WriteLine("X = hit on your ship, O = opponent's miss");
					writer.WriteLine();
				}
				player1.SaveBoardToFile(Player1BoardFile, false); // Show own ships with opponent's hits

				using (StreamWriter writer = new StreamWriter(Player1BoardFile, true))
				{
					writer.WriteLine("OPPONENT'S BOARD (showing your attacks):");
					writer.WriteLine("X = your hit on opponent's ship, O = your miss");
					writer.WriteLine();
				}
				player2.SaveBoardToFile(Player1BoardFile, true); // Hide opponent's ships, show your hits/misses

				// Player2's board file
				using (StreamWriter writer = new StreamWriter(Player2BoardFile, false))
				{
					writer.WriteLine($"=== {player2.Name}'s GAME STATE ===");
					writer.WriteLine();
					writer.WriteLine("YOUR BOARD (showing opponent's hits on your ships):");
					writer.WriteLine("X = hit on your ship, O = opponent's miss");
					writer.WriteLine();
				}
				player2.SaveBoardToFile(Player2BoardFile, false); // Show own ships with opponent's hits

				using (StreamWriter writer = new StreamWriter(Player2BoardFile, true))
				{
					writer.WriteLine("OPPONENT'S BOARD (showing your attacks):");
					writer.WriteLine("X = your hit on opponent's ship, O = your miss");
					writer.WriteLine();
				}
				player1.SaveBoardToFile(Player2BoardFile, true); // Hide opponent's ships, show your hits/misses
			}
		}

		public void MakeMove(string coordinates, bool isSpecialBomb = false)
		{
			if (isGameOver || currentPlayer == null || player1 == null || player2 == null) return;

			var targetPlayer = currentPlayer == player1 ? player2 : player1;
			var result = targetPlayer.ReceiveAttack(coordinates, isSpecialBomb);

			logger.LogMove(currentPlayer.Name, coordinates, result);

			// Save updated board states to files
			SaveBoardStates();

			// Display result in console
			Console.Clear();
			Console.WriteLine($"{currentPlayer.Name} attacks {coordinates}");
			Console.WriteLine($"Board states updated in {(currentPlayer == player1 ? Player1BoardFile : Player2BoardFile)}");

			// Display special bomb counts for both players
			DisplaySpecialBombCounts();

			if (result.IsHit)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("HIT!");
				Console.ResetColor();

				if (result.IsShipDestroyed)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("SHIP DESTROYED!");
					Console.ResetColor();

					if (targetPlayer.AreAllShipsDestroyed())
					{
						EndGame(currentPlayer);
						return;
					}
				}
				// Player gets another turn on hit
				return;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("MISS!");
				Console.ResetColor();
			}

			// Switch players on miss
			currentPlayer = targetPlayer;
		}

		private void DisplaySpecialBombCounts()
		{
			if (player1 == null || player2 == null) return;

			Console.WriteLine("\nSpecial Bombs Remaining:");
			Console.WriteLine("----------------------");

			// Player 1's bombs
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write($"{player1.Name}: ");
			Console.ResetColor();
			Console.Write($"Radar: {player1.GetBombCount(BombType.Radar)} | ");
			Console.Write($"Explosive: {player1.GetBombCount(BombType.Explosive)}");
			Console.WriteLine();

			// Player 2's bombs
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write($"{player2.Name}: ");
			Console.ResetColor();
			Console.Write($"Radar: {player2.GetBombCount(BombType.Radar)} | ");
			Console.Write($"Explosive: {player2.GetBombCount(BombType.Explosive)}");
			Console.WriteLine("\n");
		}

		public void UseSpecialBomb(string coordinates, BombType bombType)
		{
			if (isGameOver || currentPlayer == null || player1 == null || player2 == null) return;

			var targetPlayer = currentPlayer == player1 ? player2 : player1;
			var result = targetPlayer.ReceiveSpecialBomb(coordinates, bombType);

			logger.LogSpecialBomb(currentPlayer.Name, coordinates, bombType, result);

			// Save updated board states to files
			SaveBoardStates();

			// Display result in console
			Console.Clear();
			Console.WriteLine($"{currentPlayer.Name} uses {bombType} bomb at {coordinates}");
			Console.WriteLine($"Board states updated in {(currentPlayer == player1 ? Player1BoardFile : Player2BoardFile)}");

			// Display special bomb counts for both players
			DisplaySpecialBombCounts();

			if (bombType == BombType.Radar)
			{
				if (result.RevealedShips.Count > 0)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"RADAR reveals ships at: {string.Join(", ", result.RevealedShips)}");
					Console.ResetColor();
				}
				else
				{
					Console.WriteLine("RADAR reveals no ships in the area");
				}

				// Switch players after using Radar bomb (doesn't give extra turn)
				currentPlayer = targetPlayer;
			}
			else if (bombType == BombType.Explosive)
			{
				bool hitAny = false;

				if (result.DestroyedShips.Count > 0)
				{
					hitAny = true;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"EXPLOSIVE bomb hits positions: {string.Join(", ", result.DestroyedShips)}");
					Console.ResetColor();
				}

				if (result.MissedPositions.Count > 0)
				{
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.WriteLine($"EXPLOSIVE bomb misses at: {string.Join(", ", result.MissedPositions)}");
					Console.ResetColor();
				}

				if (!hitAny)
				{
					Console.WriteLine("EXPLOSIVE bomb hits nothing");

					// Switch players if no ships were hit
					currentPlayer = targetPlayer;
				}
				else
				{
					// Check if all ships are destroyed
					if (targetPlayer.AreAllShipsDestroyed())
					{
						EndGame(currentPlayer);
						return;
					}
					// Otherwise, current player gets another turn (don't switch players)
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("You hit ships with the explosive bomb! Take another turn.");
					Console.ResetColor();
				}
			}
		}

		private void EndGame(Player winner)
		{
			isGameOver = true;
			logger.LogGameEnd(winner.Name);

			// Save final board states
			SaveBoardStates();
		}

		public bool IsGameOver => isGameOver;
		public Player? CurrentPlayer => currentPlayer;
		public Player? Player1 => player1;
		public Player? Player2 => player2;

		~Game()
		{
			try
			{
				// Save final game state if game is still in progress
				if (!isGameOver && player1 != null && player2 != null)
				{
					SaveBoardStates();
				}

				// Clean up any temporary files or resources
				if (File.Exists(Player1BoardFile))
				{
					File.AppendAllText(Player1BoardFile, "\n--- GAME CLEANUP ---\n");
				}
				if (File.Exists(Player2BoardFile))
				{
					File.AppendAllText(Player2BoardFile, "\n--- GAME CLEANUP ---\n");
				}
			}
			catch
			{
				// Ignore any errors during cleanup
			}
		}
	}
}