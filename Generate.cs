using System;
using System.Collections.Generic;
using System.IO;

namespace CoinSheet3 {
    class Generate {
        const int MODE_SEARCHING = -1;
        const int MODE_VENUE = 0;
        const int MODE_TOURNAMENT = 1;
        const int MODE_PLACING = 2;
        const int MODE_CHARACTERS = 3;

        static void Main(string[] args) {
            string[] lines = System.IO.File.ReadAllLines("input.txt");

            List<Venue> venueList = new List<Venue>();
            List<Tournament> tournamentList = new List<Tournament>();
            List<Competitor> competitorList = new List<Competitor>();
            List<MeleeCharacter> characterList = new List<MeleeCharacter>();

            int parsingMode = -1;

            // Store info we come across until object creation
            Tournament outputTournament = new Tournament();
            MatchGroup outputMG = null;
            bool bo5mode = false;
            bool bo1mode = false;

            MeleeCharacter unknownCharacter = new MeleeCharacter("Unknown");
            characterList.Add(unknownCharacter);

            foreach (string s in lines) {
                Console.WriteLine(s);

                // Detect mode changes
                if (s.Contains("<venues>")) parsingMode = MODE_VENUE;
                if (s.Contains("{")) parsingMode = MODE_TOURNAMENT;
                if (s.Contains("<placings>")) parsingMode = MODE_PLACING;
                if (s.Contains("<characters>")) parsingMode = MODE_CHARACTERS;

                switch (parsingMode) {
                    case MODE_CHARACTERS:
                        // Check competitor list for this player; if it doesn't exist yet, add it and assign them their main
                        if (s.IndexOf("|") > 0) {
                            string characterName = s.Substring(s.IndexOf("|") + 1);
                            MeleeCharacter outputCharacter = null;
                            foreach(MeleeCharacter mc in characterList) {
                                if (mc.GetName() == characterName) outputCharacter = mc;
                            }

                            if(outputCharacter == null) {
                                outputCharacter = new MeleeCharacter(characterName);
                                characterList.Add(outputCharacter);
                            }

                            string playerName = s.Substring(s.IndexOf("\t") + 1, s.Substring(s.IndexOf("\t") + 1).IndexOf("|"));
                            bool playerExists = false;
                            foreach(Competitor c in competitorList) {
                                if (c.GetTag() == playerName) {
                                    playerExists = true;
                                    c.SetDefaultCharacter(outputCharacter);
                                }
                            }

                            if(!playerExists) {
                                Competitor newCompetitor = new Competitor(playerName);
                                newCompetitor.SetDefaultCharacter(outputCharacter);
                                competitorList.Add(newCompetitor);
                            }
                        }

                        if (s.Contains("</characters>")) parsingMode = MODE_SEARCHING;
                        break;

                    case MODE_VENUE:
                        Venue outputVenue = new Venue();
                        if (s.IndexOf("|") > 0) {
                            outputVenue.SetName(s.Substring(s.IndexOf("\t") + 1, s.Substring(s.IndexOf("\t") + 1).IndexOf("|")));
                            outputVenue.SetAddress(s.Substring(s.IndexOf("|") + 1));
                            venueList.Add(outputVenue);
                        }

                        // End venue parsing if </venue> is found
                        if (s.Contains("</venues>")) parsingMode = MODE_SEARCHING;

                        break;

                    case MODE_TOURNAMENT:
                        // Assign attributes
                        if (s.Contains("name:")) outputTournament.SetName(s.Substring(s.IndexOf("\"") + 1, s.Substring(s.IndexOf("\"") + 1).IndexOf("\"")));
                        if (s.Contains("date:")) outputTournament.SetDate(s.Substring(s.IndexOf(":") + 1));
                        if (s.Contains("url:")) outputTournament.SetURL(s.Substring(s.IndexOf(":") + 1));

                        // Search venue list for associated venue object
                        if (s.Contains("venue:")) {
                            string venueName = s.Substring(s.IndexOf("\"") + 1, s.Substring(s.IndexOf("\"") + 1).IndexOf("\""));
                            foreach (Venue v in venueList) if (v.GetName() == venueName) outputTournament.SetVenue(v);
                            if (outputTournament.GetVenue() == null) Console.WriteLine("Venue '" + venueName + "' is missing from directory!");
                        }

                        // Search developing tournament list for the parent tournament
                        if (s.Contains("parent:")) {
                            string parentName = s.Substring(s.IndexOf("\"") + 1, s.Substring(s.IndexOf("\"") + 1).IndexOf("\""));

                            foreach (Tournament t in tournamentList) {
                                if (t.GetName() == parentName) {
                                    outputTournament.SetParentTournament(t);

                                    // Inherited attributes
                                    outputTournament.SetDate(t.GetDate());
                                    outputTournament.SetVenue(t.GetVenue());
                                }
                            }

                            if (outputTournament.GetParentTournament() == null) {
                                Console.WriteLine("Parent tournament '" + parentName + "' was not found!");
                            }
                        }

                        // Submit current output tournament and exit this parse mode if bracket close is found
                        if (s.Contains("}")) {
                            tournamentList.Add(outputTournament);

                            foreach(Competitor c in competitorList) {
                                c.bufferedWins = 0;
                                c.bufferedLosses = 0;
                            }

                            // Reset output tournament
                            outputTournament = new Tournament();

                            // Reset search mode
                            parsingMode = MODE_SEARCHING;
                        }

                        if (s.Contains("<rr>") || s.Contains("<bracket>")) {
                            outputMG = new MatchGroup();
                        }

                        // Perhaps move this to its own parse mode, but this should be fine for now?
                        if (s.Contains("[")) {
                            if (outputMG == null) {
                                Console.WriteLine("Tournament '" + outputTournament.GetName() + "' tried to add match outside of MatchGroup; check for missing <rr> or <bracket>");
                            } else {
                                Match outputMatch = new Match();

                                string winnerPlayerName = s.Substring(s.IndexOf("[") + 1, s.Substring(s.IndexOf("[") + 1).IndexOf("]"));

                                MeleeCharacter winnerCharacterUsed = null;
                                MeleeCharacter loserCharacterUsed = null;

                                if (winnerPlayerName.Contains("|")) {
                                    string winnerCharacterUsedName = winnerPlayerName.Substring(winnerPlayerName.IndexOf("|") + 1);
                                    foreach(MeleeCharacter mc in characterList) {
                                        if (mc.GetName() == winnerCharacterUsedName) winnerCharacterUsed = mc;
                                    }

                                    if(winnerCharacterUsed == null) {
                                        winnerCharacterUsed = new MeleeCharacter(winnerCharacterUsedName);
                                        characterList.Add(winnerCharacterUsed);
                                    }

                                    winnerPlayerName = winnerPlayerName.Substring(0, winnerPlayerName.IndexOf("|"));
                                }

                                // Grab score, need to tokenize it for scores still
                                string score = s.Substring(s.IndexOf("]") + 2, (s.Substring(s.IndexOf("]") + 2).IndexOf(" ")));
                                int gamesWonByWinner = Int32.Parse(score.Substring(0,1));
                                int gamesWonByLoser = Int32.Parse(score.Substring(2));

                                string loserPlayerName = s.Substring(s.IndexOf("]") + 7);
                                if (loserPlayerName.Contains("|")) {
                                    string loserCharacterUsedName = loserPlayerName.Substring(loserPlayerName.IndexOf("|") + 1, loserPlayerName.Substring(loserPlayerName.IndexOf("|") + 1).IndexOf("]"));
                                    foreach (MeleeCharacter mc in characterList) {
                                        if (mc.GetName() == loserCharacterUsedName) loserCharacterUsed = mc;
                                    }

                                    if (loserCharacterUsed == null) {
                                        loserCharacterUsed = new MeleeCharacter(loserCharacterUsedName);
                                        characterList.Add(loserCharacterUsed);
                                    }

                                    loserPlayerName = loserPlayerName.Substring(0, loserPlayerName.IndexOf("|"));    // Needs to grab character overrides
                                } else loserPlayerName = loserPlayerName.Substring(0, loserPlayerName.IndexOf("]"));

                                // Check competitor list for this tag, assign objects if found
                                Competitor winnerCompetitor = null;
                                Competitor loserCompetitor = null;
                                foreach (Competitor c in competitorList) {
                                    if (c.GetTag() == winnerPlayerName) winnerCompetitor = c;
                                    if (c.GetTag() == loserPlayerName) loserCompetitor = c;
                                }

                                // If competitor list does not contain these tags, create a new competitor
                                if (winnerCompetitor == null) {
                                    Competitor newCompetitor = null;
                                    newCompetitor = new Competitor(winnerPlayerName);

                                    // Link secondaries account to their main account
                                    if (winnerPlayerName[0] == '-') {
                                        Competitor mainAccount = null;
                                        foreach (Competitor c in competitorList) {
                                            if (c.GetTag() == winnerPlayerName.Substring(1)) mainAccount = c;
                                        }

                                        if (mainAccount == null) {
                                            mainAccount = new Competitor(winnerPlayerName.Substring(1));
                                            competitorList.Add(mainAccount);
                                        }

                                        newCompetitor.SetMainAccount(mainAccount);
                                    }

                                    competitorList.Add(newCompetitor);
                                    winnerCompetitor = newCompetitor;
                                }

                                if (loserCompetitor == null) {
                                    Competitor newCompetitor = new Competitor(loserPlayerName);

                                    // Link secondaries account to their main account
                                    if (loserPlayerName[0] == '-') {
                                        Competitor mainAccount = null;
                                        foreach (Competitor c in competitorList) {
                                            if (c.GetTag() == loserPlayerName.Substring(1)) mainAccount = c;
                                        }

                                        if (mainAccount == null) {
                                            mainAccount = new Competitor(loserPlayerName.Substring(1));
                                            competitorList.Add(mainAccount);
                                        }

                                        newCompetitor.SetMainAccount(mainAccount);
                                    }

                                    competitorList.Add(newCompetitor);
                                    loserCompetitor = newCompetitor;
                                }

                                if (winnerCharacterUsed == null) {
                                    if (winnerCompetitor.GetDefaultCharacter() == null)
                                        winnerCharacterUsed = unknownCharacter;
                                    else winnerCharacterUsed = winnerCompetitor.GetDefaultCharacter();
                                }

                                if (loserCharacterUsed == null) {
                                    if (loserCompetitor.GetDefaultCharacter() == null)
                                        loserCharacterUsed = unknownCharacter;
                                    else loserCharacterUsed = loserCompetitor.GetDefaultCharacter();
                                }

                                winnerCompetitor.AddResult(loserCompetitor, true, winnerCharacterUsed, gamesWonByWinner, gamesWonByLoser);
                                loserCompetitor.AddResult(winnerCompetitor, false, loserCharacterUsed, gamesWonByLoser, gamesWonByWinner);

                                outputMatch.SetWinner(winnerCompetitor, winnerCharacterUsed);
                                outputMatch.SetLoser(loserCompetitor, loserCharacterUsed);

                                if (bo5mode) outputMatch.SetWinCondition(3);
                                if (bo1mode) outputMatch.SetWinCondition(1);

                                outputMG.matches.Add(outputMatch);
                            }
                        }

                        // End match group
                        if (s.Contains("</rr>") || s.Contains("</bracket>")) {
                            outputTournament.AddMatchGroup(outputMG);
                            outputMG = null;
                        }

                        // Toggle bo5
                        if (s.Contains("<bo5>")) bo5mode = true;
                        if (s.Contains("</bo5>")) bo5mode = false;

                        // Toggle bo1
                        if (s.Contains("<bo1>")) bo1mode = true;
                        if (s.Contains("</bo1")) bo1mode = false;

                        break;

                    case MODE_PLACING:
                        if (s.Contains("</placings>")) {
                            parsingMode = MODE_TOURNAMENT;
                            break;
                        }

                        if (s.Contains("<placings>")) break;

                        Placing outputPlacing = new Placing();
                        if (s.IndexOf("|") > 0) {
                            string placingPlayerName = s.Substring(s.IndexOf("|") + 1);

                            foreach(Competitor c in competitorList) {
                                if (c.GetTag() == placingPlayerName) {
                                    outputPlacing.SetCompetitor(c);
                                    outputPlacing.SetWins(c.bufferedWins);
                                    outputPlacing.SetLosses(c.bufferedLosses);
                                }
                            }

                            if (outputPlacing.GetCompetitor() == null) Console.WriteLine(placingPlayerName + " not found");

                            outputPlacing.SetPlacingText(s.Substring(s.IndexOf("\t") + 1, s.Substring(s.IndexOf("\t") + 1).IndexOf("|")));
                            outputPlacing.SetTournament(outputTournament);

                            outputPlacing.GetCompetitor().AddPlacing(outputPlacing);
                            outputTournament.AddPlacing(outputPlacing);
                        }

                        break;
                }
            }

            // Assign PR
            foreach (Competitor c in competitorList) {
                switch(c.GetTag()) {
                    case "Ginger": 
                        c.SetRating(2000f, true);
                        c.SetTier(0);
                        break;
                    case "KJH": // KJH was #2 on the last PR he was on
                    case "Morsecode762":
                        c.SetRating(1950f, true);
                        c.SetTier(0);
                        break;
                    case "@Heartstrings":
                        c.SetRating(1900f, true);
                        c.SetTier(0);
                        break;
                    case "tentpoles":
                        c.SetRating(1850f, true);
                        c.SetTier(0);
                        break;
                    case "JCubez":
                        c.SetRating(1800f, true);
                        c.SetTier(0);
                        break;
                    case "baka4moe":
                        c.SetRating(1750f, true);
                        c.SetTier(0);
                        break;
                    case "Butterdonkey":
                        c.SetRating(1700f, true);
                        c.SetTier(0);
                        break;
                    case "Kuyashi":
                        c.SetRating(1650f, true);
                        c.SetTier(0);
                        break;
                    case "Ossify":
                        c.SetRating(1600f, true);
                        c.SetTier(0);
                        break;
                    case "Wishblade":
                        c.SetRating(1550f, true);
                        c.SetTier(0);
                        break;
                }
            }

            int lowestTier = 0;
            for (int i = 0; i < 20; i++) {
                foreach (Competitor c in competitorList) {
                    if(c.GetTier() == -1) c.CheckForPromotionWins(i);
                    if (c.GetTier() > lowestTier) lowestTier = c.GetTier();
                }
            }

            // Competitors with tier -1 still are set to the bottomest class
            foreach (Competitor c in competitorList) {
                if (c.GetTier() == -1) {
                    c.SetTier(lowestTier + 1);
                    c.SetRating(1500f - ((lowestTier + 1) * 100f), true);
                }
            }

            Console.WriteLine(lowestTier);

            // Elo exchange
            foreach (Tournament t in tournamentList) {
                // Update out of state participants
                t.UpdateOutOfStateAttendants();

                // Buffer all competitors' ranks
                foreach(Competitor c in competitorList) {
                    c.bufferedRating = c.GetRating();
                }

                // First, generate a "tournament weight": combine elos of all present participants
                // Higher weiht
                float kFactor = t.GetRatingWeight() * (0.0025f * 0.334f);
                Console.WriteLine(t.GetName() + ": " + kFactor + " / 20");

                foreach(MatchGroup mg in t.GetAllMatches()) {
                    foreach (Match m in mg.matches) {
                        // Prevent negative kfactor (yes it can happen) and apply a cap
                        // Also apply win condition; in layman's terms, best-of-5 has a higher k factor than best-of-3s because they are harder to win
                        float effectiveKFactor = MathF.Min(20f, MathF.Max(0f,kFactor));
                        effectiveKFactor *= (float)(m.GetWinCondition()) / 2f;

                        // Elo rating math
                        // GetRating(true) will call main account's rating if this player is using secondaries; players who lose to secondaries will be deducted coints equal to losing to their opponent's main
                        float winnerTransformedRating = MathF.Pow(10f, m.GetWinner().GetRating() / 400f);
                        float loserTransformedRating = MathF.Pow(10f, m.GetLoser().GetRating() / 400f);

                        float winnerExpectedScore = winnerTransformedRating / (winnerTransformedRating + loserTransformedRating);
                        float loserExpectedScore = loserTransformedRating / (winnerTransformedRating + loserTransformedRating);

                        float winnerPlayerResultingScore = m.GetWinner().bufferedRating + (effectiveKFactor * (1-winnerExpectedScore));
                        float loserPlayerResultingScore = m.GetLoser().bufferedRating + (effectiveKFactor * (0 - loserExpectedScore));

                        float winnerRatingChange = winnerPlayerResultingScore - m.GetWinner().bufferedRating;
                        float loserRatingChange = loserPlayerResultingScore - m.GetLoser().bufferedRating;

                        // Adjust player rating
                        m.GetWinner().IncrementBufferRating(winnerRatingChange);
                        m.GetLoser().IncrementBufferRating(loserRatingChange);

                        // Adjust player character matchup performance
                        m.GetWinner().AdjustCharacterMatchup(m.GetLoserCharacterUsed(), winnerRatingChange);
                        m.GetLoser().AdjustCharacterMatchup(m.GetWinnerCharacterUsed(), loserRatingChange);

                        // Adjust global character matchup data
                        m.GetWinnerCharacterUsed().AdjustCharacterMatchup(m.GetLoserCharacterUsed(), winnerRatingChange);
                        m.GetLoserCharacterUsed().AdjustCharacterMatchup(m.GetWinnerCharacterUsed(), loserRatingChange);
                    }
                }

                // Change rating to buffer after all matches have been recorded
                foreach (Competitor c in competitorList) {
                    // Update placing coint exchange
                    foreach(Placing p in c.GetPlacings()) {
                        if(p.GetTournament() == t) {
                            p.SetRatingChange(c.bufferedRating - c.GetRating());
                        }
                    }

                    c.SetRating(c.bufferedRating);
                }
            }

            // Bubble sort competitors by rating
            for (int write = 0; write < competitorList.Count; write++) {
                for (int sort = 0; sort < competitorList.Count - 1; sort++) {
                    if(competitorList[sort].GetRating() < competitorList[sort+1].GetRating()) {
                        Competitor temp = competitorList[sort + 1];
                        competitorList[sort + 1] = competitorList[sort];
                        competitorList[sort] = temp;
                    }
                }
            }

            // Find number of players per character (60% usage of a character counts as 0.6)
            foreach(Competitor c in competitorList) {
                foreach(CharacterUsage cu in c.GetCharacterUsage()) {
                    cu.GetCharacter().AddMain((float)(cu.GetWins() + cu.GetLosses()) / (float)(c.GetSetWinCount() + c.GetSetLossCount()));
                }
            }

            // Style sheet, navbar potentially
            string header = "<!doctype html><html lang=\"en\">";
            header += "<head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>CoinSheet-3 WIP</title><style>table, th, td { border: 1px solid black; } </style><link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-F3w7mX95PdgyTmZZMECAngseQB83DfGTowi0iMjiWaeVhAn4FJkqJByhZMI3AhiU\" crossorigin=\"anonymous\"></head>";
            header += "<body>";

            using (StreamWriter writer = new StreamWriter("simple.php")) {
                writer.WriteLine(header);
                writer.WriteLine("<table class=\"table\"><thead><tr><td>#</td><td>Tag</td><td>Coints</td><td>Tournaments</td><td>Notable Records</td><td>Character Usage</td><td>Strong Matchups</td><td>Weak Matchups</td></thead>");

                int rankingNumber = 0;
                for (int i = 0; i < competitorList.Count; i++) {
                    if (competitorList[i].GetTag() == "HAM") competitorList[i].incoinito = true;


                    if(!competitorList[i].incoinito && !competitorList[i].outOfState && !competitorList[i].GetTag().Contains("Artemis Peach")) {
                        rankingNumber++;

                        // Rank, tag, rating, starting rating
                        writer.WriteLine("<tr><td>" + rankingNumber + "</td><td>" + competitorList[i].GetTag() + "</td><td>" + String.Format("{0:n0}",MathF.Round(competitorList[i].GetRating()*10f))+"<br><br>"+competitorList[i].GetTierName()+": "+ String.Format("{0:n0}", MathF.Round(competitorList[i].GetStartingRating()*10f))+"</td><td>");
                        
                        // Placings list
                        for(int j = competitorList[i].GetPlacings().Count-1; j > -1; j--) {
                            Placing p = competitorList[i].GetPlacings()[j];
                            string ratingChangeText = "(secondaries)";
                            if (p.GetCompetitor().GetTag()[0] != '-') {
                                if (p.GetCompetitor().incoinito) ratingChangeText = "";
                                else ratingChangeText = "("+(p.GetRatingChange() >= 0 ? "+" : "") + MathF.Round(p.GetRatingChange() * 10f)+")";
                            }

                            writer.WriteLine(p.GetTournament().GetName() + ": " + p.GetPlacingText() + " out of " + p.GetTournament().GetAllCompetitors().Count + " "+ratingChangeText + "<br>");
                        }

                        writer.WriteLine("</td><td>");

                        // Sort records by rank, then list them out
                        foreach(Record r in competitorList[i].GetRecords()) {
                            bool recordIsNotable = false;
                            if(r.GetOpponent().GetRating() > competitorList[i].GetRating()) {
                                // Any winning record on a player with a higher rating is considered notable
                                if (r.GetSetWins() >= r.GetSetLosses()) recordIsNotable = true;

                                // Otherwise, decide if win rate is notable based on difference in rating
                                float ratingDifference = r.GetOpponent().GetRating() - competitorList[i].GetRating();
                                float winRate = (float)(r.GetSetWins()) / (r.GetSetWins() + r.GetSetLosses());
                                if (winRate > (800f - MathF.Min(800f,ratingDifference)) / 1600f) recordIsNotable = true;
                            } else {
                                float ratingDifference = competitorList[i].GetRating() - r.GetOpponent().GetRating();
                                if ((ratingDifference <= 100f || r.GetOpponent().GetTier() == 0) && r.GetSetWins() > r.GetSetLosses()) recordIsNotable = true;
                            }

                            // Cancel notable if they're incoinito or a secondaries account
                            if (r.GetOpponent().incoinito || r.GetOpponent().GetMainAccount() != null) recordIsNotable = false;

                            if(recordIsNotable) writer.WriteLine(r.GetSetWins() + "-" + r.GetSetLosses() + " vs. "+r.GetOpponent().GetTag()+"<br>");
                        }

                        writer.WriteLine("</td><td>");

                        // Character usage rate
                        foreach (CharacterUsage cu in competitorList[i].GetCharacterUsage()) {
                            writer.WriteLine(cu.GetCharacter().GetName() + ": " + MathF.Round((float)(cu.GetWins() + cu.GetLosses()) / (float)(competitorList[i].GetSetWinCount() + competitorList[i].GetSetLossCount()) * 100f)+"%<br>");
                        }

                        writer.WriteLine("</td><td>");

                        // Sort matchups by strength
                        competitorList[i].SortCharacterMatchups();

                        // Strong matchups
                        foreach(CharacterMatchup cm in competitorList[i].GetCharacterMatchups()) {
                            if(cm.GetCharacter() != unknownCharacter && cm.GetRatingExchange() > 0f)
                            writer.WriteLine("vs. " + cm.GetCharacter().GetName() + ": " + (cm.GetRatingExchange() >= 0f ? "+" : "") + String.Format("{0:n0}", MathF.Round(cm.GetRatingExchange() * 10f))+"<br>");
                        }

                        writer.WriteLine("</td><td>");

                        // Weak matchups
                        foreach (CharacterMatchup cm in competitorList[i].GetCharacterMatchups()) {
                            if (cm.GetCharacter() != unknownCharacter && cm.GetRatingExchange() < 0f)
                                writer.WriteLine("vs. " + cm.GetCharacter().GetName() + ": " + (cm.GetRatingExchange() >= 0f ? "+" : "") + String.Format("{0:n0}", MathF.Round(cm.GetRatingExchange() * 10f)) + "<br>");
                        }

                        writer.WriteLine("</td></tr>");
                    }
                }

                writer.WriteLine("</table>");
            }

            using (StreamWriter writer = new StreamWriter("tournaments.php")) {
                writer.WriteLine(header);
                writer.WriteLine("<table class=\"table\"><thead><tr><td>Date</td><td>Tournament<td>Placings</td></tr></thead>");

                for (int i = tournamentList.Count - 1; i > -1; i--) {
                    writer.WriteLine("<tr>");
                    writer.WriteLine("<td>" + tournamentList[i].GetDate() + "</td><td><a href=\"" + tournamentList[i].GetURL() + "\">" + tournamentList[i].GetName() + "</a><br><br>Entrants: " + tournamentList[i].GetAllCompetitors().Count + "<br>Weight: " + String.Format("{0:n2}", (tournamentList[i].GetRatingWeight() * (0.0025f * 0.334f))) + " / 20</td><td>");
                    foreach (Placing p in tournamentList[i].GetPlacings()) {
                        string ratingChangeText = "(secondaries)";
                        if (p.GetCompetitor().GetTag()[0] != '-') {
                            if (p.GetCompetitor().incoinito) ratingChangeText = "";
                            else ratingChangeText = "(" + (p.GetRatingChange() >= 0 ? "+" : "") + MathF.Round(p.GetRatingChange() * 10f) + ")";
                        }

                        writer.WriteLine(p.GetPlacingText() + ". " + p.GetCompetitor().GetTag() + " " + ratingChangeText + "<br>");
                    }
                    writer.WriteLine("</td></tr>");
                }

                writer.WriteLine("</table></body>");
            }

            using(StreamWriter writer = new StreamWriter("characterUsage.php")) {
                writer.WriteLine(header);

                // Bubble sort character list by number of mains
                for (int write = 0; write < characterList.Count; write++) {
                    for (int sort = 0; sort < characterList.Count - 1; sort++) {
                        if (characterList[sort].GetNumberOfMains() < characterList[sort + 1].GetNumberOfMains()) {
                            MeleeCharacter temp = characterList[sort + 1];
                            characterList[sort + 1] = characterList[sort];
                            characterList[sort] = temp;
                        }
                    }
                }

                // Table will print a header for every character in the list
                writer.WriteLine("<table class=\"table\"><thead><tr><td><b>Matchup for...</b></td>");
                foreach (MeleeCharacter mc in characterList) {
                    if (mc.GetName() != "Unknown") {
                        writer.WriteLine("<td>"+mc.GetName() + "</td>");
                    }
                }
                writer.WriteLine("</tr></thead>");

                // Character rows
                foreach(MeleeCharacter mc in characterList) {
                    if (mc.GetName() != "Unknown") {
                        writer.WriteLine("<tr><td>" + mc.GetName() + " (" + mc.GetNumberOfMains()+") </td>");
                        foreach (MeleeCharacter mc2 in characterList) {
                            writer.WriteLine("<td>");

                            foreach(CharacterMatchup mu in mc.GetCharacterMatchups()) {
                                if(mu.GetCharacter() == mc2) {
                                    writer.WriteLine(mu.GetRatingExchange());
                                }
                            }

                            writer.WriteLine("</td>");
                        }

                        writer.WriteLine("</tr>");
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter("uofm_datagen")) {
                string playerQuery = "Wendy";
                List<string> uOfMTags = new List<string>();
                List<Competitor> prData_Candidates = new List<Competitor>();
                List<Competitor> prData_DataPoints = new List<Competitor>();
                List<Competitor> prData_Other = new List<Competitor>();

                uOfMTags.Add("Negat!ve");
                uOfMTags.Add("Paco");
                uOfMTags.Add("gpie");
                uOfMTags.Add("Butterdonkey");
                uOfMTags.Add("Morsecode762");
                uOfMTags.Add("mathandsurf");
                uOfMTags.Add("morc");
                uOfMTags.Add("Cash Cow");
                uOfMTags.Add("Mushu");
                uOfMTags.Add("A2Quizzy");
                uOfMTags.Add("Orbit");
                uOfMTags.Add("Polymath");
                uOfMTags.Add("J$");
                uOfMTags.Add("Ladies");
                uOfMTags.Add("Wendy");
                uOfMTags.Add("AlphaPancake");

                // Find the player we're querying and associate competitors with tags we want to study
                Competitor competitorQuery = null;
                foreach (Competitor c in competitorList) {
                    if (c.GetTag() == playerQuery) competitorQuery = c;
                    if (uOfMTags.Contains(c.GetTag())) prData_Candidates.Add(c);
                }

                // Second go, this time identify all players who are NOT a u of m candidate but have a set win over a u of m candidate (shared data point)
                foreach (Competitor c in competitorList) {
                    if(!prData_Candidates.Contains(c)) {
                        int candidateWins = 0;
                        int candidateLosses = 0;
                        foreach(Record r in c.GetRecords()) {
                            if(prData_Candidates.Contains(r.GetOpponent())) {
                                if(r.GetSetWins() > 0) candidateWins++;
                                if(r.GetSetLosses() > 0) candidateLosses++;
                            }
                        }

                        if (candidateWins > 0 && candidateLosses > 0) prData_DataPoints.Add(c);
                    }
                }

                writer.WriteLine(playerQuery + "\n\nTournaments:");
                foreach(Placing p in competitorQuery.GetPlacings()) {
                    writer.WriteLine(p.GetTournament().GetName() + "\t" + p.GetPlacingText());
                }

                writer.WriteLine("\nVs Other Candidates:");
                foreach(Record r in competitorQuery.GetRecords()) {
                    if(prData_Candidates.Contains(r.GetOpponent())) writer.WriteLine(r.GetOpponent().GetTag()+"\t"+ r.GetSetWins()+"-"+r.GetSetLosses());
                }

                writer.WriteLine("\nVs Data Points Over Other Candidates:");
                foreach (Record r in competitorQuery.GetRecords()) {
                    if (prData_DataPoints.Contains(r.GetOpponent())) writer.WriteLine(r.GetOpponent().GetTag() + "\t" + r.GetSetWins() + "-" + r.GetSetLosses());
                }

                writer.WriteLine("\nVs Misc:");
                foreach (Record r in competitorQuery.GetRecords()) {
                    if (!prData_Candidates.Contains(r.GetOpponent()) && !prData_DataPoints.Contains(r.GetOpponent())) writer.WriteLine(r.GetOpponent().GetTag() + "\t" + r.GetSetWins() + "-" + r.GetSetLosses());
                }
            }
        }
    }
}
