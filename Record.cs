using System;

namespace CoinSheet3 {
    public class Record {
        Competitor opponent;
        int setWins = 0;
        int setLosses = 0;
        int gameWins = 0;
        int gameLosses = 0;

        public Record(Competitor c) {
            opponent = c;
        }

        public Competitor GetOpponent() {
            return opponent;
        }

        public void SetOpponent(Competitor c) {
            opponent = c;
        }

        public int GetSetWins() {
            return setWins;
        }

        public int GetSetLosses() {
            return setLosses;
        }

        public void AddSetWin(int gamesWon, int gamesLost) {
            setWins++;
            gameWins += gamesWon;
            gameLosses += gamesLost;
        }

        public void AddSetLoss(int gamesWon, int gamesLost) {
            setLosses++;
            gameWins += gamesWon;
            gameLosses += gamesLost;
        }

        public int GetGameWins() {
            return gameWins;
        }

        public int GetGameLosses() {
            return gameLosses;
        }

        public void AddGameWin() {
            gameWins++;
        }

        public void AddGameLoss() {
            gameLosses++;
        }
    }
}