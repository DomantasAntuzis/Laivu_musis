using System;
using System.IO;

namespace LaivuMusis
{
	public class GameLogger
	{
		private readonly string logFilePath;

		public GameLogger(string logFilePath = "game_log.txt")
		{
			this.logFilePath = logFilePath;
			File.WriteAllText(logFilePath, "--- GAME START ---\n");
		}

		public void LogGameStart(string startingPlayer)
		{
			AppendToLog($"{startingPlayer} starts");
		}

		public void LogMove(string player, string coordinates, AttackResult result)
		{
			var resultText = result.IsHit ? "HIT" : "MISS";
			if (result.IsShipDestroyed)
			{
				resultText += " SHIP DESTROYED";
			}
			AppendToLog($"{player} {coordinates} {resultText}");
		}

		public void LogSpecialBomb(string player, string coordinates, BombType bombType, SpecialBombResult result)
		{
			var bombTypeText = bombType == BombType.Radar ? "RADAR" : "EXPLOSIVE";
			AppendToLog($"{player} {coordinates} SPECIAL-{bombTypeText}");

			if (result.RevealedShips.Count > 0)
			{
				AppendToLog($"Revealed ships at: {string.Join(", ", result.RevealedShips)}");
			}

			if (result.DestroyedShips.Count > 0)
			{
				AppendToLog($"Destroyed ships at: {string.Join(", ", result.DestroyedShips)}");
			}
		}

		public void LogGameEnd(string winner)
		{
			AppendToLog($"{winner} WINNER");
			AppendToLog("--- GAME END ---");
		}

		private void AppendToLog(string message)
		{
			File.AppendAllText(logFilePath, message + "\n");
		}
	}
}