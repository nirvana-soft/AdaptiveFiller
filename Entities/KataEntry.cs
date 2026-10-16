using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Entities
{
    public class KataEntry
    {
        public Participant Part { get; set;  }
        public List<float> Scores { get; set; } = [0, 0, 0, 0, 0];
        public float TotalScore 
        {
            get
            {
                if (Scores.Count != 5)
                    return 0;

                var mean = Scores.OrderBy(x => x).ToList();
                mean.RemoveAt(0);
                mean.RemoveAt(mean.Count - 1);

                float sum = 0;
                foreach(var m in mean)
                {
                    if(m != null)
                        sum += m;
                }

                return sum;
            }
        }
    
        public void SetScores(List<float> scores)
        {
            if (scores.Count != 5)
                throw new Exception("scores out of range");
            else
            {
                Scores.Clear();
                Scores.AddRange(scores);
            }
        }

        public void SetScore(int index, float score)
        {
            if(Scores.Count > index)
                Scores[index] = score;
        }

        public KataEntry(Participant p)
        {
            Part = p;
        }
    }
}
