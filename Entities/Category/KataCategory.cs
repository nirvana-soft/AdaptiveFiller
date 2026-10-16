using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Entities.Category
{
    public class KataCategory : Category
    {
        public List<KataEntry> FirstRound { get; set; }

        public List<KataEntry> SecondRound { get; set; }

        public KataCategory()
        {
            InitializeFirstRound();
        }

        public void InitializeFirstRound()
        {
            if(FirstRound == null)
                FirstRound = new List<KataEntry>();
            else
                FirstRound.Clear();

            foreach(var p in Parts)
            {
                var ke = new KataEntry(p);
                FirstRound.Add(ke);
            }
        }

        public void AutoQualify()
        {
            SecondRound = FirstRound.Where(x => x.TotalScore != 0).OrderByDescending(x => x.TotalScore).Take(6).ToList();
        }

        public void InitializeResults()
        {
            Result = new List<Participant>();

            Result = SecondRound.Where(x => x.TotalScore != 0).OrderByDescending(x => x.TotalScore).ToList()[0..^0].Select(y => y.Part).ToList();
        }
    }
}
