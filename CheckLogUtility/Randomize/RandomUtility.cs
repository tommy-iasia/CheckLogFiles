using System;

namespace CheckLogUtility.Randomize
{
    public class RandomUtility
    {
        public static Random Random { get; } = new Random();

        public static int Next(int maxValue) => Random.Next(maxValue);
    }
}
