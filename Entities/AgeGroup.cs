
using DocumentFormat.OpenXml.Drawing;
using HyakuninKumiteApplication.Models.Enums;
using HyakuninKumiteApplication.Services;
using System.Text.Json.Serialization;
using System.Windows.Controls.Primitives;
using Range = HyakuninKumiteApplication.Models.Range;

using HyakuninKumiteApplication.Entities.Category;

namespace HyakuninKumiteApplication.Entities
{
    public class AgeGroup
    {
        public string Name { get; set; }
        public string FirstKata { get; set; } = "";
        public string SecondKata { get; set; } = "";
        public Range AgeRange { get; set; } = new Range(0, 1);
        public Division MaleDivision { get; set; } = new Division(Gender.Male, 5, new Range(0,80));
        public Division FemaleDivision { get; set; } = new Division(Gender.Female, 5, new Range(0, 80));

        public AgeGroup(Range agerange)
        {
            AgeRange = agerange;

            RenameDivisions();
        }

        //--> Methods

        public void AutonameWeightCategories()
        {
            foreach (var division in GetDivisions())
            {
                division.AutonameWeightCategories();
            }
        }

        public void RenameDivisions()
        {
            MaleDivision.AgeGroupName = Name;
            FemaleDivision.AgeGroupName = Name;
        }

        #region Child Class Relations
        public List<Participant> GetParticipants() =>
            GetWeightCategories()
                .SelectMany(w => w.Parts)
                .Concat(GetDivisions().SelectMany(d => d._KataCategory.Parts))
                .ToList();
        public List<WeightCategory> GetWeightCategories() => MaleDivision.WeightCategories.Concat(FemaleDivision.WeightCategories).ToList();
        public List<Division> GetDivisions() => new List<Division> { MaleDivision, FemaleDivision };
        #endregion

        //Nested Class
        public class Division
        {
            public string CategoryGroupName { get; set; } = "";
            public string AgeGroupName { get; set; } = "";
            public string Name => $"{AgeGroupName} {Gender.ToString()}";
            public Gender Gender { get; set; }
            public int WeightIntervals { get; set; }
            public Range WeightRange { get; set; }

            public List<WeightCategory> WeightCategories { get; private set; } = new List<WeightCategory>();

            public KataCategory _KataCategory { get; set; } = new KataCategory();

            public Division(Gender gender, int wi, Range wr)
            {
                Gender = gender;
                WeightIntervals = wi;
                WeightRange = wr;

                GenerateWeightCategories();
            }

            #region--> Weight Category List Manager Methods
            //--><--
            public void GenerateWeightCategories()
            {
                if (WeightIntervals == 0)
                    return;

                WeightCategories.Clear();
                int weightRange = WeightRange.max - WeightRange.min;
                int wCategoryCount = (weightRange / WeightIntervals) + 1;

                for (int i = 0; i <= wCategoryCount; i++)
                {
                    int _min = WeightRange.min + WeightIntervals * (i - 1);
                    int _max = WeightRange.min + WeightIntervals * (i);
                    if (i == 0)
                        _min = 0;
                    else if (i == wCategoryCount)
                        _max = int.MaxValue;

                    WeightCategory wc = new WeightCategory(_min, _max);
                    wc.Name = _max != int.MaxValue ? $"{_min}-{_max}kg" : $"+{_min}";

                    WeightCategories.Add(wc);
                }

                AutonameWeightCategories();
            }
            public void AutonameWeightCategories()
            {
                var wcs = WeightCategories;

                wcs = wcs.OrderBy(a => a.WeightRange.min).ToList();

                foreach (WeightCategory wc in wcs)
                {
                    int _max = wc.WeightRange.max;
                    int _min = wc.WeightRange.min;

                    wc.Name = _max != int.MaxValue ? $"{_min}-{_max}kg" : $"+{_min}";

                    if (wc.Name == "+0")
                        wc.Name = "Open Weight";
                }
            }
            public void RemoveWeightCategory(WeightCategory wc)
            {
                if (WeightCategories.Contains(wc))
                {
                    WeightCategories.Remove(wc);
                }
            }

            public void GenerateKataCategory()
            {
                _KataCategory = new KataCategory();
                _KataCategory.Name = $"{Gender.ToString()} Kata";
            }

            public void MatchmakeDivisionParticipants()
            {
                foreach (WeightCategory wc in WeightCategories)
                {
                    wc.Matchmake();
                }
            }
            
            #endregion

            #region Child Class Relations
            public List<Participant> GetParticipants() => WeightCategories.SelectMany(w => w.Parts).ToList();

            public List<Match> AllMatches => WeightCategories.SelectMany(w => w.AllMatches).ToList();
            #endregion
        }
    }
}
