using HyakuninKumite.Services;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using static HyakuninKumiteApplication.Entities.AgeGroup;
using HyakuninKumiteApplication.Models.Enums;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using Path = System.IO.Path;
using Range = HyakuninKumiteApplication.Models.Range;

using HyakuninKumiteApplication.Entities.Category;

namespace HyakuninKumiteApplication.Entities
{
    public class Competition
    {
        [JsonIgnore]
        readonly static CompetitionListService compListService = new CompetitionListService();

        //--> Data Variables
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string StartDate { get; set; }

        // System.Text.Json can automatically use private setters during deserialization
        public List<Participant> Participants { get; private set; } = new List<Participant>();

        public List<AgeGroup> AgeGroups { get; private set; } = new List<AgeGroup>();

        public List<CategoryGroup> CategoryGroups { get; private set; } = new List<CategoryGroup>();

        public string PictureBase64
        {
            get => ImageToBase64(Picture);
            set => Picture = Base64ToImage(value);
        }

        //--> Ignored Variables
        [JsonIgnore]
        public int DataID;
        [JsonIgnore]
        public Image Picture;
        [JsonIgnore]
        public bool MatchmakeStatus { get; private set; } = false;

        #region Category Group List Management Methods
        public void AddCategoryGroup()
        {
           CategoryGroups.Add(new CategoryGroup());

            RenameCategoryGroups(CategoryGroups);

            MatchmakeStatus = false;

            SaveCompetitionData();
        }
        //--><--
        public void DeleteCategoryGroup(CategoryGroup _cg)
        {
            if (_cg != null && CategoryGroups.Contains(_cg))
            {
                CategoryGroups.Remove(_cg);

                RenameCategoryGroups(CategoryGroups);

                MatchmakeStatus = false;

                SaveCompetitionData();
            }
        }

        public void RenameCategoryGroups(List<CategoryGroup> cgs)
        {
            for (int i = 0; i < cgs.Count; i++)
            {
                char letter = (char)('A' + i);

                cgs[i].ID = letter.ToString();
                cgs[i].Name = $"Group {letter}";
            }
        }

        public void GenerateCategoryGroup()
        {
            CategoryGroups.Clear();

            CategoryGroups.Add(new CategoryGroup
            {
                ID = "A",
                Name = "Group A",
            });
            foreach (AgeGroup ag in AgeGroups)
            {
                CategoryGroups[0].AddDivision(ag.MaleDivision);
                CategoryGroups[0].AddDivision(ag.FemaleDivision);
            }

            MatchmakeStatus=false;

            SaveCompetitionData();
        }
        #endregion
        
        #region--> Participant List Manager Methods
        public void AddParticipant(Participant part, bool insert)
        {
            if (!Participants.Contains(part))
            {
                if (insert)
                    Participants.Insert(0, part);
                else
                    Participants.Add(part);

                MatchmakeStatus = false;

                SaveCompetitionData();
            }
        }
        public void AddParticipant(Participant part)
        {
            AddParticipant(part, false);
        }
        public void AddParticipantList(List<Participant> parts)
        {
            Participants.AddRange(parts);

            MatchmakeStatus = false;

            SaveCompetitionData();
        }
        
        public void SetParticipantList(List<Participant> parts)
        {
            Participants.Clear();
            Participants.AddRange(parts);

            SaveCompetitionData();
        }

        public void DeleteParticipant(Participant part)
        {
            if (Participants.Contains(part))
            {
                Participants.Remove(part);

                MatchmakeStatus = false;

                SaveCompetitionData();
            }
        }
        public void ClearParts()
        {
            Participants.Clear();

            MatchmakeStatus = false;

            SaveCompetitionData();
        }
        #endregion

        #region--> Age Groups List Manager Methods
        public void AddAgeGroup(Range range, bool insert)
        {
            AgeGroup ag = new AgeGroup(range);

            if (insert)
                AgeGroups.Insert(0, ag);
            else
                AgeGroups.Add(ag);

            SaveCompetitionData();
        }
        public void AddAgeGroup(Range r)
        {
            AddAgeGroup(r, false);
        }
        public bool DeleteAgeGroup(AgeGroup _ag)
        {
            var ag = AgeGroups.Find(a => a == _ag);
            if (ag != null)
            {
                AgeGroups.Remove(ag);
                SaveCompetitionData();
                return true;
            }

            //SortParticipantListIntoCategories(Participants, AgeGroups);

            return false;
        }

        public void ClearAgeGroups()
        {
            AgeGroups.Clear();
            SaveCompetitionData();
        }
        public void GenerateAgeGroups()
        {
            AgeGroups.Clear();
            AgeGroups.AddRange(new List<AgeGroup>
            {
                new AgeGroup(new Range(0, 7))
                {
                    Name = "Under 8 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    MaleDivision = new Division(Gender.Male, 5, new Range(20, 30)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(20, 30)),
                },
                new AgeGroup(new Range(8, 9))
                {
                    Name = "8-9 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(8, 9),
                    MaleDivision = new Division(Gender.Male, 5, new Range(20, 35)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(20, 35)),
                },
                new AgeGroup(new Range(10, 11))
                {
                    Name = "10-11 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(10, 11),
                    MaleDivision = new Division(Gender.Male, 5, new Range(30, 45)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(30, 45)),
                },
                new AgeGroup(new Range(12, 13))
                {
                    Name = "12-13 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(12, 13),
                    MaleDivision = new Division(Gender.Male, 5, new Range(40, 55)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(30, 50)),
                },
                new AgeGroup(new Range(14, 15))
                {
                    Name = "14-15 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(14, 15),
                    MaleDivision = new Division(Gender.Male, 5, new Range(50, 75)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(40, 60)),
                },
                new AgeGroup(new Range(16, 17))
                {
                    Name = "16-17 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(16, 17),
                    MaleDivision = new Division(Gender.Male, 5, new Range(65, 80)),
                    FemaleDivision = new Division(Gender.Female, 5, new Range(55, 65)),
                },
                new AgeGroup(new Range(18, 99))
                {
                    Name = "+18 Years Old",
                    FirstKata = Kata.PinanSonoIchi,
                    SecondKata = Kata.PinanSonoNi,
                    AgeRange = new Range(18, 50),
                    MaleDivision = new Division(Gender.Male, 10, new Range(60, 90)),
                    FemaleDivision = new Division(Gender.Female, 10, new Range(50, 80)),
                },
            });

            MatchmakeStatus = false;

            RenameAgeGroups();

            SaveCompetitionData();
        }
        public void RenameAgeGroups()
        {
            var ags = AgeGroups;

            ags = ags.OrderBy(a => a.AgeRange.min).ToList();

            ags[0].Name = $"Under {ags[0].AgeRange.max + 1} Years Old";
            ags[^1].Name = $"+{ags[ags.Count - 1].AgeRange.min} Years Old";

            for (int i = 1; i < ags.Count - 1; i++) 
            { 
                var ag = ags[i];

                ag.Name = $"{ag.AgeRange.min}-{ag.AgeRange.max} Years Old";
            }

            foreach (var item in ags)
            {
                item.RenameDivisions();
            }
        }


        #endregion

        #region--> Category Related Methods
        public void Matchmake()
        {
            CategorizeParticipants(Participants, AgeGroups);

            foreach (var cg in CategoryGroups)
            {
                cg.MatchmakeParticipants();
            }

            InitializeKataCategories();

            SaveCompetitionData();
        }

        public void InitializeKataCategories()
        {
            foreach(var ag in AgeGroups)
            {
                foreach(var div in ag.GetDivisions())
                {
                    div._KataCategory.InitializeFirstRound();
                }
            }
        }

        public void CategorizeParticipants(List<Participant> parts, List<AgeGroup> ags)
        {
            foreach (AgeGroup ag in ags)
            {
                foreach(WeightCategory wc in ag.MaleDivision.WeightCategories)
                {
                    wc.Parts.Clear();
                }
                ag.MaleDivision.GenerateKataCategory();
                foreach (WeightCategory wc in ag.FemaleDivision.WeightCategories)
                {
                    wc.Parts.Clear();
                }
                ag.FemaleDivision.GenerateKataCategory();
            }

            foreach (Participant part in parts)
            {
                Division div = null;
                foreach (AgeGroup ag in AgeGroups)
                {
                    //--> Sort Age
                    if (part.Age >= ag.AgeRange.min && part.Age <= ag.AgeRange.max)
                    {
                        //--> Sort Gender
                        if (part.Gender == Gender.Male)
                        {
                            div = ag.MaleDivision;
                        }
                        else if (part.Gender == Gender.Female)
                        {
                            div = ag.FemaleDivision;
                        }

                        //--> Sort Weight
                        if (div != null)
                        {
                            if (part.Kumite)
                            {
                                foreach (WeightCategory _wc in div.WeightCategories)
                                {
                                    if (part.Weight > _wc.WeightRange.min && part.Weight <= _wc.WeightRange.max && !_wc.Parts.Contains(part))
                                    {
                                        _wc.Parts.Add(part);
                                    }
                                }
                            }
                            if (part.Kata)
                            {
                                div._KataCategory.Parts.Add(part);
                            }
                        }
                    }
                }
            } 
        }

        public void EnumerateParticipants()
        {
            List<Match> matches = new List<Match>();
            List<Participant> partList = new List<Participant>();
            int count = 0;
            foreach (AgeGroup ageGroup in AgeGroups)
            {
                foreach (WeightCategory wc in ageGroup.GetWeightCategories())
                {
                    if (wc.Rounds.Count <= 0)
                        continue;

                    foreach (Match m in wc.Rounds[0].Matches)
                    {
                        foreach (Participant part in m.Pair)
                        {
                            if (!part.isBye && part != null && !partList.Contains(part))
                            {
                                count++;
                                part.CompId = count;
                                partList.Add(part);
                            }
                        }
                    }
                }

                foreach(var p in ageGroup.GetParticipants())
                {
                    if (!partList.Contains(p))
                    {
                        count++;
                        p.CompId = count;
                        partList.Add(p);
                    }

                }
            }

            SaveCompetitionData();
        }

        public void EnumerateMatches()
        {
            foreach (var cg in CategoryGroups)
            {
                cg.EnumerateMatches();
            }

            SaveCompetitionData();
        }

        public int WriteAllRoundMatches()
        {
            int count = 0;
            string message = "";
            foreach (AgeGroup ag in AgeGroups)
            {
                foreach (Match match in ag.MaleDivision.WeightCategories.SelectMany(wc => wc.Rounds.SelectMany(r => r.Matches)))
                {
                    if (match == null) continue;
                    message = message + $"\nMatch: {match.Pair[0].Name}   VS   {match.Pair[1].Name}  {match.Pair.Count}";
                }
                foreach (Match match in ag.FemaleDivision.WeightCategories.SelectMany(wc => wc.Rounds.SelectMany(r => r.Matches)))
                {
                    if (match == null) continue;
                    message = message + $"\nMatch: {match.Pair[0].Name}   VS   {match.Pair[1].Name}  {match.Pair.Count}";
                }
            }

            MessageBox.Show(message);

            return count;
        }
        public int WriteAllWCParts(WeightCategory wc)
        {
            int count = 0;
            foreach (Participant part in wc.Rounds.SelectMany(r => r.Matches.SelectMany(m => m.Pair)))
            {
                if (part.Surname != null && part.Surname != "")
                {
                    count++;
                }
            }
            string message = "Rounds Participants in Order Count:" + " " + count;

            return count;
        }

        #endregion

        #region--> Image<=>Base64 Converter Methods
        private static string ImageToBase64(Image image)
        {
            if (image == null) return null;
            var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            return Convert.ToBase64String(ms.ToArray());
        }
        private static Image Base64ToImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return null;
            byte[] imageBytes = Convert.FromBase64String(base64String);
            var ms = new MemoryStream(imageBytes);
            return Image.FromStream(ms);
        }
        #endregion

        #region Children Class Relations
        public List<WeightCategory> GetWeightCategories() => AgeGroups.SelectMany(a => a.GetWeightCategories()).ToList();

        public List<Division> GetDivisions() => AgeGroups.SelectMany(a => a.GetDivisions()).ToList();
        #endregion

        #region--> Database Functions
        public void SaveCompetitionData()
        {
            string exportDirectoryPath = Path.GetDirectoryName(FilePath);
            string exportFilePath = Path.Combine(exportDirectoryPath, Name) + ".hya";

            fs.SaveCompetitionToHYAFile(this, exportFilePath, FilePath);
            FilePath = exportFilePath;

            compListService.EditComp(this);
        }
        #endregion
    }
}
