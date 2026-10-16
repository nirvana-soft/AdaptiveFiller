using HyakuninKumite.Services;
using HyakuninKumiteApplication.Models.Enums;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using static HyakuninKumite.Services.MatchmakingService;
using static HyakuninKumiteApplication.Entities.AgeGroup.Division;

namespace HyakuninKumiteApplication.Entities
{
    public class Match
    {
        public string Name;
        public int MatchCompNumber = 0;
        public int UniversalRound = 0;
        public int RelativeRound = 0;
        public List<Participant> Pair { get; set; }

        public Participant Winner { get; set; }

        public MatchClass MatchClass = MatchClass.Regular;
        public MatchType GetMatchType()
        {
            MatchType mType = Models.Enums.MatchType.FullMatch;
            if (Pair.All(p => p.PartType == PartType.NullPart))
                mType = MatchType.ByeMatch;
            else if (Pair.Any(p => p.PartType == PartType.NullPart))
                mType = MatchType.HalfMatch;

            return mType;
        }

        public Participant OpponentOf (Participant part) => Pair.Find(p => p != part);

        public Participant GetRealParticipant => GetMatchType() == MatchType.HalfMatch ? OpponentOf(Pair[AvailableSlot(PartType.NullPart)]) : null;

        public int AvailableSlot(PartType pt)
        {
            if (Pair[0].PartType == pt) return 0;
            if (Pair[1].PartType == pt) return 1;

            return -1;
        }

        public void InitializePair()
        {
            Pair = new List<Participant>();
            while(Pair.Count < 2)
            {
                Pair.Add(new Participant { PartType = PartType.NullPart });
            }
        }

        public bool isNull()
        {
            bool b = false;

            if (Pair.All(p => p.PartType == PartType.NullPart))
                b = true;

            return b;
        }

        public bool isPlaceHolder()
        {
            bool b = false;

            if (Pair.Any(p => p.PartType == PartType.PlaceHolderPart))
                b = true;

            return b;
        }
    }

}
