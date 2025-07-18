using System.Text.RegularExpressions;

public static class InputSanitizer
{
    // Removes potentially dangerous characters and tags
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags and HTML
        string sanitized = Regex.Replace(input, "<.*?>", string.Empty);

        // Remove common SQL/XSS injection characters
        sanitized = Regex.Replace(sanitized, @"[<>""'%;()&+]", string.Empty);

        // Normalize whitespace
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();

        return sanitized;
    }

    // Validates email format
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }
}
