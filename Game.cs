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
		}

		public void MakeMove(string coordinates, bool isSpecialBomb = false)
		{
			if (isGameOver || currentPlayer == null || player1 == null || player2 == null) return;

			var targetPlayer = currentPlayer == player1 ? player2 : player1;
			var result = targetPlayer.ReceiveAttack(coordinates, isSpecialBomb);

			logger.LogMove(currentPlayer.Name, coordinates, result);

			if (result.IsHit)
			{
				if (result.IsShipDestroyed)
				{
					if (targetPlayer.AreAllShipsDestroyed())
					{
						EndGame(currentPlayer);
						return;
					}
				}
				// Player gets another turn on hit
				return;
			}

			// Switch players on miss
			currentPlayer = targetPlayer;
		}

		public void UseSpecialBomb(string coordinates, BombType bombType)
		{
			if (isGameOver || currentPlayer == null || player1 == null || player2 == null) return;

			var targetPlayer = currentPlayer == player1 ? player2 : player1;
			var result = targetPlayer.ReceiveSpecialBomb(coordinates, bombType);

			logger.LogSpecialBomb(currentPlayer.Name, coordinates, bombType, result);

			// Switch players after using special bomb
			currentPlayer = targetPlayer;
		}

		private void EndGame(Player winner)
		{
			isGameOver = true;
			logger.LogGameEnd(winner.Name);
		}

		public bool IsGameOver => isGameOver;
		public Player? CurrentPlayer => currentPlayer;
	}
}