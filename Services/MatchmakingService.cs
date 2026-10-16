global using MMS = HyakuninKumite.Services.MatchmakingService;

using HyakuninKumiteApplication.Entities;
using System.Windows;
using static HyakuninKumiteApplication.Entities.AgeGroup;
using static HyakuninKumiteApplication.Entities.AgeGroup.Division;
using HyakuninKumiteApplication.Models.Enums;
using HyakuninKumiteApplication.Services;

using HyakuninKumiteApplication.Entities.Category;

using Range = HyakuninKumiteApplication.Models.Range;

namespace HyakuninKumite.Services
{
    public static class MatchmakingService
    {
        #region Matchmaking System
        public static List<Tree> GenerateRounds(List<Participant> parts)
        {
            var (power, exp) = MathEXT.NextPowerOfTwo(parts.Count);
            int idealRootMatchCount = power / 2;
            int idealRoundsCount = exp;

            List<Tree> idealRounds = new List<Tree>();
            for (int i = 0; i < idealRoundsCount; i++)
            {
                idealRounds.Add(null);
            }

            if (idealRounds.Count <= 0)
                return idealRounds;

            for (int i = 0; i < idealRoundsCount; i++)
            {
                int roundMatchesCount = idealRootMatchCount / (int)Math.Pow(2, i);

                MatchClass mClass = MatchClass.Regular;

                if (i == idealRoundsCount - 2)
                    mClass = MatchClass.SemiFinal;
                else if (i == idealRoundsCount - 1)
                    mClass = MatchClass.Final;

                Tree round = InitializeTree(roundMatchesCount, mClass, i);

                idealRounds[i] = round;
            }

            return idealRounds;
        }

        public static Tree GenerateMatchMakings(List<Participant> Parts)
        {
            Console.WriteLine($"Matchmaked {Parts.Count} parts!");

            if (Parts.Count == 0)
                return new Tree();

            var (power, exp) = MathEXT.NextPowerOfTwo(Parts.Count);
            int idealMatchCount = power / 2;
            int idealRoundsCount = exp;

            Tree idealTree = InitializeAdaptableTree(Parts.Count, idealMatchCount);

            if (Parts.Count != 1)
            {
                var usedParticipants = new HashSet<Participant>();

                List<Participant> orgParts = new List<Participant>();

                orgParts = GAFSortBy(Parts, p => p.Country);

                //Segmental Search Placement 
                foreach (var part in orgParts) //var part in group
                {
                    if (usedParticipants.Contains(part))
                        continue;

                    var m = FindEligibleMatch(idealTree, part, p => p.Country);
                    m.Pair[m.AvailableSlot(PartType.PlaceHolderPart)] = part;
                    usedParticipants.Add(part);
                }

                foreach(var m in idealTree.Matches)
                {
                    List<Participant> ps = m.Pair.FindAll(p => p.PartType == PartType.PlaceHolderPart);
                    foreach(var p in ps)
                    {
                        p.PartType = PartType.NullPart;
                    }
                }

                return ReorganizeSameCountryMatches(idealTree, Parts);
            }
            else
            {
                Participant part = Parts.Find(p => p.PartType != PartType.NullPart);
                Match m = idealTree.Matches[0];
                m.Pair = new List<Participant> { part, new Participant { PartType = PartType.NullPart } };

                return idealTree;
            }
        }
        public static Tree ReorganizeSameCountryMatches(Tree originalTree, List<Participant> partList)
        {
            Tree idealTree = new Tree();
            idealTree.Matches.AddRange(originalTree.Matches);

            var countryGroups = partList
                .GroupBy(p => p.Country)
                .OrderByDescending(g => g.Count());

            foreach (var cGroup in countryGroups)
            {
                if (cGroup.All(p => p.PartType == PartType.NullPart))
                    continue;

                var usedParticipants = new HashSet<Participant>();

                //Extract the participants of the same country
                foreach (var part in cGroup)
                {
                    if (usedParticipants.Contains(part))
                        continue;

                    int indexMatch = idealTree.Matches.FindIndex(m => m.Pair.Any(p => p == part));
                    int indexPair = idealTree.Matches[indexMatch].Pair.FindIndex(p => p == part);

                    usedParticipants.Add(part);

                    idealTree.Matches[indexMatch].Pair[indexPair] = new Participant { PartType = PartType.PlaceHolderPart };
                }

                var dPartList = GAFSortBy(cGroup.ToList(), p => p.DOP);
                usedParticipants.Clear();

                foreach (var part in dPartList)
                {
                    if (usedParticipants.Contains(part))
                        continue;

                    var m = FindEligibleMatch(idealTree, part, p => p.DOP);
                    m.Pair[m.AvailableSlot(PartType.PlaceHolderPart)] = part;
                    usedParticipants.Add(part);
                }
                
            }

            return idealTree;
        }

        public static List<Participant> MergeMatchesIntoList(List<Match> matches)
        {
            List<Participant> partList = new List<Participant>();
            foreach (Match m in matches)
            {
                if (m?.Pair[0].isBye == false && m?.Pair[0] != null)
                    partList.Add(m.Pair[0]);
                if (m?.Pair[1].isBye == false && m?.Pair[1] != null)
                    partList.Add(m.Pair[1]);
            }
            return partList;
        }

        //Grouped Adaptive Filling Sorting System
        private static List<Participant> GAFSortBy<Tkey>(List<Participant> parts, Func<Participant, Tkey> keySelector)
        {
            var groups = parts
                .GroupBy(keySelector)
                .OrderByDescending(g => g.Count()).ToList();

            List<Participant> orgParts = new List<Participant>();

            int groupCount = groups[groups.Count() - 1].Count();

            for (int i = 0; i < groupCount; i++)
            {
                var list = new List<Participant>();

                foreach (var c in groups)
                {
                    int segmentcount = (int)MathF.Ceiling((float)c.Count() / groupCount);

                    int range = segmentcount;
                    if (segmentcount * (i + 1) - 1 >= c.Count())
                        range = c.Count() - segmentcount * i;

                    var segmentparts = c.ToList().GetRange(segmentcount * i, range).ToList();

                    list.AddRange(segmentparts);
                }

                orgParts.AddRange(list);
            }

            return orgParts;
        }
        private static Match FindEligibleMatch<TKey>(Tree idealTree, Participant part, Func<Participant, TKey> keySelector)
        {
            int idealMatchCount = idealTree.Matches.Count;
            var (_, idealRoundsCount) = MathEXT.NextPowerOfTwo(idealTree.Matches.Count * 2);

            bool foundMatch = false;
            var totalSegmentsCount = MathEXT.SumOfThePowersOfTwo(idealRoundsCount);

            Console.WriteLine(idealTree.Matches.Count(m => m.Pair.Any(p => p.PartType == PartType.PlaceHolderPart)));

            for (int j = 0; j < idealRoundsCount && !foundMatch; j++)
            {
                var lastAvailableMatch = idealTree.Matches.LastOrDefault(m => m.Pair.Any(p => p.PartType == PartType.PlaceHolderPart));
                int segmentCountForRound = (int)(Math.Pow(2, j));
                int segmentMatchCountForRound = idealMatchCount / segmentCountForRound;

                for (int m = 0; m < segmentCountForRound && !foundMatch; m++)
                {
                    List<Match> segmentMatches = idealTree.Matches.GetRange(m * segmentMatchCountForRound, segmentMatchCountForRound);
                    List<Participant> segmentParts = MergeMatchesIntoList(segmentMatches);

                    bool isLastSegmentAndRound = (j == idealRoundsCount - 1 && segmentMatches.Contains(lastAvailableMatch));

                    if (!segmentParts.Any(p => EqualityComparer<TKey>.Default.Equals(keySelector(p), keySelector(part))) || isLastSegmentAndRound)
                    {
                        List<Match> placableSegmentMatches = segmentMatches.GetRange(0, segmentMatches.Count);

                        Match match = new Match();
                        if (isLastSegmentAndRound)
                        {
                            match = lastAvailableMatch;
                        }
                        else
                        {
                            match = placableSegmentMatches.Find(_m => _m.Pair.All(p => p.PartType == PartType.PlaceHolderPart));
                            if (match == null)
                                match = placableSegmentMatches.Find(_m => _m.Pair.Any(p => p.PartType == PartType.PlaceHolderPart) && !_m.Pair.Any(p => p.PartType == PartType.NullPart)); // Leaves half matches for last (half match = 1 placeholder & 1 null)
                            if (match == null)
                                match = placableSegmentMatches.Find(_m => _m.Pair.Any(p => p.PartType == PartType.PlaceHolderPart));
                            if (match == null)
                                continue;
                        }

                        int availableSlot = match.AvailableSlot(PartType.PlaceHolderPart);
                        int matchIndex = idealTree.Matches.IndexOf(match);

                        if (match.GetMatchType() == MatchType.HalfMatch && !isLastSegmentAndRound) //if contains null part then it is a half match
                        {
                            int remainder = matchIndex % 2;

                            Match pairedmatch;
                            if (remainder == 1)
                                pairedmatch = idealTree.Matches[matchIndex - 1];
                            else
                            {
                                pairedmatch = idealTree.Matches[matchIndex + 1];
                            }

                            if (pairedmatch.GetMatchType() == MatchType.HalfMatch && pairedmatch.Pair.Any(p => p.Country == part.Country))
                                continue;
                        }

                        foundMatch = true;
                        return idealTree.Matches[matchIndex];
                    }
                }
            }

            
            Console.WriteLine("PH" + idealTree.Matches.Count(m => m.Pair.Any(p => p.PartType == PartType.PlaceHolderPart)));
            Console.WriteLine("NP" + idealTree.Matches.Count(m => m.Pair.Any(p => p.PartType == PartType.NullPart)));
            Console.WriteLine("ALL" + idealTree.Matches.Count());

            idealTree.PrintMatches();

            throw new Exception("FindEligibleMatch method malfunctioned.");
        }

        //Tree Initialization
        public static Tree InitializeTree(int totalMatchesCount, MatchClass mClass, int Round)
        {
            Tree tree = new Tree();
            for (int i = 0; i < totalMatchesCount; i++)
            {
                tree.Matches.Add(new Match
                {
                    Name = $"Match {i + 1}",
                    Pair = new List<Participant>
                    {
                        new Participant
                        {
                            PartType=PartType.NullPart
                        },
                        new Participant
                        {
                            PartType=PartType.NullPart
                        }
                    },
                    MatchClass = mClass,
                    RelativeRound = Round
                });
            }

            return tree;
        }
        public static Tree InitializeAdaptableTree(int totalPartsCount, int totalMatchesCount)
        {
            int t_halfMatches = totalMatchesCount * 2 - totalPartsCount;
            int t_dualMatches = totalMatchesCount - t_halfMatches;

            Tree tree = new Tree();
            for (int i = 0; i < totalMatchesCount; i++)
            {
                tree.Matches.Add(new Match
                {
                    Name = $"Match {i + 1}",
                    Pair = new List<Participant>
                    {
                        new Participant {PartType = PartType.PlaceHolderPart },
                        new Participant {PartType = PartType.PlaceHolderPart },
                    },
                });
            }

            int s1_HalfMatches = (int)MathF.Ceiling(t_halfMatches / 2);
            int s2_HalfMatches = t_halfMatches - s1_HalfMatches;

            if (t_halfMatches > 0)
            {
                for (int i = 0; i < s1_HalfMatches; i++)
                {
                    Match m = tree.Matches[i];

                    m.Pair[1].PartType = PartType.NullPart;

                }
                if (t_halfMatches >= 2)
                {
                    int startindex = totalMatchesCount / 2;

                    for (int i = 0; i < s2_HalfMatches; i++)
                    {
                        Match m = tree.Matches[i + startindex];

                        m.Pair[1].PartType = PartType.NullPart;
                    }
                }
            }

            return tree;
        }
        #endregion

        public static AgeGroup FindRangeOverlap(Range r, List<AgeGroup> ags)
        {
            AgeGroup ag = null;
            bool overlap = false;

            int min = r.min;
            int max = r.max;

            foreach (var a in ags)
            {
                int _min = a.AgeRange.min;
                int _max = a.AgeRange.max;

                if (_min == min && _max == max)
                    continue;

                overlap = ((min <= _min) && (max >= _min)) || ((min <= _max) && (max >= _max));

                if (overlap)
                {
                    ag = a;
                    break;
                }
            }

            return ag;
        }
        public static WeightCategory FindRangeOverlap(Range r, List<WeightCategory> wcs)
        {
            WeightCategory wc = null;
            bool overlap = false;

            int min = r.min;
            int max = r.max;

            foreach (var w in wcs)
            {
                int _min = w.WeightRange.min;
                int _max = w.WeightRange.max;

                if (_min == min && _max == max)
                {
                    continue;
                }

                overlap = ((min < _min) && (max > _min)) || ((min < _max) && (max > _max));

                if (overlap)
                {
                    wc = w;
                    break;
                }
            }

            return wc;
        }
    }

    
}


