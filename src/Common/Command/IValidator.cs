using System.Collections.Generic;

namespace Common.Command
{
    public interface IValidator<in TCommand>
    {
        IEnumerable<IValidationResult> Validate(TCommand argument);
    }

    public static class ValidationCommandExtensions
    {
        public static IEnumerable<IValidationResult> GetResults<T, TK>(this IEnumerable<T> validators, TK argument)
            where T : IValidator<TK>
        {
            var results = new List<IValidationResult>();
            foreach (var validator in validators)
            {
                results.AddRange(validator.Validate(argument));
            }

            return results;
        }
    }
}