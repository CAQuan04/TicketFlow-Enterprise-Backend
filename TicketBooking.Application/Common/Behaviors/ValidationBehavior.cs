using FluentValidation; // Import FluentValidation core.
using MediatR; // Import MediatR for pipeline behaviors.
using ValidationException = TicketBooking.Application.Common.Exceptions.ValidationException; // Alias our custom exception.

namespace TicketBooking.Application.Common.Behaviors
{
    // A pipeline behavior that wraps the request handling process to perform validation.
    // TRequest: The command/query type. TResponse: The result type.
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull // Ensure the request is not null.
    {
        // Inject all validators registered for this specific TRequest type.
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        // Constructor injection.
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators; // Assign validators to the private field.
        }

        // The method that intercepts the request.
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // 1. Check if there are any validators configured for this request type.
            if (!_validators.Any())
            {
                // No validators? Just continue to the next step (the actual Handler).
                return await next();
            }

            // 2. Create a validation context for the current request.
            var context = new ValidationContext<TRequest>(request);

            // 3. Execute all validators asynchronously in parallel.
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // 4. Collect all failures from all validators into a single list.
            var failures = validationResults
                .Where(r => r.Errors.Any()) // Filter results that have errors.
                .SelectMany(r => r.Errors) // Flatten the list of errors.
                .ToList(); // Convert to a List.

            // 5. If there are any validation failures, stop the pipeline and throw exception.
            if (failures.Any())
            {
                // Throw our custom ValidationException (which will be caught by GlobalExceptionHandler).
                throw new ValidationException(failures);
            }

            // 6. If validation passes, continue to the next step (the actual Handler).
            return await next();
        }
    }
}