﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinqToTwitter
{
    public partial class TwitterContext
    {
        public const ulong MissingID = 0ul;
        public const ulong NoReply = 0ul;
        public const decimal NoCoordinate = Decimal.MaxValue;

        /// <summary>
        /// sends a status update with attached media
        /// </summary>
        /// <param name="status">Status text</param>
        /// <param name="possiblySensitive">Set to true if media does not contain age appropriate content</param>
        /// <param name="image">Media to send</param>
        /// <returns>Status containing new tweet</returns>
        [Obsolete("Twitter has deprecated this API and you should use UploadMediaAsync/TweetAsync instead.")]
        public async Task<Status> TweetWithMediaAsync(string status, bool possiblySensitive, byte[] image)
        {
            return await ReplyWithMediaAsync(NoReply, status, possiblySensitive, NoCoordinate, NoCoordinate, null, false, image).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with attached media
        /// </summary>
        /// <param name="status">Status text</param>
        /// <param name="possiblySensitive">Set to true if media does not contain age appropriate content</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query)</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet</param>
        /// <param name="image">Media to send</param>
        /// <returns>Status containing new reply</returns>
        [Obsolete("Twitter has deprecated this API and you should use UploadMediaAsync/TweetAsync instead.")]
        public async Task<Status> TweetWithMediaAsync(string status, bool possiblySensitive, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, byte[] image)
        {
            return await ReplyWithMediaAsync(NoReply, status, possiblySensitive, latitude, longitude, placeID, displayCoordinates, image).ConfigureAwait(false);
        }

        /// <summary>
        /// sends a status update with attached media
        /// </summary>
        /// <param name="inReplyToStatusID">id of status replying to - optional - pass null if not used</param>
        /// <param name="status">Status text</param>
        /// <param name="possiblySensitive">Set to true if media does not contain age appropriate content</param>
        /// <param name="image">Media to send</param>
        /// <returns>Status containing new reply</returns>
        [Obsolete("Twitter has deprecated this API and you should use UploadMediaAsync/ReplyAsync instead.")]
        public async Task<Status> ReplyWithMediaAsync(ulong inReplyToStatusID, string status, bool possiblySensitive, byte[] image)
        {
            return await ReplyWithMediaAsync(inReplyToStatusID, status, possiblySensitive, NoCoordinate, NoCoordinate, null, false, image).ConfigureAwait(false);
        }

        /// <summary>
        /// sends a status update with attached media
        /// </summary>
        /// <param name="inReplyToStatusID">id of status replying to - optional - pass null if not used</param>
        /// <param name="status">Status text</param>
        /// <param name="possiblySensitive">Set to true if media does not contain age appropriate content</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query)</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet</param>
        /// <param name="image">Media to send</param>
        /// <param name="cancelToken">Allows you to cancel async operation</param>
        /// <returns>Status containing new reply</returns>
        [Obsolete("Twitter has deprecated this API and you should use UploadMediaAsync/ReplyAsync instead.")]
        public async Task<Status> ReplyWithMediaAsync(ulong inReplyToStatusID, string status, bool possiblySensitive, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, byte[] image, CancellationToken cancelToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException("status", "status is a required parameter.");

            if (image == null || image.Length == 0)
                throw new ArgumentNullException("image", "You must provide a byte[] of image data.");

            string updateUrl = BaseUrl + "statuses/update_with_media.json";
            string imageType = "application/octet-stream";
            string name = "media[]";
            string randomUnusedFileName = new Random().Next(100, 999).ToString();

            var reqProc = new StatusRequestProcessor<Status>();

            RawResult =
                await TwitterExecutor.PostMediaAsync(
                    updateUrl,
                    new Dictionary<string, string>
                    {
                        {"status", status},
                        {"possibly_sensitive", possiblySensitive ? true.ToString() : null },
                        {"lat", latitude == NoCoordinate ? null : latitude.ToString(Culture.US) },
                        {"long", longitude == NoCoordinate ? null : longitude.ToString(Culture.US) },
                        {"place_id", string.IsNullOrWhiteSpace(placeID) ? null : placeID },
                        {"display_coordinates", displayCoordinates ? true.ToString() : null },
                        {"in_reply_to_status_id", inReplyToStatusID == NoReply ? null : inReplyToStatusID.ToString(CultureInfo.InvariantCulture)}
                    },
                    image, 
                    name,
                    randomUnusedFileName,
                    imageType,
                    cancelToken)
                    .ConfigureAwait(false);

            return reqProc.ProcessActionResult(RawResult, StatusAction.SingleStatus);
        }

        /// <summary>
        /// Uploads a media (e.g. media) to be attached to a subsequent tweet.
        /// </summary>
        /// <param name="media">Media to upload</param>
        /// <param name="cancelToken">Allows you to cancel async operation</param>
        /// <returns>Status containing new reply</returns>
        public async Task<Media> UploadMediaAsync(byte[] media, CancellationToken cancelToken = default(CancellationToken))
        {
            if (media == null || media.Length == 0)
                throw new ArgumentNullException("image", "You must provide a byte[] of image data.");

            string updateUrl = UploadUrl + "media/upload.json";
            string imageType = "application/octet-stream";
            string name = "media";
            string randomUnusedFileName = new Random().Next(100, 999).ToString();

            var reqProc = new StatusRequestProcessor<Status>();

            RawResult =
                await TwitterExecutor.PostMediaAsync(
                    updateUrl,
                    new Dictionary<string, string>(),
                    media,
                    name,
                    randomUnusedFileName,
                    imageType,
                    cancelToken)
                    .ConfigureAwait(false);

            Status status = reqProc.ProcessActionResult(RawResult, StatusAction.MediaUpload);
            return status.Media;
        }

        /// <summary>
        /// Replies to a tweet.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to.</param>
        /// <param name="status">Reply status text.</param>
        /// <returns>Reply status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status)
        {
            return await ReplyAsync(tweetID, status, NoCoordinate, NoCoordinate, null, false, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with coordinates.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to.</param>
        /// <param name="status">Reply status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Reply status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, decimal latitude, decimal longitude, bool displayCoordinates)
        {
            return await ReplyAsync(tweetID, status, latitude, longitude, null, displayCoordinates, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with coordinates, place, and option to not include the user entity.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to.</param>
        /// <param name="status">Reply status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <returns>Reply status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, decimal latitude, decimal longitude, string placeID, bool trimUser)
        {
            return await ReplyAsync(tweetID, status, latitude, longitude, placeID, false, trimUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with place, option to display coordinates, and option to not include user entity.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to.</param>
        /// <param name="status">Reply status text.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Reply status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, string placeID, bool displayCoordinates, bool trimUser)
        {
            return await ReplyAsync(tweetID, status, NoCoordinate, NoCoordinate, placeID, displayCoordinates, trimUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with all non-media options.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to.</param>
        /// <param name="status">Reply status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Reply status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, bool trimUser)
        {
            if (tweetID == MissingID)
                throw new ArgumentException("0 is *not* a valid tweetID. You must provide the ID of the tweet you're replying to.", "tweetID");

            return await TweetOrReplyAsync(tweetID, status, latitude, longitude, placeID, displayCoordinates, trimUser, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with attached media.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="status">Status text.</param>
        /// <param name="mediaIds">Collection of ids of media to include in tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, IEnumerable<ulong> mediaIds)
        {
            if (tweetID == MissingID)
                throw new ArgumentException("0 is *not* a valid tweetID. You must provide the ID of the tweet you're replying to.", "tweetID");

            return await TweetOrReplyAsync(tweetID, status, NoCoordinate, NoCoordinate, NoInputParam, false, false, mediaIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Replies to a tweet with all options.
        /// </summary>
        /// <remarks>
        /// You must include the recipient's screen name (as @ScreenName) for the reply to work.
        /// </remarks>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <param name="trimUser">Remove user entity from response</param>
        /// <param name="mediaIds">Collection of ids of media to include in tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> ReplyAsync(ulong tweetID, string status, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, bool trimUser, IEnumerable<ulong> mediaIds)
        {
            if (tweetID == MissingID)
                throw new ArgumentException("0 is *not* a valid tweetID. You must provide the ID of the tweet you're replying to.", "tweetID");

            return await TweetOrReplyAsync(tweetID, status, latitude, longitude, placeID, displayCoordinates, trimUser, mediaIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status)
        {
            return await TweetOrReplyAsync(NoReply, status, NoCoordinate, NoCoordinate, null, false, false, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with coordinates.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, decimal latitude, decimal longitude)
        {
            return await TweetOrReplyAsync(NoReply, status, latitude, longitude, null, false, false, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with coordinates and option to display.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, decimal latitude, decimal longitude, bool displayCoordinates)
        {
            return await TweetOrReplyAsync(NoReply, status, latitude, longitude, null, displayCoordinates, false, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with coordinates, placeID, and option to not include user entity.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, decimal latitude, decimal longitude, string placeID, bool trimUser)
        {
            return await TweetOrReplyAsync(NoReply, status, latitude, longitude, placeID, false, trimUser, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with all location options and option to not include user entity.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, bool trimUser)
        {
            return await TweetOrReplyAsync(NoReply, status, latitude, longitude, placeID, displayCoordinates, trimUser, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with place, option to display coordinates, and option to not include user entity.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, string placeID, bool displayCoordinates, bool trimUser)
        {
            return await TweetOrReplyAsync(NoReply, status, NoCoordinate, NoCoordinate, placeID, displayCoordinates, trimUser, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with attached media.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="mediaIds">Collection of ids of media to include in tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, IEnumerable<ulong> mediaIds)
        {
            return await TweetOrReplyAsync(NoReply, status, NoCoordinate, NoCoordinate, NoInputParam, false, false, mediaIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update with all options.
        /// </summary>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <param name="trimUser">Remove user entity from response</param>
        /// <param name="mediaIds">Collection of ids of media to include in tweet.</param>
        /// <returns>Tweeted status.</returns>
        public async Task<Status> TweetAsync(string status, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, bool trimUser, IEnumerable<ulong> mediaIds)
        {
            return await TweetOrReplyAsync(NoReply, status, latitude, longitude, placeID, displayCoordinates, trimUser, mediaIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a status update.
        /// </summary>
        /// <param name="tweetID">ID (aka StatusID) of tweet to reply to. Sent via ReplyAsync overloads.</param>
        /// <param name="status">Status text.</param>
        /// <param name="latitude">Latitude coordinate of where tweet occurred.</param>
        /// <param name="longitude">Longitude coordinate of where tweet occurred.</param>
        /// <param name="placeID">ID of place (found via Geo Reverse lookup query).</param>
        /// <param name="displayCoordinates">Allow or prevent display of coordinates for this tweet.</param>
        /// <param name="trimUser">Remove user entity from response</param>
        /// <param name="mediaIds">Collection of ids of media to include in tweet.</param>
        /// <returns>Tweeted status.</returns>
        internal async Task<Status> TweetOrReplyAsync(ulong tweetID, string status, decimal latitude, decimal longitude, string placeID, bool displayCoordinates, bool trimUser, IEnumerable<ulong> mediaIds, CancellationToken cancelToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(status) && (mediaIds == null || !mediaIds.Any()))
                throw new ArgumentException("status is a required parameter.", "status");

            var updateUrl = BaseUrl + "statuses/update.json";

            RawResult =
                await TwitterExecutor.PostToTwitterAsync<Status>(
                    updateUrl,
                    new Dictionary<string, string>
                    {
                        {"status", status},
                        {"in_reply_to_status_id", tweetID == NoReply ? null : tweetID.ToString()},
                        {"lat", latitude == NoCoordinate ? null : latitude.ToString(Culture.US)},
                        {"long", longitude == NoCoordinate ? null : longitude.ToString(Culture.US)},
                        {"place_id", placeID},
                        {"display_coordinates", displayCoordinates ? displayCoordinates.ToString().ToLower() : null},
                        {"trim_user", trimUser ? trimUser.ToString().ToLower() : null },
                        {"media_ids", mediaIds == null || !mediaIds.Any() ? null : string.Join(",", mediaIds) }
                    },
                    cancelToken)
                    .ConfigureAwait(false);

            return new StatusRequestProcessor<Status>()
                .ProcessActionResult(RawResult, StatusAction.SingleStatus);
        }

        /// <summary>
        /// Deletes a tweet.
        /// </summary>
        /// <param name="tweetID">ID of tweet to delete.</param>
        /// <returns>Deleted tweet.</returns>
        public async Task<Status> DeleteTweetAsync(ulong tweetID, CancellationToken cancelToken = default(CancellationToken))
        {
            if (tweetID == MissingID)
                throw new ArgumentException("0 is *not* a valid tweetID. You must provide the ID of the tweet you're deleting.", "tweetID");

            var destroyUrl = BaseUrl + "statuses/destroy/" + tweetID + ".json";

            RawResult = await TwitterExecutor
                .PostToTwitterAsync<Status>(destroyUrl, new Dictionary<string, string>(), cancelToken)
                .ConfigureAwait(false);

            return new StatusRequestProcessor<Status>()
                .ProcessActionResult(RawResult, StatusAction.SingleStatus);
        }

        /// <summary>
        /// Retweets a tweet.
        /// </summary>
        /// <param name="tweetID">ID of tweet being retweeted.</param>
        /// <returns>Retweeted tweet.</returns>
        public async Task<Status> RetweetAsync(ulong tweetID, CancellationToken cancelToken = default(CancellationToken))
        {
            if (tweetID == MissingID)
                throw new ArgumentException("0 is *not* a valid tweetID. You must provide the ID of the tweet you're retweeting.", "tweetID");

            var retweetUrl = BaseUrl + "statuses/retweet/" + tweetID + ".json";

            RawResult = await TwitterExecutor
                .PostToTwitterAsync<Status>(retweetUrl, new Dictionary<string, string>(), cancelToken)
                .ConfigureAwait(false);

            return new StatusRequestProcessor<Status>()
                .ProcessActionResult(RawResult, StatusAction.SingleStatus);
        }
    }
}
