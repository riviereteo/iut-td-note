namespace FilmApi.Tests.Builders;

public class CountryBuilder
{
    private string _code = "US";
    private string _name = "United States";

    public static CountryBuilder ACountry() => new();

    public CountryBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public CountryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Country Build() => new()
    {
        Code = _code,
        Name = _name
    };
}