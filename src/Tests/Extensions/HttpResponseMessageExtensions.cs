namespace Tests.Extensions
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> ToTestString(this HttpResponseMessage response)
        {
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine($"Response Code: {(int)response.StatusCode} {response.StatusCode}");
            sb.AppendLine();
            sb.AppendLine("Headers:");

            foreach (var header in response.Headers)
            {
                sb.AppendLine($"    {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    sb.AppendLine($"    {header.Key}: {string.Join(", ", header.Value)}");
                }

                sb.AppendLine();
                sb.AppendLine("Content:");

                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    sb.AppendLine(
                        JsonConvert.SerializeObject(
                            JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()),
                            Formatting.Indented));
                }
                else
                {
                    sb.AppendLine(await response.Content.ReadAsStringAsync());
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}