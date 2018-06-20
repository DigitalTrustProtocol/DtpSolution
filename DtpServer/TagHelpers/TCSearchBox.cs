using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace DtpServer.TagHelpers
{
    public class TCSearchBox : TagHelper
    {

        public string Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.TagName = "form";    // Replaces <email> with <a> tag

            var html = $@"
            < form asp-page=""./Index"" method=""get"" >
                < div class=""form-actions no-color"">
                    <p>
                        Find:
                        <input type=""text"" name=""searchString"" value=""{Value}"" />
                        <input type=""submit"" value=""Search"" class=""btn btn-default"" /> |
                        <a asp-page=""./Index"">Back to full List</a>
                    </p>
                </div>
            </form>";

            output.Content.SetHtmlContent(html);
            //context.AllAttributes.All(p=> context. p)
            //output.Attributes.Add();
            output.TagMode = TagMode.StartTagAndEndTag;

        }
}
}
