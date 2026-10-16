using System;
using System.Collections.Generic;
using System.Text;
using HyakuninKumiteApplication.Models.Enums;

namespace HyakuninKumiteApplication.Entities
{
    public class Tree
    {
        public List<Match> Matches { get; set; } = new List<Match>();

        public void EmptyMatches()
        {
            foreach(Match m in Matches)
            {
                m.Pair[0] = new Participant { PartType = PartType.NullPart };
                m.Pair[1] = new Participant { PartType = PartType.NullPart };
            }
        }

        public void PrintMatches()
        {
            Console.WriteLine($"=== Tree Matches ({Matches.Count}) ===");

            for (int i = 0; i < Matches.Count; i++)
            {
                var m = Matches[i];
                string matchName = string.IsNullOrEmpty(m.Name) ? $"Match {i + 1}" : m.Name;

                string p1Str = GetParticipantDescription(m.Pair?[0]);
                string p2Str = GetParticipantDescription(m.Pair?[1]);

                Console.WriteLine($"[{matchName}] ({m.MatchClass}) -> {p1Str}  VS  {p2Str}");
            }
        }

        private string GetParticipantDescription(Participant p)
        {
            if (p == null) return "Null";

            switch (p.PartType)
            {
                case PartType.NullPart:
                    return "[Empty / Null]";
                case PartType.PlaceHolderPart:
                    return "[Placeholder]";
                default:
                    // Fallback to name/country if available, otherwise general description
                    string identifier = !string.IsNullOrEmpty(p.Name) ? $"{p.Name}" : "Competitor";
                    return $"{identifier}";
            }
        }
    }
}
