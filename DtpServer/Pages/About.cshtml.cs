using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DtpServer.Pages
{
    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "The Digital Trust Protocol project developer";
        }
    }
}
