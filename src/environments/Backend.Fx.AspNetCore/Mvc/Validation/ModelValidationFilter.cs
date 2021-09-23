﻿using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Backend.Fx.AspNetCore.Mvc.Validation
{
    public abstract class ModelValidationFilter : IActionFilter
    {
        public abstract void OnActionExecuting(ActionExecutingContext context);
        public abstract void OnActionExecuted(ActionExecutedContext context);

        protected void LogErrors(FilterContext context, string controllerName, Errors errors)
        {
            var logger = TryGetControllerType(controllerName, out var controllerType)
                ? LogManager.Create(controllerType)
                : LogManager.Create<ModelValidationFilter>();
            logger.Warn(
                $"Model validation failed during {context.HttpContext.Request.Method} {context.HttpContext.Request.PathBase}: " +
                string.Join(System.Environment.NewLine, errors.Select(err => err.ToString())));
        }

        protected bool AcceptsJson(FilterContext context)
        {
            IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
            return accept?.Any(mth => mth.Type == "application" && mth.SubType == "json") == true;
        }

        protected bool AcceptsHtml(FilterContext context)
        {
            IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
            return accept?.Any(mth => mth.Type == "text" && mth.SubType == "html") == true;
        }

        private static bool TryGetControllerType(string controllerName, out Type type)
        {
            try
            {
                type = Type.GetType(controllerName);
            }
            catch
            {
                type = null;
            }

            return type != null;
        }
    }
}
