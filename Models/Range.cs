using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Models
{
    public class Range
    {
        public int min { get; set; }
        public int max { get; set; }

        // Constructor to initialize the range with a start and end
        public Range(int _min, int _max)
        {
            if (_min > _max)
            {
                throw new ArgumentException("The minimum cannot be greater than the maximum." +
                    "\nCorrect usage is \"Range(min, max)\"");
            }

            min = _min;
            max = _max;
        }

        public bool Contains(int value)
        {
            return value >= min && value <= max;
        }
    }
}
