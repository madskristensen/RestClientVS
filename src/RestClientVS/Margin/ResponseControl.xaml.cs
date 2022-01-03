using System.Windows.Controls;
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
                Control.Text = await result.Response.ToRawStringAsync();
            }
            else
            {
                Control.Text = result.ErrorMessage;
            }
        }
    }
}
