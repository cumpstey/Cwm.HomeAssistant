using System.ComponentModel.DataAnnotations;

namespace Cwm.HomeAssistant.Config
{
    public enum ButtonType
    {
        [Display(Name = "button")]
        Push,

        [Display(Name = "hold-button")]
        Hold,

        [Display(Name = "hold-release-button")]
        HoldAndRelease,
    }
}
