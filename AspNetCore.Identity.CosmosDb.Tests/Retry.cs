namespace AspNetCore.Identity.CosmosDb.Tests
{

    //https://stackoverflow.com/questions/1563191/cleanest-way-to-write-retry-logic
    public static class Retry
    {
        /// <summary>
        /// Do action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount"></param>
        public static void Do(
            Action action,
            TimeSpan retryInterval,
            int maxAttemptCount = 5)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        /// <summary>
        /// Do action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public static T Do<T>(
            Func<T> action,
            TimeSpan retryInterval,
            int maxAttemptCount = 5)
        {
            var exceptions = new List<Exception>();

            for (var attempted = 0; attempted < maxAttemptCount; attempted++)
                try
                {
                    if (attempted > 0) Thread.Sleep(retryInterval);
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            throw new AggregateException(exceptions);
        }
    }
}
