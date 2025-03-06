using System;
using System.IO;

namespace LaivuMusis
{
	public class GameState
	{
		private readonly string filePath;

		public GameState(string filePath = "game_state.txt")
		{
			this.filePath = filePath;
		}

		public void SaveInitialState(GameBoard player1Board, GameBoard player2Board)
		{
			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("Player 1 Board:");
				WriteBoard(writer, player1Board);

				writer.WriteLine("\nPlayer 2 Board:");
				WriteBoard(writer, player2Board);
			}
		}

		private void WriteBoard(StreamWriter writer, GameBoard board)
		{
			// Write column headers
			writer.Write("   ");
			for (int i = 0; i < board.Size; i++)
			{
				writer.Write($"{(char)('A' + i)}  ");
			}
			writer.WriteLine();

			// Write board content
			for (int i = 0; i < board.Size; i++)
			{
				writer.Write($"{i + 1,2} ");
				for (int j = 0; j < board.Size; j++)
				{
					writer.Write($"{board.Board[i, j]}  ");
				}
				writer.WriteLine();
			}
		}
	}
}