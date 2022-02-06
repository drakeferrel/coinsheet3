using System;

namespace CoinSheet3 {
	public class CharacterUsage {
		MeleeCharacter characterUsed;
		int wins = 0;
		int losses = 0;
		int ties = 0;

		public CharacterUsage(MeleeCharacter mc) {
			characterUsed = mc;
        }

		public MeleeCharacter GetCharacter() {
			return characterUsed;
        }

		public int GetWins() {
			return wins;
        }

		public int GetLosses() {
			return losses;
        }

		public void AddWin() {
			wins++;
        }

		public void AddLoss() {
			losses++;
        }
	}
}