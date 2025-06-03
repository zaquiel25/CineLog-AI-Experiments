// In Ezequiel_Movies/Models/NoFutureDateAttribute.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Ezequiel_Movies.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NoFutureDateAttribute : ValidationAttribute
    {
        // Constructor to allow setting custom error message
        public NoFutureDateAttribute()
        {
            // Default error message if not set by the user of the attribute
            ErrorMessage = "The {0} cannot be in the future.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // This attribute considers a null value valid.
            // If the field is also [Required], that attribute will handle the null check.
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateValue)
            {
                if (dateValue.Date <= DateTime.Today) // Compare only the Date part
                {
                    return ValidationResult.Success; // Validation passed
                }
                else
                {
                    // Use the ErrorMessage set when applying the attribute or the default one.
                    // The {0} placeholder will be replaced by the field's display name.
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }

            // This would be reached if the property type isn't DateTime or DateTime?
            return new ValidationResult($"Invalid date format for {validationContext.DisplayName}.");
        }

        // Override FormatErrorMessage to use the ErrorMessage property of the attribute
        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}