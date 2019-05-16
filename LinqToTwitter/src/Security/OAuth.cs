﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LinqToTwitter.Net;

namespace LinqToTwitter.Security
{
    public class OAuth : IOAuth
    {
        public string GetAuthorizationString(string method, string url, IDictionary<string, string> parameters, string consumerSecret, string oAuthTokenSecret)
        {
            string encodedAndSortedString = BuildEncodedSortedString(parameters);
            string signatureBaseString = BuildSignatureBaseString(method, url, encodedAndSortedString);
            string signingKey = BuildSigningKey(consumerSecret, oAuthTokenSecret);
            string signature = CalculateSignature(signingKey, signatureBaseString);
            string authorizationHeader = BuildAuthorizationHeaderString(encodedAndSortedString, signature);

            return authorizationHeader;
        }
  
        const string OAuthVersion = "1.0";
        const string OAuthSignatureMethod = "HMAC-SHA1";
        internal void AddMissingOAuthParameters(IDictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("oauth_timestamp"))
                parameters.Add("oauth_timestamp", GetTimestamp());

            if (!parameters.ContainsKey("oauth_nonce"))
                parameters.Add("oauth_nonce", GenerateNonce());

            if (!parameters.ContainsKey("oauth_version"))
                parameters.Add("oauth_version", OAuthVersion);

            if (!parameters.ContainsKey("oauth_signature_method"))
                parameters.Add("oauth_signature_method", OAuthSignatureMethod);     
        }

        internal string BuildEncodedSortedString(IDictionary<string, string> parameters)
        {
            AddMissingOAuthParameters(parameters);

            return
                string.Join("&",
                    (from parm in parameters
                     orderby parm.Key
                     select parm.Key + "=" + Url.PercentEncode(parameters[parm.Key]))
                    .ToArray());
        }

        internal string BuildSignatureBaseString(string method, string url, string encodedStringParameters)
        {
            int paramsIndex = url.IndexOf('?');

            string urlWithoutParams = paramsIndex >= 0 ? url.Substring(0, paramsIndex) : url;

            return string.Join("&", new string[]
            {
                method.ToUpper(),
                Url.PercentEncode(urlWithoutParams),
                Url.PercentEncode(encodedStringParameters)
            });
        }

        internal string BuildSigningKey(string consumerSecret, string oAuthTokenSecret)
        {
            return string.Format(
                CultureInfo.InvariantCulture, "{0}&{1}", 
                Url.PercentEncode(consumerSecret),
                Url.PercentEncode(oAuthTokenSecret));
        }

        internal string CalculateSignature(string signingKey, string signatureBaseString)
        {
            byte[] key = Encoding.UTF8.GetBytes(signingKey);
            byte[] msg = Encoding.UTF8.GetBytes(signatureBaseString);

            byte[] hash = new Hmac(new Sha1()).Sign(key, msg);

            return Convert.ToBase64String(hash);
        }

        internal string BuildAuthorizationHeaderString(string encodedAndSortedString, string signature)
        {
            string[] allParms = (encodedAndSortedString + "&oauth_signature=" + Url.PercentEncode(signature)).Split('&');
            string allParmsString =
                string.Join(", ",
                    (from parm in allParms
                     let keyVal = parm.Split('=')
                     where parm.StartsWith("oauth") || parm.StartsWith("x_auth")
                     orderby keyVal[0]
                     select keyVal[0] + "=\"" + keyVal[1] + "\"")
                    .ToList());
            return "OAuth " + allParmsString;
        }

        const long UnixEpocTicks = 621355968000000000L;
        internal string GetTimestamp()
        {
            long ticksSinceUnixEpoc = DateTime.UtcNow.Ticks - UnixEpocTicks;
            double secondsSinceUnixEpoc = new TimeSpan(ticksSinceUnixEpoc).TotalSeconds;
            return Math.Floor(secondsSinceUnixEpoc).ToString(CultureInfo.InvariantCulture);
        }

        internal virtual string GenerateNonce()
        {
            return new Random().Next(111111, 9999999).ToString(CultureInfo.InvariantCulture);
        }
    }
}
