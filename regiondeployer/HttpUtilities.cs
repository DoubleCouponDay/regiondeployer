using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace regiondeployer
{
    public class HttpUtilities
    {
        private readonly string basicAuthSeparator = ":";
        private readonly string basicAuthKey = "Authorization";
        private readonly Uri dummyUrl = new Uri("http://localhost");

        public StringContent GetJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public HttpRequestMessage CreateRequestMessage(string url, HttpMethod method, HttpContent content, KeyValuePair<string, string>[] headers)
        {
            var request = new HttpRequestMessage(method, new Uri(url))
            {
                Content = content
            };
            foreach (var kvp in headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
            return request;
        }

        public HttpRequestMessage Clone(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri)
            {
                Content = req.Content,
                Version = req.Version
            };
            foreach (var option in req.Options.AsEnumerable())
            {
                clone.Options.Append(new KeyValuePair<string, object>(option.Key, option.Value));
            }

            var oldHeaders = req.Headers.ToList();
            foreach (var current in oldHeaders)
            {
                clone.Headers.TryAddWithoutValidation(current.Key, current.Value);
            }

            return clone;
        }

        public KeyValuePair<string, string> CreateBasicAuthHeader(string username, string password)
        {
            var input = username + basicAuthSeparator + password;
            var bytes = Encoding.UTF8.GetBytes(input);
            var output = Convert.ToBase64String(bytes);
            return new KeyValuePair<string, string>(basicAuthKey, "Basic " + output);
        }
    }
}
