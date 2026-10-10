namespace LanguageLMS.Core.Modules.Students.Requests;

public class GenerateStudentsRequest
{
    public int Count { get; set; } = 10;

    public string EmailPrefix { get; set; } = "student";

    public string Password { get; set; } = "student123";

    public string FullNamePrefix { get; set; } = "Student";
}
