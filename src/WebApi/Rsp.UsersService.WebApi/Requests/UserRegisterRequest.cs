namespace Rsp.UsersService.WebApi.Requests;

public class UserRegisterRequest
{
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; init; } = null!;

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; init; } = null!;

    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// The user's password.
    /// </summary>
    public static string Password => GetPassword();

    private static string GetPassword()
    {
        const int length = 8;
        const string specialChars = "!@#$%^&*()_+";
        const string numbers = "0123456789";
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        var special = specialChars[random.Next(specialChars.Length)];
        var number = numbers[random.Next(numbers.Length)];
        string password = new(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

        // insert numeric and alpha number characters into the password
        var index = (random.Next(password.Length), random.Next(password.Length));

        return password
                .Insert(index.Item1, special.ToString())
                .Insert(index.Item2, number.ToString());
    }
}