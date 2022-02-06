using System;
using System.Collections.Generic;

namespace CoinSheet3 {
    public class Competitor {
        string tag;
        float rating;
        float peakRating;
        float startingRating;
        List<Record> records;
        List<Placing> placings;
        List<CharacterUsage> charactersUsed;
        List<CharacterMatchup> characterMatchups;

        public float bufferedRating;
        public int bufferedWins = 0;
        public int bufferedLosses = 0;
        int promotionGameWins = 0;
        int promotionSetWins = 0;
        int promotionWinsUnique = 0;
        int tier = -1;

        public bool incoinito = false;
        public bool outOfState = false;

        MeleeCharacter defaultCharacter;

        Competitor mainAccount;
        Competitor outOfStateStrongestWin;

        public Competitor(string s) {
            tag = s;
            records = new List<Record>();
            charactersUsed = new List<CharacterUsage>();
            placings = new List<Placing>();
            characterMatchups = new List<CharacterMatchup>();
            rating = 0f;
            peakRating = 0f;
            startingRating = 0f;

            if (tag.StartsWith("@")) outOfState = true;
        }

        public float GetRating() {
            return rating;
        }

        public float GetRating(bool b) {
            if (b == true && mainAccount != null) return mainAccount.GetRating();
            else return GetRating();
        }

        public float GetStartingRating() {
            return startingRating;
        }

        public int GetTier() {
            return tier;
        }

        public string GetTag() {
            return tag;
        }

        public List<Record> GetRecords() {
            return records;
        }

        public List<CharacterUsage> GetCharacterUsage() {
            return charactersUsed;
        }

        public List<Placing> GetPlacings() {
            return placings;
        }

        public List<CharacterMatchup> GetCharacterMatchups() {
            return characterMatchups;
        }

        public void SortCharacterMatchups() {
            // Bubble sort matchups by absolute value of rating exchange
            CharacterMatchup temp;
            for(int write = 0; write < characterMatchups.Count; write++) {
                for(int sort = 0; sort < characterMatchups.Count -1; sort++) {
                    if(MathF.Abs(characterMatchups[sort].GetRatingExchange()) < MathF.Abs(characterMatchups[sort+1].GetRatingExchange())) {
                        temp = characterMatchups[sort + 1];
                        characterMatchups[sort + 1] = characterMatchups[sort];
                        characterMatchups[sort] = temp;
                    }
                }
            }
        }

        public void SetMainAccount(Competitor c) {
            mainAccount = c;

            // Secondaries are excluded from the final list, notable records, etc.
            incoinito = true;
        }

        public MeleeCharacter GetDefaultCharacter() {
            return defaultCharacter;
        }

        public int GetSetWinCount() {
            int winCount = 0;
            foreach(Record r in records) {
                winCount+=r.GetSetWins();
            }

            return winCount;
        }

        public int GetSetLossCount() {
            int lossCount = 0;
            foreach (Record r in records) {
                lossCount += r.GetSetLosses();
            }

            return lossCount;
        }

        public void AddResult(Competitor opponent, bool isWinner, MeleeCharacter characterUsed, int gamesWon, int gamesLost) {
            if (isWinner) {
                bufferedWins++;
                if (mainAccount != null) mainAccount.AddResult(opponent, isWinner, characterUsed, gamesWon, gamesLost);
            } else bufferedLosses++;

            Record recordVersusThisOpponent = null;
            foreach(Record r in records) {
                if(r.GetOpponent() == opponent) {
                    recordVersusThisOpponent = r;
                }
            }

            // If record wasn't found, create a new one versus this opponent
            if (recordVersusThisOpponent == null) {
                recordVersusThisOpponent = new Record(opponent);
                records.Add(recordVersusThisOpponent);
            }

            if (isWinner) recordVersusThisOpponent.AddSetWin(gamesWon,gamesLost);
            else recordVersusThisOpponent.AddSetLoss(gamesWon,gamesLost);

            // Track character usage
            CharacterUsage characterUsage = null;
            foreach(CharacterUsage cu in charactersUsed) {
                if(cu.GetCharacter() == characterUsed) {
                    characterUsage = cu;
                }
            }

            if(characterUsage == null) {
                characterUsage = new CharacterUsage(characterUsed);
                charactersUsed.Add(characterUsage);
            }

            if (isWinner) characterUsage.AddWin();
            else characterUsage.AddLoss();
        }

        public void IncrementBufferRating(float f) {
            bufferedRating += f;
        }

        public void SetRating(float f) {
            rating = f;
            if (rating > peakRating) peakRating = rating;
        }

        public void SetRating(float f, bool b) {
            SetRating(f);
            if(b == true) startingRating = f;
        }

        public void SetTier(int newTier) {
            tier = newTier;
        }

        public void CheckForPromotionWins(int checkTier) {
            /*
            foreach(Record r in records) {
                if(r.GetOpponent().GetTier() == checkTier) {
                    if (r.GetSetWins() > 0) {
                        promotionWins += r.GetSetWins();
                        promotionWinsUnique++;
                    }
                }
            }
            */

            // Testing using game wins instead of set wins
            foreach(Record r in records) {
                if(r.GetOpponent().GetTier() == checkTier) {
                    if (r.GetGameWins() > 0) {
                        // promotionGameWins += r.GetGameWins();

                        if (r.GetSetWins() > 0) promotionSetWins += r.GetSetWins();
                        promotionWinsUnique++;

                        if (GetTag() == "Asiago") Console.WriteLine(r.GetOpponent().GetTag() + ": " + r.GetOpponent().GetTier());
                    }
                }
            }
            
            int promotionWinGameRequirement = 8;
            int promotionWinSetRequirement = 2;
            int promotionWinUniqueRequirement = 2;

            if((promotionGameWins >= promotionWinGameRequirement || promotionSetWins >= promotionWinSetRequirement) && promotionWinsUnique >= promotionWinUniqueRequirement) {
                if (tier == -1) {
                    SetRating(1500f - (checkTier * 100f), true);
                }

                tier = checkTier + 1;
            }
        }

        public void SetDefaultCharacter(MeleeCharacter mc) {
            defaultCharacter = mc;
        }

        public void AddPlacing(Placing p) {
            placings.Add(p);

            if (mainAccount != null) mainAccount.AddPlacing(p);
        }

        public void AdjustCharacterMatchup(MeleeCharacter mc, float f) {
            CharacterMatchup requestedMatchup = null;

            // See if we already have data on this matchup
            foreach(CharacterMatchup mu in characterMatchups) {
                if (mu.GetCharacter() == mc) requestedMatchup = mu;
            }

            // We don't already have this matchup, so create it
            if(requestedMatchup == null) {
                requestedMatchup = new CharacterMatchup(mc);
                characterMatchups.Add(requestedMatchup);
            }

            requestedMatchup.AddRatingExchange(f);

        }

        public string GetTierName() {
            if (tier == 0) {
                return "[PR]";
            } else {
                char className = (char)(65 + (tier - 1));

                return "[" + className + "]";
            }
        }

        public Competitor GetMainAccount() {
            return mainAccount;
        }
    }
}