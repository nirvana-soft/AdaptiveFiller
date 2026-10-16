using HyakuninKumiteApplication.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Range = HyakuninKumiteApplication.Models.Range;
using Newtonsoft.Json;

namespace HyakuninKumiteApplication.Entities.Category
{
    public class WeightCategory : Category
    {
        public Range WeightRange { get; set; }

        public List<Tree> Rounds { get; set; } = new List<Tree>();
        public Match ConsolationMatch { get; set; }

        public bool HasConsolationMatch => Parts.Count > 3;

        [JsonIgnore]
        public List<Match> AllMatches
        {
            get
            {
                var matches = Rounds.SelectMany(r => r.Matches).ToList();

                if (ConsolationMatch != null)
                {
                    matches.Add(ConsolationMatch);
                }

                return matches;
            }
        }

        public WeightCategory(int minWeight, int maxWeight)
        {
            this.WeightRange = new Range(minWeight, maxWeight);

            InitializeResults();
        }

        public void Matchmake()
        {
            Rounds.Clear();
            Rounds = MMS.GenerateRounds(Parts);

            if (Rounds.Count == 0)
                return;

            Rounds[0] = MMS.GenerateMatchMakings(Parts);

            if (HasConsolationMatch)
            {
                ConsolationMatch = new Match { MatchClass = MatchClass.Consolation };
                ConsolationMatch.InitializePair();
                ConsolationMatch.RelativeRound = FinalMatch.RelativeRound;
                ConsolationMatch.UniversalRound = FinalMatch.UniversalRound;
            }

            AutoQualify();
        }

        public void AutoQualify()
        {
            List<Tree> idealRounds = Rounds;

            for (int i = 0; i < idealRounds[0].Matches.Count; i++)
            {
                Match m = idealRounds[0].Matches[i];
                Participant part = m.GetRealParticipant;

                if (part != null)
                {
                    QualifyPart(m, part);
                }
            }
        }
        public void QualifyPart(Match m, Participant part)
        {
            var nxtmatch = NextMatchOf(m);
            int partIndex = MatchIndexOf(m) % 2;

            if (nxtmatch != null)
            {
                if (HasConsolationMatch && nxtmatch.MatchClass == MatchClass.Final)
                {
                    Console.WriteLine($"pi = {partIndex}, pc = {ConsolationMatch.Pair.Count}");
                    ConsolationMatch.Pair[partIndex] = m.OpponentOf(part);
                }

                m.Winner = part;
                nxtmatch.Pair[partIndex] = part;
            }
            else
            {
                if (m.MatchClass == MatchClass.Consolation)
                {
                    m.Winner = part;
                    Result[2] = part;
                    Result[3] = m.OpponentOf(part);
                }
                else if (m.MatchClass == MatchClass.Final)
                {
                    m.Winner = part;
                    Result[0] = part;
                    Result[1] = m.OpponentOf(part);
                }
            }
        }

        public void ResetAdvancements()
        {
            for (int i = 1; i + 1 < Rounds.Count; i++)
            {
                Rounds[i].EmptyMatches();
            }
            foreach (Match m in Rounds[0].Matches)
            {
                m.Winner = null;
            }

            AutoQualify();
        }

        #region Match Related
        public int MatchIndexOf(Match m)
        {
            return Rounds[m.RelativeRound].Matches.IndexOf(m);
        }
        public Match NextMatchOf(Match m)
        {
            if (HasConsolationMatch)
            {
                if (m.MatchClass == MatchClass.Regular)
                    return Rounds[m.RelativeRound + 1].Matches[MatchIndexOf(m) / 2];
                else if (m.MatchClass == MatchClass.SemiFinal)
                    return FinalMatch;
                else if (m.MatchClass == MatchClass.Consolation)
                    return null;
                else if (m.MatchClass == MatchClass.Final)
                    return null;

            }
            else if (m.RelativeRound + 1 < Rounds.Count)
            {
                return Rounds[m.RelativeRound + 1].Matches[MatchIndexOf(m) / 2];
            }

            return null;
        }
        public List<Match> PreMatchesOf(Match m)
        {
            if (m.RelativeRound == 0)
                return null;

            if (m.MatchClass == MatchClass.Regular || m.MatchClass == MatchClass.SemiFinal)
            {
                Tree r = Rounds[m.RelativeRound - 1];
                int index = MatchIndexOf(m) * 2;

                return new List<Match> { r.Matches[index], r.Matches[index + 1] };
            }
            else if (m.MatchClass == MatchClass.Consolation || m.MatchClass == MatchClass.Final)
            {
                if (Rounds.Count < 2)
                    return null;

                Tree r = Rounds[^2];
                return r.Matches;
            }
            else
            {
                return null;
            }
        }

        public Match FinalMatch
        {
            get
            {
                if (Rounds.Count > 0 && Rounds[^1].Matches.Count > 0)
                {
                    return Rounds[^1].Matches[0];
                }
                return null;
            }
        }
        #endregion

        private void InitializeResults()
        {
            Result = new List<Participant>();

            while (Result.Count < 4)
            {
                Result.Add(null);
            }
        }
    }
}
