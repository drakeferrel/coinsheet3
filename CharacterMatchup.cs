using System;

namespace CoinSheet3 {
	public class CharacterMatchup {
		MeleeCharacter versusCharacter;
		float ratingExchange = 0f;

		public CharacterMatchup(MeleeCharacter mc) {
			versusCharacter = mc;
        }

		public MeleeCharacter GetCharacter() {
			return versusCharacter;
        }

		public float GetRatingExchange() {
			return ratingExchange;
        }

		public void SetVersusCharacter(MeleeCharacter mc) {
			versusCharacter = mc;
        }

		public void AddRatingExchange(float f) {
			ratingExchange += f;
        }
	}
}
