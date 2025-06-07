using System.Text.RegularExpressions;

namespace Common;

public static class ValidationHelper
{
    public static bool IsValidPwd(string pwd)
    {
        return Regex.IsMatch(pwd, @"^(?=.*[^a-z])(?=.*\d).{8,}$", RegexOptions.None);
    }
        
    public static bool IsValidPin(string pin)
    {
        return pin.Length == 6 &&
               Regex.IsMatch(pin, @"^[0-9]{6,6}$", RegexOptions.None);
    }

    public static bool IsValidPhone(string candidate)
    {
        //http://regexlib.com/Search.aspx?k=phone+number&c=0&m=0&ps=20&p=11

        const string pattern =
            @"^([\\+][0-9]{1,3}([ \\.\\-])?)?([\\(]{1}[0-9]{3}[\\)])?([0-9A-Z \\.\\-]{1,32})((x|ext|extension)?[0-9]{1,4}?)$";

        var regex = new Regex(pattern);

        var result = regex.IsMatch(candidate);

        return result;
    }
    
    public static bool IsValidGuid(string guidString)
    {
        Guid parsedGuid;
        return Guid.TryParse(guidString, out parsedGuid);
    }

    public static bool IsValidEmail(string email)
    {
        // E-posta doğrulama mantığını buraya yazabilirsiniz
        return new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);
    }
}