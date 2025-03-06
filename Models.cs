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
	}

	public enum BombType
	{
		Radar,
		Explosive
	}
}