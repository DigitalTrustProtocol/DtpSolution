using DtpCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DtpServer.AspNetCore.MVC.Filters
{
    /// <summary>
    /// Validate the package
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private Type _modelType;
        private Type _validator;


        /// <summary>
        /// 
        /// </summary>
        public ValidateModelAttribute(Type modelType, Type validatorType) : base()
        {
            _modelType = modelType;
            _validator = validatorType;
        }

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
            var validatorService = ServiceProvider.GetRequiredService(_validator) as IModelSchemaValidator;
            if (validatorService == null)
                return;

            var keyValue = context.ActionArguments.FirstOrDefault(p=> p.Value != null && p.Value.GetType() == _modelType);
            if(keyValue.Value == null)
            {
                context.Result = new BadRequestObjectResult($"Missing {_modelType.Name}");
                return;
            }
            var model = keyValue.Value;
            //var result = validatorService.Validate(model);
            //if (result == null)
//                return;

            //if(result.Errors.Count > 0)
            //{
                
            //    foreach (var item in result.Errors)
            //    {
            //        context.ModelState.AddModelError(_modelType.Name, item);
            //    }
                
            //    context.Result = new BadRequestObjectResult(context.ModelState);
            //    return;
            //}
        }
    }
}
