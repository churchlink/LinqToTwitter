﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinqToTwitter
{
    public partial class TwitterContext
    {
        /// <summary>
        /// Lets logged-in user report spam.
        /// </summary>
        /// <param name="userID">User id of alleged spammer.</param>
        /// <returns>Alleged spammer user info.</returns>
        public async Task<User> ReportSpamAsync(ulong userID, CancellationToken cancelToken = default(CancellationToken))
        {
            if (userID == 0)
                throw new ArgumentException("Twitter doesn't have a user with ID == 0", "userID");

            var reportParams = new Dictionary<string, string>
            {
                { "user_id", userID.ToString() }
            };

            return await ReportSpamAsync(reportParams, cancelToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Lets logged-in user report spam.
        /// </summary>
        /// <param name="screenName">Screen name of alleged spammer.</param>
        /// <returns>Alleged spammer user info.</returns>
        public async Task<User> ReportSpamAsync(string screenName, CancellationToken cancelToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(screenName))
                throw new ArgumentException("Please supply a valid screen name", "screenName");

            var reportParams = new Dictionary<string, string>
            {
                { "screen_name", screenName }
            };

            return await ReportSpamAsync(reportParams, cancelToken).ConfigureAwait(false);
        }

        internal async Task<User> ReportSpamAsync(IDictionary<string, string> reportParams, CancellationToken cancelToken = default(CancellationToken))
        {
            string reportSpamUrl = BaseUrl + "users/report_spam.json";

            RawResult =
                await TwitterExecutor
                    .PostToTwitterAsync<User>(reportSpamUrl, reportParams, cancelToken)
                    .ConfigureAwait(false);

            return new UserRequestProcessor<User>()
                .ProcessActionResult(RawResult, StatusAction.SingleStatus);
        }
    }
}
