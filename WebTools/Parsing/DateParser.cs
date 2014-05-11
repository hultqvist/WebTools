using System;
using System.Globalization;

namespace SilentOrbit.Parsing
{
    /// <summary>
    /// Slighlty more permissive Date parser
    /// </summary>
	public static class DateParser
    {
        static readonly DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
        static readonly string[] formats = new string[]
        {
            "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", //dateTimeFormat.RFC1123Pattern,
            "ddd, dd MMMM yyyy HH':'mm':'ss 'GMT'",
            "ddd',' d MMM yyyy HH:mm:ss zzz",
            "ddd',' dd MMM yyyy HH:mm:ss zzz",
        };
        const DateTimeStyles style =
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal |
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault;

        public static bool TryParse(string value, out DateTime result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = DateTime.MinValue;
                return false;
            }

            value = ReplaceRfc822TimeZoneWithOffset(value);

            if (DateTime.TryParseExact(value, formats, dateTimeFormat, style, out result))
                return true;

            if (DateTime.TryParse(value, dateTimeFormat, style, out result))
                return true;

            //Console.WriteLine("PARSED: " + result);
            /*
			result = DateTime.MinValue;
			foreach(string f in formats)
			{
				Console.WriteLine("Test: " + DateTime.Now.ToString(f));
				if(DateTime.TryParseExact(value, f, dateTimeFormat, DateTimeStyles.None, out result))
				   return true;
			}
			*/
            return false;
        }
        //============================================================
        // From: http://argotic.codeplex.com/
        // RFC-822 FORMAT METHODS
        //============================================================

        #region ReplaceRfc822TimeZoneWithOffset(string value)

        /// <summary>
        /// Replaces the RFC-822 time-zone component with its offset equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-822 formatted date to convert.</param>
        /// <returns>A string containing a RFC-822 formatted date, with the <i>zone</i> component converted to its offset equivalent.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        /// <seealso cref="TryParseRfc822DateTime(string, out DateTime)"/>
        private static string ReplaceRfc822TimeZoneWithOffset(string value)
        {
            //------------------------------------------------------------
            //	Perform conversion
            //------------------------------------------------------------
            value = value.Trim();
            if (value.EndsWith("UT", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}GMT", value.TrimEnd("UT".ToCharArray()));
            }
            else if (value.EndsWith("EST", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-05:00", value.TrimEnd("EST".ToCharArray()));
            }
            else if (value.EndsWith("EDT", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-04:00", value.TrimEnd("EDT".ToCharArray()));
            }
            else if (value.EndsWith("CST", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-06:00", value.TrimEnd("CST".ToCharArray()));
            }
            else if (value.EndsWith("CDT", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-05:00", value.TrimEnd("CDT".ToCharArray()));
            }
            else if (value.EndsWith("MST", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-07:00", value.TrimEnd("MST".ToCharArray()));
            }
            else if (value.EndsWith("MDT", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-06:00", value.TrimEnd("MDT".ToCharArray()));
            }
            else if (value.EndsWith("PST", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-08:00", value.TrimEnd("PST".ToCharArray()));
            }
            else if (value.EndsWith("PDT", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-07:00", value.TrimEnd("PDT".ToCharArray()));
            }
            else if (value.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}GMT", value.TrimEnd("Z".ToCharArray()));
            }
            else if (value.EndsWith("A", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-01:00", value.TrimEnd("A".ToCharArray()));
            }
            else if (value.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}-12:00", value.TrimEnd("M".ToCharArray()));
            }
            else if (value.EndsWith("N", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}+01:00", value.TrimEnd("N".ToCharArray()));
            }
            else if (value.EndsWith("Y", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(null, "{0}+12:00", value.TrimEnd("Y".ToCharArray()));
            }
            else
            {
                return value;
            }
        }

        #endregion

    }
}

