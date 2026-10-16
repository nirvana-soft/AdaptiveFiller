using DocumentFormat.OpenXml.Wordprocessing;
using HyakuninKumite.Services;
using HyakuninKumiteApplication.Entities.Category;
using HyakuninKumiteApplication.Models.Enums;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static HyakuninKumiteApplication.Entities.AgeGroup;
using static HyakuninKumiteApplication.Entities.AgeGroup.Division;

namespace HyakuninKumiteApplication.Entities
{
    public class CategoryGroup
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public List<Division> Divisions { get; private set; } = new List<Division>();

        public CategoryGroup()
        {

        }

        public void MatchmakeParticipants()
        {
            foreach (Division div in Divisions)
            {
                div.MatchmakeDivisionParticipants();
            }
        }

        public void EnumerateMatches()
        {
            List<Division> divs = Divisions;

            List<WeightCategory> wcs = divs.SelectMany(div => div.WeightCategories).ToList();
            List<WeightCategory> wcorder = wcs.OrderByDescending(wc => wc.Rounds.Count).ToList();

            if (wcs.Count == 0)
                return;

            int roundDepth = wcorder[0].Rounds.Count;

            foreach (var wc in wcorder)
            {
                for (int i = 0; i < wc.Rounds.Count; i++)
                {
                    var matches = wc.Rounds[i].Matches;

                    foreach (Match m in matches)
                    {
                        m.UniversalRound = -wc.Rounds.Count + roundDepth + m.RelativeRound;
                    }
                }

                if(wc.ConsolationMatch != null)
                    wc.ConsolationMatch.UniversalRound = wc.FinalMatch.UniversalRound;
            }

            int count = 0;
            for (int i = 0; i < roundDepth; i++)
            {
                List<Match> matches = wcs
                    .Where(wc => wc.Rounds.Count >= (roundDepth - i))
                    .SelectMany(wc => wc.Rounds[wc.Rounds.Count - roundDepth + i].Matches)
                    .ToList();

                if(i == roundDepth - 1)//lastround
                {
                    List<Match> consmatches = wcs.Where(wc => wc.HasConsolationMatch).Select(wc => wc.ConsolationMatch).ToList();
                    matches.InsertRange(0, consmatches);
                }

                //For Round 1 dont count half matches count only full matches
                foreach (Match m in matches)
                {
                    if (m.GetMatchType() == MatchType.FullMatch || m.RelativeRound != 0 || m.MatchClass == MatchClass.Consolation)
                    {
                        count++;
                        m.MatchCompNumber = count;
                    }
                    else
                    {
                        m.MatchCompNumber = 0;
                    }
                }
            }
        }

        public List<Match> AllMatches => WeightCategories
            .Where(wc => wc.Rounds.Count >= (1))
            .SelectMany(wc => wc.Rounds.SelectMany(r => r.Matches))
            .ToList();

        public List<WeightCategory> WeightCategories => Divisions.SelectMany(div => div.WeightCategories).ToList();

        public void AddDivision(Division div)
        {
            Divisions.Add(div);
            div.CategoryGroupName = Name;
            EnumerateMatches();
        }

        public void RemoveDivision(Division div) 
        {
            div.CategoryGroupName = "";
            Divisions.Remove(div);
            EnumerateMatches();
        }

        public void ClearDivisions()
        {
            foreach(var d in Divisions)
            {
                RemoveDivision(d);
            }
        }
    
        public void SetDivisions(List<Division> divisions)
        {
            Divisions.Clear();
            Divisions.AddRange(divisions);
            EnumerateMatches();
        }
    }
}
