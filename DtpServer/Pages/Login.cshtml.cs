using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using DtpServer.Extensions;

namespace DtpServer.Pages
{
    public class LoginModel : PageModel
    {

        private readonly IConfiguration _configuration;

        public bool IsAdmin { get; set; }

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void OnGet(string pw)
        {
            IsAdmin = (pw == _configuration.GetValue<string>("adminpassword"));
            HttpContext.Session.Set("isadmin", IsAdmin);
        }
    }
}