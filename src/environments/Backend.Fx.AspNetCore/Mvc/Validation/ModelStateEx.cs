using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.Fx.AspNetCore.Mvc.Validation;

public static class ModelStateEx
{
    public static Errors ToErrorsDictionary(this ModelStateDictionary modelState)
    {
        var dictionary = new Dictionary<string, string[]>();
        
        foreach (var keyValuePair in modelState)
        {
            dictionary.Add(keyValuePair.Key, keyValuePair.Value.Errors.Select(err => err.ErrorMessage).ToArray());    
        }

        return new Errors(dictionary);
    }


    public static void Add(this ModelStateDictionary modelState, Errors errors)
    {
        foreach (var (key, value) in errors)
        {
            foreach (var errorMessage in value)
            {
                modelState.AddModelError(key, errorMessage);
            }
        }
    }
}