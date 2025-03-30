using System.Collections.Generic;

namespace LaivuMusis
{
	public class AttackResult
	{
		public bool IsHit { get; set; }
		public bool IsShipDestroyed { get; set; }
	}

	public class SpecialBombResult
	{
		public List<string> RevealedShips { get; set; } = new List<string>();
		public List<string> DestroyedShips { get; set; } = new List<string>();
		public List<string> MissedPositions { get; set; } = new List<string>();

		// Boolean property to check if any ships were destroyed
		public bool HasHitShips => DestroyedShips.Count > 0;
	}

	public enum BombType
	{
		Radar,
		Explosive
	}
}