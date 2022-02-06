using System;
using System.Collections.Generic;

namespace CoinSheet3 {
	public class Tournament {
		string name;
		string date;
		string url;
		Tournament parent;
		Venue venue;
		List<MatchGroup> allMatches;
		List<Placing> placings;

		public Tournament() {
			allMatches = new List<MatchGroup>();
			placings = new List<Placing>();
		}

		public string Display() {
			return name + " with " + allMatches.Count;
		}

		public string GetName() {
			return name;
		}

		public string GetDate() {
			return date;
		}

		public string GetURL() {
			return url;
        }

		public Tournament GetParentTournament() {
			return parent;
		}

		public List<MatchGroup> GetAllMatches() {
			return allMatches;
		}

		public Venue GetVenue() {
			return venue;
		}

		public void SetName(string s) {
			name = s;
		}

		public void SetDate(string s) {
			date = s;
		}

		public void SetURL(string s) {
			url = s;
		}

		public void SetParentTournament(Tournament t) {
			parent = t;
		}

		public void SetVenue(Venue v) {
			venue = v;
		}

		public void AddMatchGroup(MatchGroup mg) {
			allMatches.Add(mg);
		}

		public void AddPlacing(Placing p) {
			placings.Add(p);
		}

		public float GetRatingWeight() {
			List<Competitor> allParticipants = new List<Competitor>();
			float ratingSum = 0f;
			foreach(MatchGroup mg in allMatches) {
				foreach (Match m in mg.matches) {
					if (!allParticipants.Contains(m.GetWinner())) {
						allParticipants.Add(m.GetWinner());
						ratingSum += m.GetWinner().GetRating();
					}

					if (!allParticipants.Contains(m.GetLoser())) {
						allParticipants.Add(m.GetLoser());
						ratingSum += m.GetLoser().GetRating();
					}
				}
            }

			return ratingSum;
		}

		public List<Competitor> GetAllCompetitors() {
			List<Competitor> allParticipants = new List<Competitor>();
			foreach (MatchGroup mg in allMatches) {
				foreach (Match m in mg.matches) {
					if (!allParticipants.Contains(m.GetWinner())) {
						allParticipants.Add(m.GetWinner());
					}

					if (!allParticipants.Contains(m.GetLoser())) {
						allParticipants.Add(m.GetLoser());
					}
				}
			}

			return allParticipants;
		}

		public List<Placing> GetPlacings() {
			return placings;
        }

		public void UpdateOutOfStateAttendants() {
			List<Competitor> allParticipants = GetAllCompetitors();
			foreach(Competitor c in allParticipants) {
				if(c.outOfState) {
					foreach(Record r in c.GetRecords()) {
						if (r.GetSetWins() > 0) {
							if (c.GetTier() > r.GetOpponent().GetTier()) {
								c.SetTier(r.GetOpponent().GetTier());
								Console.WriteLine(c.GetTag() + " steals " +r.GetOpponent().GetTag()+" tier: "+c.GetTier()+" vs "+r.GetOpponent().GetTier());
							}

							if (c.GetRating() < r.GetOpponent().GetRating()) {
								c.SetRating(r.GetOpponent().GetRating());
								Console.WriteLine(c.GetTag() + " steals " + r.GetOpponent().GetTag() + "'s rating: " + c.GetRating() + " vs " + r.GetOpponent().GetRating());
							}
						}
                    }
                }
            }
        }
	}
}
