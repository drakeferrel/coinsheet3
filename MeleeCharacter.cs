using System;
using System.Collections.Generic;

namespace CoinSheet3 {
    public class MeleeCharacter {
        string name;
        List<CharacterMatchup> characterMatchups;
        float numberOfMains = 0;

        public MeleeCharacter(string s) {
            name = s;
            characterMatchups = new List<CharacterMatchup>();
        }

        public string GetName() {
            return name;
        }
        public void AdjustCharacterMatchup(MeleeCharacter mc, float f) {
            CharacterMatchup requestedMatchup = null;

            // See if we already have data on this matchup
            foreach (CharacterMatchup mu in characterMatchups) {
                if (mu.GetCharacter() == mc) requestedMatchup = mu;
            }

            // We don't already have this matchup, so create it
            if (requestedMatchup == null) {
                requestedMatchup = new CharacterMatchup(mc);
                characterMatchups.Add(requestedMatchup);
            }

            requestedMatchup.AddRatingExchange(f);

        }

        public void AddMain(float f) {
            numberOfMains += f;
        }

        public int GetNumberOfMains() {
            return (int)(MathF.Round(numberOfMains));
        }

        public List<CharacterMatchup> GetCharacterMatchups() {
            return characterMatchups;
        }
    }
}