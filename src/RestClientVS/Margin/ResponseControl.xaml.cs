using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using RestClient.Client;

namespace RestClientVS.Margin
{
    public partial class ResponseControl : UserControl
    {
        public ResponseControl()
        {
            InitializeComponent();
        }

        public async Task SetResponseTextAsync(RequestResult result)
        {
            if (result.Response != null)
            {
                var text = new StringBuilder();
                text.AppendLine(result.Response.Headers.ToString());
                text.AppendLine(result.Response.Content.Headers.ToString());
                var mediaType = result.Response.Content.Headers?.ContentType?.MediaType;
                if (mediaType == null)
                {
	                var rawString = await result.Response.Content.ReadAsStringAsync();
	                text.AppendLine("------------------------------------------------");
	                text.AppendLine($"StatusCode: {result.Response.StatusCode} ({(int)result.Response.StatusCode})");
	                if (!string.IsNullOrEmpty(rawString))
	                {
		                text.AppendLine("Content:");
		                text.AppendLine(rawString);
                    }

                    Control.Text = text.ToString();
                    return;
                }
                if (mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var jsonString = await result.Response.Content.ReadAsStringAsync();
                    try
                    {
                        var token = JToken.Parse(jsonString);
                        text.AppendLine("------------------------------------------------");
                        text.AppendLine($"StatusCode: {result.Response.StatusCode} ({(int)result.Response.StatusCode})");
                        text.AppendLine("Content:");
                        text.AppendLine(token.ToString());
                    }
                    catch (Exception ex)
                    {
                        text.AppendFormat("** {0} : Error parsing JSON ( {1} ), raw content follows. **\r\r", nameof(RestClientVS), ex.GetBaseException().Message);
                        text.AppendLine(jsonString);
                    }
                    Control.Text = text.ToString();
                    return;
                }
                if (mediaType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var xmlString = await result.Response.Content.ReadAsStringAsync();
                    try
                    {
                        var xmlElement = XElement.Parse(xmlString);
                        text.AppendLine("------------------------------------------------");
                        text.AppendLine($"StatusCode: {result.Response.StatusCode} ({(int)result.Response.StatusCode})");
                        text.AppendLine("Content:");
                        text.AppendLine(xmlElement.ToString());
                    }
                    catch (Exception ex)
                    {
                        text.AppendFormat("** {0} : Error parsing XML ( {1} ), raw content follows. **\r\r", nameof(RestClientVS), ex.GetBaseException().Message);
                        text.AppendLine(xmlString);
                    }
                    Control.Text = text.ToString();
                    return;
                }
                Control.Text = await result.Response.ToRawStringAsync();
            }
            else
            {
                Control.Text = result.ErrorMessage;
            }
        }
    }
}
