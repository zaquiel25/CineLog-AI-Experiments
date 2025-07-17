// In Ezequiel_Movies/Models/ValidReleasedYearAttribute.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization; // Required for TextInfo if you want to enhance error message formatting later

namespace Ezequiel_Movies.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidReleasedYearAttribute : ValidationAttribute
    {
        public int MinimumYear { get; set; } = 1888; // Sets a default minimum year
        public int AllowFutureYears { get; set; } = 1; // Allows release year up to current year + this value

        // Constructor to allow setting custom error message
        public ValidReleasedYearAttribute()
        {
            // You can set a default error message here if you prefer,
            // or rely on the ErrorMessage property being set when the attribute is used.
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // This attribute considers a null value valid.
            // If the field is also [Required], that attribute will handle the null check.
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is int year)
            {
                int currentYear = DateTime.Today.Year;
                int maximumYear = currentYear + AllowFutureYears;

                if (year >= MinimumYear && year <= maximumYear)
                {
                    return ValidationResult.Success; // Validation passed
                }
                else
                {
                    // Use the ErrorMessage set when applying the attribute.
                    // If ErrorMessage is not set, it will try to use a default from base or this constructed one.
                    string errorMessage = ErrorMessageString;
                    if (string.IsNullOrEmpty(errorMessage)) // Fallback message if not set on attribute usage
                    {
                        errorMessage = $"The year must be between {MinimumYear} and {maximumYear}.";
                    }
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, MinimumYear, maximumYear));
                }
            }

// Handles edge case where non-integer value is passed to integer property validation
            return new ValidationResult("Invalid year format provided.");
        }

        // Helper to format the error message
        public string FormatErrorMessage(string displayName, int minYear, int maxYear)
        {
            // Use the ErrorMessage from the attribute if set, otherwise default.
            // This allows {0} for field name, {1} for min, {2} for max in the ErrorMessage string.
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString ?? "The field {0} must be between {1} and {2}.", displayName, minYear, maxYear);
        }
    }
}