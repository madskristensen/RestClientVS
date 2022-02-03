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
                switch (result.Response.Content.Headers.ContentType.MediaType)
                {
                    case "application/json":
                        var jsonString = await result.Response.Content.ReadAsStringAsync();
                        text.AppendLine(result.Response.Headers.ToString());
                        try
                        {
                            var jsonObject = JObject.Parse(jsonString);
                            text.AppendLine(jsonObject.ToString());
                        }
                        catch (Exception ex)
                        {
                            text.AppendFormat("** {0} : Error parsing JSON ( {1} ), raw content follows. **\r\r", nameof(RestClientVS), ex.GetBaseException().Message);
                            text.AppendLine(jsonString);
                        }
                        Control.Text = text.ToString();
                        break;
                    case "application/xml":
                        var xmlString = await result.Response.Content.ReadAsStringAsync();
                        text.AppendLine(result.Response.Headers.ToString());
                        try
                        {
                            var xmlElement = XElement.Parse(xmlString);
                            text.AppendLine(xmlElement.ToString());
                        }
                        catch (Exception ex)
                        {
                            text.AppendFormat("** {0} : Error parsing XML ( {1} ), raw content follows. **\r\r", nameof(RestClientVS), ex.GetBaseException().Message);
                            text.AppendLine(xmlString);
                        }
                        Control.Text = text.ToString();
                        break;
                    default:
                        Control.Text = await result.Response.ToRawStringAsync();
                        break;
                }
            }
            else
            {
                Control.Text = result.ErrorMessage;
            }
        }
    }
}
