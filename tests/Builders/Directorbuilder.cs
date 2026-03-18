namespace FilmApi.Tests.Builders;

public class DirectorBuilder
{
    private string _id = "1";
    private string _lastName = "Demme";
    private string _firstName = "Jonathan";
    private string _nationality = "American";
    private DateTime? _birthDate = new DateTime(1944, 2, 22);

    public static DirectorBuilder ADirector() => new();

    public DirectorBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public DirectorBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public DirectorBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public DirectorBuilder WithNationality(string nationality)
    {
        _nationality = nationality;
        return this;
    }

    public DirectorBuilder WithBirthDate(DateTime? birthDate)
    {
        _birthDate = birthDate;
        return this;
    }

    public DirectorBuilder WithNoBirthDate()
    {
        _birthDate = null;
        return this;
    }

    public Director Build() => new()
    {
        Id = _id,
        LastName = _lastName,
        FirstName = _firstName,
        Nationality = _nationality,
        BirthDate = _birthDate
    };
}