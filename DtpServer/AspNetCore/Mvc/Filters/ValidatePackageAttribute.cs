using DtpCore.Enumerations;
using DtpCore.Interfaces;
using DtpCore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DtpServer.AspNetCore.MVC.Filters
{
    /// <summary>
    /// Validate the package
    /// </summary>
    public class ValidatePackageAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Validate data
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            var ServiceProvider = context.HttpContext.RequestServices;
            var schemaService = ServiceProvider.GetRequiredService<IPackageSchemaService>();
            var keyValue = context.ActionArguments.FirstOrDefault(p=> p.Value is Package);
            if(keyValue.Value == null)
            {
                context.Result = new BadRequestObjectResult("Missing package");
                return;
            }
            var package = (Package)keyValue.Value;
            var result = schemaService.Validate(package, TrustSchemaValidationOptions.Full);

            if(result.Errors.Count > 0)
            {
                
                foreach (var item in result.Errors)
                {
                    context.ModelState.AddModelError("Package", item);
                }
                
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }
        }
    }
}
