using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Extensions;
using Object = DalSoft.RestClient.Extensions.Object;

namespace DalSoft.RestClient.Handlers
{
    public class TwitterHandler : DelegatingHandler
    {
        private const string TwitterApiBaseUrl = "https://api.twitter.com/1.1";
        private const string UploadApiBaseUrl = "https://upload.twitter.com/1.1";
        private readonly string _consumerKey, _accessToken;
        private readonly HMACSHA1 _sigHasher;
        private readonly DateTime _epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TwitterHandler(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerKeySecret) || string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(accessTokenSecret))
                throw new ArgumentNullException($"{nameof(consumerKey)}, {nameof(consumerKeySecret)}, {nameof(accessToken)}, {nameof(accessTokenSecret)} cannot be Empty");
            
            _consumerKey = consumerKey;
            _accessToken = accessToken;
            _sigHasher = new HMACSHA1(Encoding.UTF8.GetBytes($"{consumerKeySecret}&{accessTokenSecret}"));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.ToString().StartsWith(TwitterApiBaseUrl) || request.RequestUri.ToString().StartsWith(UploadApiBaseUrl))
            {
                var content = request.GetContent(); // Get passed content to Post()
                
                var multipartFormData = content?.FlattenObjectToKeyValuePairs<object>(includeThisType: typeInfo => true) ?? new List<KeyValuePair<string, object>>();
                var formData = multipartFormData
                    .Where(_ => _.Value.GetType() != typeof(Stream) && _.Value.GetType() != typeof(byte[]))
                    .Select(_ => new KeyValuePair<string, string>(_.Key, _.Value.ToString())).ToList();
                
                request.RequestUri = FormatTwitterUrl(request.RequestUri, multipartFormData);
                
                var urlWithoutQuery = request.RequestUri.GetLeftPart(UriPartial.Path);

                var timestamp = (int)(DateTime.UtcNow - _epochUtc).TotalSeconds;
                
                var oAuthData = new Dictionary<string, string>
                {
                    {"oauth_consumer_key", _consumerKey},
                    {"oauth_signature_method", "HMAC-SHA1"},
                    {"oauth_timestamp", timestamp.ToString()},
                    {"oauth_nonce", Guid.NewGuid().ToString("N") }, // Used for uniqueness (double dectection) not secuirty so a Guid is ok here
                    {"oauth_token", _accessToken},
                    {"oauth_version", "1.0"}
                };

                var dataToSign = new List<KeyValuePair<string, string>>();
                
                dataToSign.AddRange(QueryStringToDictionary(request.RequestUri.Query));     // QueryString
                dataToSign.AddRange(formData);                                              // FormData
                dataToSign.AddRange(oAuthData);                                             // oAuth Headers
                
                var oAuthSignature = GenerateHMACSHA1Signature(request.Method.ToString().ToUpper(), urlWithoutQuery, dataToSign); // Generate the OAuth signature and add it to our payload
                oAuthData.Add(oAuthSignature.Key, oAuthSignature.Value); // Add oauth_signature to oAuthData dictionary
                
                var oAuthHeader = GenerateOAuthHeader(oAuthData); // Build the OAuth HTTP Header from the oAuthData dictionary
                
                request.Headers.Add("Authorization", oAuthHeader);

                if (IsStreamOrBytes(multipartFormData))
                {
                    request.SetContentType("multipart/form-data");
                    request.Content = MultipartFormDataHandler.GetContent(request);
                }
                else if (request.Method.ToString().IsMutableHttpMethod())
                    request.Content = new FormUrlEncodedContent(formData);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false); // Next in the pipeline
        }

        private static bool IsStreamOrBytes(IEnumerable<KeyValuePair<string, object>> multipartFormData)
        {
            return multipartFormData.Any(_ => _.Value.GetType() == typeof(Stream) || _.Value.GetType() == typeof(byte[]));
        }

        private static Uri FormatTwitterUrl(Uri uri, IEnumerable<KeyValuePair<string, object>> multipartFormData)
        {
            var urlWithoutQuery = uri.GetLeftPart(UriPartial.Path).ToLower(); // Deal with case sensitive resources

            if (IsStreamOrBytes(multipartFormData))
                urlWithoutQuery = urlWithoutQuery.Replace(TwitterApiBaseUrl, UploadApiBaseUrl);

            var url = urlWithoutQuery;
            url = url.Replace(@"/json", ".json"); // .json on the resource
            url = urlWithoutQuery.EndsWith(".json") ? url : url.Replace(urlWithoutQuery, urlWithoutQuery + ".json"); //.json on the resource
            url = url + uri.Query.Replace("+", "%20"); // Add Query Strinng and fix QueryCommand Encode
            
            return new Uri(url);
        }

        // ReSharper disable once InconsistentNaming
        private KeyValuePair<string, string> GenerateHMACSHA1Signature(string httpMethod, string url, ICollection<KeyValuePair<string, string>> dataToSign)
        {
            var sigString = string.Join("&", dataToSign
                                                .Union(dataToSign)
                                                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")
                                                .OrderBy(s => s));

            var fullSigData = $"{httpMethod}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(sigString)}";

            var signature = Convert.ToBase64String(_sigHasher.ComputeHash(Encoding.UTF8.GetBytes(fullSigData)));

            var oAuthSignatureHeader = new KeyValuePair<string, string>("oauth_signature", signature);
            
            return oAuthSignatureHeader;
        }

        private static string GenerateOAuthHeader(IEnumerable<KeyValuePair<string, string>> oAuthData)
        {
            return "OAuth " + string.Join(", ", oAuthData
                                                   .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\"")
                                                   .OrderBy(s => s));
        }

        private static IEnumerable<KeyValuePair<string, string>> QueryStringToDictionary(string queryString)
        {
            var matches = Regex.Matches(queryString, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            
            return matches.Cast<Match>().ToDictionary(m => Uri.UnescapeDataString(m.Groups[2].Value), m => Uri.UnescapeDataString(m.Groups[3].Value));
        }
    }
}
