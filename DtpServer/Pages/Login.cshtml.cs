using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using DtpServer.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DtpServer.Pages
{
    public class LoginModel : PageModel
    {

        private readonly IWebHostEnvironment _hostingEnv;
        private readonly IConfiguration _configuration;

        public bool IsAdmin { get; set; }
        public string ff { get; set; }

        public LoginModel(IWebHostEnvironment env, IConfiguration configuration)
        {
            _hostingEnv = env;
            _configuration = configuration;
        }


        public void OnGet(string pw)
        {
            IsAdmin = _hostingEnv.IsDevelopment();
            //HttpContext.Session.Set("isadmin", IsAdmin);
        }
    }
}