using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.Fx.AspNetCore.Mvc.Validation
{
    public static class ModelStateEx
    {
        public static string ToDebugString(this ModelStateDictionary modelState)
        {
            IEnumerable<string> modelErrorMessages = modelState
                .Where(kvp => kvp.Value.ValidationState == ModelValidationState.Invalid)
                .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(err => err.ErrorMessage))}");

            return string.Join(System.Environment.NewLine, modelErrorMessages);
        }

        public static Errors ToErrorsDictionary(this ModelStateDictionary modelState)
        {
            var errors = new Errors();
            foreach (KeyValuePair<string, ModelStateEntry> keyValuePair in modelState)
            {
                errors.Add(keyValuePair.Key, keyValuePair.Value.Errors.Select(err => err.ErrorMessage));
            }

            return errors;
        }

        public static void Add(this ModelStateDictionary modelState, Errors errors)
        {
            foreach (KeyValuePair<string, string[]> keyValuePair in errors)
            {
                foreach (string errorMessage in keyValuePair.Value)
                {
                    modelState.AddModelError(keyValuePair.Key, errorMessage);
                }
            }
        }
    }
}
