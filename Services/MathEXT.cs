using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Services
{
    public static class MathEXT
    {
        public static (int Power, int Exp) NextPowerOfTwo(int n)
        {
            int[] arr = new int[2];

            if (n < 1)
            {
                return (Power: 1, Exp: 0);
            }

            if (n == 1)
            {
                return (Power: 2, Exp: 1);
            }

            int exponential = 0;
            int power = 1;
            while (power < n)
            {
                power *= 2;
                exponential++;
            }

            return (power, exponential);
        }

        public static int SumOfThePowersOfTwo(int power)
        {
            int sum = (int)Math.Pow(2, power + 1) - 1; //series simplification

            return sum;
        }
    }
}
