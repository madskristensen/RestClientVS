using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestClient.Client
{
    // Found on: https://www.jordanbrown.dev/2021/02/06/2021/http-to-raw-string-csharp/
    public static class HttpClientExtensions
    {
        public static async Task<string> ToRawStringAsync(this HttpRequestMessage request)
        {
            var sb = new StringBuilder();

            var line1 = $"{request.Method} {request.RequestUri} HTTP/{request.Version}";
            sb.AppendLine(line1);

            foreach (KeyValuePair<string, IEnumerable<string>> instance in request.Headers)
            {
                foreach (var val in instance.Value)
                {
                    var header = $"{instance.Key}: {val}";
                    sb.AppendLine(header);
                }
            }

            if (request.Content?.Headers != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> instance in request.Content.Headers)
                {
                    foreach (var val in instance.Value)
                    {
                        var header = $"{instance.Key}: {val}";
                        sb.AppendLine(header);
                    }
                }
            }
            sb.AppendLine();

            var body = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult<string?>(null));
            if (!string.IsNullOrWhiteSpace(body))
            {
                sb.AppendLine(body);
            }

            return sb.ToString();
        }

        public static async Task<string> ToRawStringAsync(this HttpResponseMessage response)
        {
            var sb = new StringBuilder();

            var statusCode = (int)response.StatusCode;
            var line1 = $"HTTP/{response.Version} {statusCode} {response.ReasonPhrase}";
            sb.AppendLine(line1);

            foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in response.Headers)
            {
                foreach (var val in keyValuePair.Value)
                {
                    var header = $"{keyValuePair.Key}: {val}";
                    sb.AppendLine(header);
                }
            }

            foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in response.Content.Headers)
            {
                foreach (var val in keyValuePair.Value)
                {
                    var header = $"{keyValuePair.Key}: {val}";
                    sb.AppendLine(header);
                }
            }

            sb.AppendLine();

            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                sb.AppendLine(body);
            }

            return sb.ToString();
        }
    }
}
