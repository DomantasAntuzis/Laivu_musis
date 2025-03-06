using System;
using System.Collections.Generic;
using System.IO;

namespace LaivuMusis
{
	public class Player
	{
		private readonly string name;
		private readonly GameBoard board;
		private readonly List<Ship> ships;
		private readonly Dictionary<BombType, int> specialBombs;
		private readonly HashSet<string> attackedPositions;

		public Player(string name, int boardSize)
		{
			this.name = name;
			this.board = new GameBoard(boardSize);
			this.ships = new List<Ship>();
			this.attackedPositions = new HashSet<string>();
			this.specialBombs = new Dictionary<BombType, int>
			{
				{ BombType.Radar, 2 },
				{ BombType.Explosive, 2 }
			};
		}

		public void LoadConfiguration(string configPath)
		{
			try
			{
				var lines = File.ReadAllLines(configPath);
				foreach (var line in lines)
				{
					var parts = line.Trim().Split(' ');
					if (parts.Length >= 2)
					{
						var coordinates = parts[0].Trim().ToUpper();
						if (int.TryParse(parts[1], out int shipSize))
						{
							try
							{
								// Try horizontal placement first
								var ship = new Ship(coordinates, shipSize, true);
								if (board.CanPlaceShip(ship))
								{
									PlaceShip(coordinates, shipSize, true);
								}
								else
								{
									// If horizontal doesn't work, try vertical
									ship = new Ship(coordinates, shipSize, false);
									if (board.CanPlaceShip(ship))
									{
										PlaceShip(coordinates, shipSize, false);
									}
									else
									{
										throw new InvalidOperationException($"Cannot place ship at position {coordinates} with size {shipSize} in either orientation");
									}
								}
							}
							catch (InvalidOperationException ex)
							{
								Console.WriteLine($"Error placing ship at {coordinates} with size {shipSize}: {ex.Message}");
								throw;
							}
						}
						else
						{
							throw new ArgumentException($"Invalid ship size: {parts[1]}");
						}
					}
					else
					{
						throw new ArgumentException($"Invalid line format: {line}");
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error loading configuration from {configPath}: {ex.Message}");
			}
		}

		public void PlaceShip(string startCoordinates, int size, bool isHorizontal)
		{
			var ship = new Ship(startCoordinates, size, isHorizontal);
			if (board.CanPlaceShip(ship))
			{
				board.PlaceShip(ship);
				ships.Add(ship);
			}
			else
			{
				throw new InvalidOperationException($"Cannot place ship at position {startCoordinates} with size {size}");
			}
		}

		public AttackResult ReceiveAttack(string coordinates, bool isSpecialBomb = false)
		{
			if (attackedPositions.Contains(coordinates))
			{
				return new AttackResult { IsHit = false, IsShipDestroyed = false };
			}

			attackedPositions.Add(coordinates);
			var hitResult = board.ReceiveAttack(coordinates);

			if (hitResult.IsHit)
			{
				var ship = ships.Find(s => s.ContainsCoordinate(coordinates));
				if (ship != null)
				{
					ship.Hit();
					if (ship.IsDestroyed)
					{
						return new AttackResult { IsHit = true, IsShipDestroyed = true };
					}
				}
			}

			return hitResult;
		}

		public SpecialBombResult ReceiveSpecialBomb(string coordinates, BombType bombType)
		{
			if (!HasSpecialBomb(bombType))
			{
				throw new InvalidOperationException("No special bombs of this type remaining");
			}

			var result = new SpecialBombResult();

			switch (bombType)
			{
				case BombType.Radar:
					result = board.RevealArea(coordinates);
					break;
				case BombType.Explosive:
					result = board.ExplodeArea(coordinates);
					break;
			}

			specialBombs[bombType]--;
			return result;
		}

		public bool HasSpecialBomb(BombType bombType)
		{
			return specialBombs.ContainsKey(bombType) && specialBombs[bombType] > 0;
		}

		public bool AreAllShipsDestroyed()
		{
			return ships.TrueForAll(s => s.IsDestroyed);
		}

		public string Name => name;
		public GameBoard Board => board;
	}
}