using System.Linq;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.Fx.AspNetCore.Mvc.Validation
{
    public static class ModelStateEx
    {
        public static string ToDebugString(this ModelStateDictionary modelState)
        {
            var modelErrorMessages = modelState
                                     .Where(kvp => kvp.Value.ValidationState == ModelValidationState.Invalid)
                                     .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(err => err.ErrorMessage))}");

            return string.Join(System.Environment.NewLine, modelErrorMessages);
        }

        public static Errors ToErrorsDictionary(this ModelStateDictionary modelState)
        {
            Errors errors = new Errors();
            foreach (var keyValuePair in modelState)
            {
                errors.Add(keyValuePair.Key, keyValuePair.Value.Errors.Select(
                                   err => new Error(keyValuePair.Value.ValidationState, err.ErrorMessage)));
            }

            return errors;
        }

        
        public static void Add(this ModelStateDictionary modelState, Errors errors)
        {
            foreach (var keyValuePair in errors)
            {
                foreach (var error in keyValuePair.Value)
                {
                    modelState.AddModelError(
                            keyValuePair.Key == Errors.GenericErrorKey 
                                    ? string.Empty 
                                    : keyValuePair.Key,
                            error.Message);
                }
            }
        }
    }
}
