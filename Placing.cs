using System;

namespace CoinSheet3 {
    public class Placing {
        Competitor competitor;
        Tournament tournament;
        string placingText;
        int wins;
        int losses;
        int ties;
        float ratingChange = 0f;
       
        public void SetCompetitor(Competitor c) {
            competitor = c;
        }
        
        public void SetPlacingText(string s) {
            placingText = s;
        }

        public void SetTournament(Tournament t) {
            tournament = t;
        }
        public void SetRatingChange(float f) {
            ratingChange = f;
        }

        public void SetWins(int i) {
            wins = i;
        }

        public void SetLosses(int i) {
            losses = i;
        }

        public Competitor GetCompetitor() {
            return competitor;
        }

        public Tournament GetTournament() {
            return tournament;
        }

        public string GetPlacingText() {
            return placingText;
        }

        public float GetRatingChange() {
            return ratingChange;
        }

        public int GetWins() {
            return wins;
        }

        public int GetLosses() {
            return losses;
        }
    }
}