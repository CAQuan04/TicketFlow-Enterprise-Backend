using FluentValidation.Results; // Use FluentValidation types.

namespace TicketBooking.Application.Common.Exceptions
{
    // Custom exception to be thrown when input data fails business validation rules.
    public class ValidationException : Exception
    {
        // Property to hold the list of validation errors (Key: Field Name, Value: Error Messages).
        public IDictionary<string, string[]> Errors { get; }

        // Default constructor initializing an empty error list.
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        // Constructor accepting a list of ValidationFailure objects from FluentValidation.
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this() // Call the base constructor first.
        {
            // Group failures by property name and convert to a dictionary.
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage) // Group by PropertyName.
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray()); // Convert to Dictionary<string, string[]>.
        }

        // Constructor 3: For Manual Business Logic Errors (e.g., Check Capacity, Check Date logic in Handler)
        // Cho phép Handler tự tạo danh sách lỗi và ném ra.
        public ValidationException(IDictionary<string, string[]> errors)
            : this()
        {
            Errors = errors;
        }
    }
}