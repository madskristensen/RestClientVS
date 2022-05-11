using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RestClientVS
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>, IRatingConfig
    {
        [Category("General")]
        [DisplayName("Request timeout")]
        [Description("The number of seconds to allow the request to run before failing.")]
        [DefaultValue(20)]
        public int Timeout { get; set; } = 20;

        [Category("General")]
        [DisplayName("Enable validation")]
        [Description("Determines if error messages should be shown for unknown variables and incorrect URIs.")]
        [DefaultValue(true)]
        public bool EnableValidation { get; set; } = true;

        [Category("Response")]
        [DisplayName("Response Window Width")]
        [Description("The number of seconds to allow the request to run before failing.")]
        [Browsable(false)]
        [DefaultValue(500)]
        public int ResponseWindowWidth { get; set; } = 500;

        [Browsable(false)]
        public int RatingRequests { get; set; }
    }
}
