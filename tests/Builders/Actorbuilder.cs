namespace FilmApi.Tests.Builders;

public class ActorBuilder
{
    private string _id = "a1";
    private string _lastName = "Doe";
    private string _firstName = "John";
    private string _role = "Main Character";

    public static ActorBuilder AnActor() => new();

    public ActorBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ActorBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public ActorBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public ActorBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    public Actor Build() => new()
    {
        Id = _id,
        LastName = _lastName,
        FirstName = _firstName,
        Role = _role
    };
}