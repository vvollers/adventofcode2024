using System.Collections.Generic;
using System.Linq;

namespace adventofcode.AdventLib
{
    public static class ExtraMath
    {
        /// <summary>
        /// Finds the least common multiple (LCM) of a collection of numbers.
        /// </summary>
        /// <param name="numbers">The collection of numbers for which to find the LCM.</param>
        /// <returns>The LCM of the provided numbers.</returns>
        public static long FindLeastCommonMultiple(IEnumerable<long> numbers) =>
            numbers.Aggregate((long)1, (current, number) => current / GreatestCommonDivisor(current, number) * number);

        /// <summary>
        /// Finds the greatest common divisor (GCD) of two numbers.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>The GCD of the two provided numbers.</returns>
        public static long GreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                a %= b;
                (a, b) = (b, a);
            }
            return a;
        }
    }
}