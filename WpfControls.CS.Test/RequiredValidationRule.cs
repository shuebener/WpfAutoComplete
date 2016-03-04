namespace WpfControls.CS.Test
{

    using System;
    using System.Windows.Controls;
    public class RequiredValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var result = new ValidationResult(true, null);
            if (value == null || ReferenceEquals(value, DBNull.Value))
            {
                result = new ValidationResult(false, "Required!");
            }
            return result;
        }
    }
}
