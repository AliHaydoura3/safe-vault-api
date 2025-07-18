using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class InputSanitizerTests
{
    [TestCase("Robert'); DROP TABLE Students;--", "Robert DROP TABLE Students--")]
    [TestCase("'; EXEC xp_cmdshell('dir'); --", "EXEC xp_cmdshelldir --")]
    [TestCase("admin' OR '1'='1", "admin OR 11")]
    public void Sanitize_ShouldRemoveSqlInjectionPatterns(string input, string expected)
    {
        var result = InputSanitizer.Sanitize(input);
        result.Should().Be(expected);
    }

    [TestCase("<script>alert('XSS')</script>", "alertXSS")]
    [TestCase("<img src='x' onerror='alert(1)'/>", "img src=x onerror=alert1")]
    [TestCase("<div onclick='stealCookies()'>Click me</div>", "Click me")]
    public void Sanitize_ShouldRemoveXssScripts(string input, string expected)
    {
        var result = InputSanitizer.Sanitize(input);
        result.Should().Be(expected);
    }

    [TestCase("user@example.com", true)]
    [TestCase("user.name+tag@sub.domain.com", true)]
    [TestCase("invalid-email@", false)]
    [TestCase("user@.com", false)]
    public void IsValidEmail_ShouldValidateEmailFormat(string email, bool isValid)
    {
        var result = InputSanitizer.IsValidEmail(email);
        result.Should().Be(isValid);
    }
}
