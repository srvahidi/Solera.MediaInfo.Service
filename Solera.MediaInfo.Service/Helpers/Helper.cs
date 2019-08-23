using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solera.MediaInfo.Service.Helpers
{
    public static class Helper
    {
        public static IEnumerable<TimeSpan> DecorrelatedJitter(int maxRetries, TimeSpan seedDelay, TimeSpan maxDelay)
        {
            Random jitterer = new Random();
            int retries = 0;

            double seed = seedDelay.TotalMilliseconds;
            double max = maxDelay.TotalMilliseconds;
            double current = seed;

            while (++retries <= maxRetries)
            {
                // Adopting the 'Decorrelated Jitter' formula from https://www.awsarchitectureblog.com/2015/03/backoff.html.
                // Can be between seed and previous * 3.  Mustn't exceed max.
                current = Math.Min(max, Math.Max(seed, current * 3 * jitterer.NextDouble()));
                yield return TimeSpan.FromMilliseconds(current);
            }
        }
    }
}