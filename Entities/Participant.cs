using Newtonsoft.Json;
using static HyakuninKumite.Services.MatchmakingService;

using HyakuninKumiteApplication.Models.Enums;

namespace HyakuninKumiteApplication.Entities
{
    public class Participant
    {
        public Participant() { }

        public int CompId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Country { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        [JsonIgnore]
        public int Age => GetAge(DateOfBirth);
        public string BC { get; set; }
        public string DOP { get; set; }
        public float Weight { get; set; }
        public bool Kumite { get; set; }
        public bool Kata { get; set; }
        public PartType PartType { get; set; } = PartType.NormalPart;

        public override string ToString()
        {
            return $"CompId: {CompId}\n" +
                   $"Name: {Name}\n" +
                   $"Surname: {Surname}\n" +
                   $"Country: {Country}\n" +
                   $"Gender: {Gender}\n" +
                   $"DateOfBirth: {DateOfBirth}\n" +
                   $"Age: {Age}\n" +
                   $"BC: {BC}\n" +
                   $"DOP: {DOP}\n" +
                   $"Weight: {Weight} kg\n" +
                   $"Kumite: {(Kumite ? "Yes" : "No")}\n" +
                   $"Kata: {(Kata ? "Yes" : "No")}\n" +
                   $"Part Type: {Enum.GetName(PartType)}"
                   ;
        }

        public string ToShortenedString()
        {
            return $"CompId: {CompId} | Name: {Name} | Surname: {Surname} | Country: {Country} | PartType: {PartType.ToString()}";
        }

        public int GetAge(DateTime date)
        {
            int age = DateTime.Now.Year - date.Year;

            if (DateTime.Now < date.AddYears(age))
            {
                age--;
            }

            return age;
        }
        public int GetAge(string dateOfBirth)
        {
            if (dateOfBirth == null)
                return 0;

            DateTime currentDate = DateTime.Now;

            string[] parts = dateOfBirth.Split('.', '/');

            if (parts.Length != 3)
                throw new FormatException($"Invalid date format for {Name} {Surname}.");

            int d = int.Parse(parts[0]);
            int m = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);

            if (m >= 12)
                m = 12;

            DateTime birthday = new DateTime(y, m, d);

            int age = DateTime.Now.Year - birthday.Year;

            if (DateTime.Now < birthday.AddYears(age))
            {
                age--;
            }

            return age;

        }

        public bool isBye => PartType == PartType.NullPart || PartType == PartType.PlaceHolderPart;
    }
}
