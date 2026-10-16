using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Entities.Category
{
    public class Category
    {
        public string Name { get; set; }

        public List<Participant> Parts { get; set; } = new List<Participant>();
        public List<Participant> Result { get; set; }
    }
}
