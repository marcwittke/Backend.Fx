using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Backend.Fx.AspNetCore.Mvc.Validation
{
    public class RedirectBackToGetActionModelValidationFilter : ModelValidationFilter
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public RedirectBackToGetActionModelValidationFilter(IModelMetadataProvider modelMetadataProvider)
        {
            this._modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid && AcceptsHtml(context))
            {
                Errors errors = context.ModelState.ToErrorsDictionary();
                LogErrors(context, context.Controller.ToString(), errors);

                // return the same view, using the posted model again
                var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
                BeforeRedirect(viewData);
                context.Result = new ViewResult
                {
                    ViewName = context.RouteData.Values["action"].ToString(),
                    ViewData = viewData,

                };
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ClientException cex && AcceptsHtml(context))
            {
                LogErrors(context, context.Controller.ToString(), cex.Errors);
                context.ModelState.Add(cex.Errors);

                // return the same view, using the posted model again
                var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
                BeforeRedirect(viewData);
                context.Result = new ViewResult
                {
                        ViewName = context.RouteData.Values["action"].ToString(),
                        ViewData = viewData,

                };
                context.ExceptionHandled = true;
            }
        }

        protected virtual void BeforeRedirect(ViewDataDictionary viewData) { }
    }
}