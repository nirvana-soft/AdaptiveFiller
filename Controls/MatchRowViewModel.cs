using System;
using System.Collections.Generic;
using System.Text;
using HyakuninKumiteApplication.Entities;
using System.Windows.Media;
using HyakuninKumiteApplication.Entities.Category;

namespace HyakuninKumiteApplication.Controls
{
    public class MatchRowViewModel
    {
        public int MatchCompNumber { get; set; }
        public string PreMatchNum { get; set; }
        public string NextMatchNum { get; set; }
        public string DivisionName { get; set; }
        public string Round { get; set; }
        public string ShiroName { get; set; }
        public string AkaName { get; set; }
        public string WinnerName { get; set; }

        public Match OriginalMatch { get; set; }
        public WeightCategory OriginalWC {  get; set; }
        public AgeGroup.Division OriginalDIV {  get; set; }

        public Brush ShiroForeground => (OriginalMatch?.Winner != null && OriginalMatch.Winner == OriginalMatch.Pair[0])
        ? Brushes.DarkGreen
        : Brushes.Black;

        public Brush AkaForeground => (OriginalMatch?.Winner != null && OriginalMatch.Winner == OriginalMatch.Pair[1])
            ? Brushes.DarkGreen
            : Brushes.Black;
    }
}
