using JetBrains.Annotations;

// this class is a workaround for a version incompatibility
// https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809


// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [UsedImplicitly]
    internal static class IsExternalInit {}
}