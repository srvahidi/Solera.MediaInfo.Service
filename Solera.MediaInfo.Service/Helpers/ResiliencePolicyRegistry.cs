using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Registry;
using Polly.Wrap;
using System.Net.Http;
using Solera.MediaInfo.Service.Helpers;
using Polly;

namespace Solera.MediaInfo.Service.Helpers
{
    public class ResiliencePolicyRegistry : PolicyRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentalConfiguration"></param>
        /// <param name="logger"></param>
        public ResiliencePolicyRegistry(IEnvironmentConfiguration environmentalConfiguration)
        {
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(environmentalConfiguration.ResiliencePolicyMaxWaitTimeInMsecs);
            TimeSpan seedDelay = TimeSpan.FromMilliseconds(environmentalConfiguration.ResiliencePolicyMinWaitTimeInMsecs);
            int maxRetries = environmentalConfiguration.ResiliencePolicyMaxRetryCount;

            // Define WaitAndRetry with DecorrelatedJitter policy
            Policy retryWithDecorrelatedJitterPolicyAsync = Policy
               .Handle<Exception>() //HttpRequestException, 5XX (server errors) and 408 (request timeout)
               .WaitAndRetryAsync(Helper.DecorrelatedJitter(maxRetries, seedDelay, maxDelay),
               onRetry: (e, t, i, context) => // Capture info for logging
               {
                   string msg = $"WaitAndRetryResiliencePolicy:  retry <{i}> due to exception <{e.Message}> with delay <{t.TotalMilliseconds}> milliseconds.";
                   Logging.LogInformation(msg);
               });

            PolicyWrap resiliencePolicyWrapAsync = Policy.WrapAsync(retryWithDecorrelatedJitterPolicyAsync, retryWithDecorrelatedJitterPolicyAsync);
            Add("mbePolicy", resiliencePolicyWrapAsync);
        }

    }
}
