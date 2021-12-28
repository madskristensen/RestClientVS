using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RestClientVS
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("General")]
        [DisplayName("Request timeout")]
        [Description("The number of seconds to allow the request to run before failing.")]
        [DefaultValue(20)]
        public int Timeout { get; set; } = 20;
    }
}
