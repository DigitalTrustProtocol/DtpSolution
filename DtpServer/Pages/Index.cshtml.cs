using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DtpServer.Pages
{
    public class IndexModel : PageModel
    {
        
        public IndexModel()
        {
        }

        public void OnGet()
        {
            // Temp hack for controlling data on the database
            //if (command == "cleandatabase")
            //{
            //    _context.ClearAllData();
            //}
        }
    }
}
