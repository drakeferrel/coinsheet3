using System;

namespace CoinSheet3 {
	public class Match {
		Competitor winner;
		Competitor loser;
		MeleeCharacter winnerCharacterUsed;
		MeleeCharacter loserCharacterUsed;
		int winnerScore = 0;
		int loserScore = 0;
		int firstToWinCondition = 2;

		public Competitor GetWinner() {
			return winner;
        }

		public Competitor GetLoser() {
			return loser;
        }

		public MeleeCharacter GetWinnerCharacterUsed() {
			return winnerCharacterUsed;
        }

		public MeleeCharacter GetLoserCharacterUsed() {
			return loserCharacterUsed;
        }

		public int GetWinCondition() {
			return firstToWinCondition;
        }

		public void SetWinner(Competitor c, MeleeCharacter mc) {
			winner = c;
			winnerCharacterUsed = mc;
        }

		public void SetLoser(Competitor c, MeleeCharacter mc) {
			loser = c;
			loserCharacterUsed = mc;
        }

		public void SetWinCondition(int i) {
			firstToWinCondition = i;
        }
	}
}