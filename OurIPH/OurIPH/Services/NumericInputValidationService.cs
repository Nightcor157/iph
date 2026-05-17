using System.Globalization;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class NumericInputValidationService
    {
        public bool IsValid(string text, NumericInputRule rule, bool allowPartial)
        {
            if (rule == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(text))
            {
                return allowPartial;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (allowPartial && rule.AllowNegative && text == "-")
            {
                return true;
            }

            if (rule.AllowDecimal)
            {
                if (allowPartial && (text == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator || text == "."))
                {
                    return true;
                }

                decimal decimalValue;
                var style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
                if (rule.AllowNegative)
                {
                    style |= NumberStyles.AllowLeadingSign;
                }

                return decimal.TryParse(text, style, CultureInfo.CurrentCulture, out decimalValue)
                       || decimal.TryParse(text, style, CultureInfo.InvariantCulture, out decimalValue);
            }

            long integerValue;
            var integerStyle = rule.AllowNegative ? NumberStyles.AllowLeadingSign : NumberStyles.None;
            return long.TryParse(text, integerStyle, CultureInfo.CurrentCulture, out integerValue)
                   || long.TryParse(text, integerStyle, CultureInfo.InvariantCulture, out integerValue);
        }
    }
}
