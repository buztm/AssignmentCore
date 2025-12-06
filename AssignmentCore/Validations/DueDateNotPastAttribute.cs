using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.Validators
{
    public class DueDateNotPastAttribute : ValidationAttribute
    {
        public DueDateNotPastAttribute()
        {
            ErrorMessage = "Due date cannot be in the past.";
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dueDate)
            {
                if (dueDate.Date < DateTime.UtcNow.Date)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success!;
        }
    }
}
