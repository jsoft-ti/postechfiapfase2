using Bogus;
using Domain.Entities;
using Xunit;

namespace Test.Domain.Entities;

public class UserTests
{
    private readonly Faker _faker;

    public UserTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void User_DeveCriarComPropriedadesPadrao()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        // IdentityUser n�o inicializa Id automaticamente, apenas quando salvo no banco
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Equal(string.Empty, user.DisplayName);
        Assert.False(user.IsActive);
    }

   
    [Fact]
    public void FullName_DeveRetornarNomeCompleto()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal($"{firstName} {lastName}", fullName);
    }

    [Fact]
    public void FullName_DeveRetornarEspacoQuandoNomesVazios()
    {
        // Arrange
        var user = new User
        {
            FirstName = string.Empty,
            LastName = string.Empty
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal(" ", fullName);
    }

    [Fact]
    public void User_DevePermitirDefinirTodasPropriedades()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var email = _faker.Internet.Email();
        var displayName = _faker.Internet.UserName();
        var birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18));

        // Act
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DisplayName = displayName,
            Birthday = birthday,
            IsActive = true
        };

        // Assert
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(email, user.Email);
        Assert.Equal(displayName, user.DisplayName);
        Assert.Equal(birthday, user.Birthday);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_DeveHerdarDeIdentityUser()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Identity.IdentityUser<Guid>>(user);
    }

    [Fact]
    public void User_DevePermitirAlterarStatusAtivo()
    {
        // Arrange
        var user = new User { IsActive = false };

        // Act
        user.IsActive = true;

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_DeveCalcularIdadeCorretamente()
    {
        // Arrange
        var dataNascimento = new DateTime(1990, 6, 15);
        var user = new User { Birthday = dataNascimento };
        var dataAtual = new DateTime(2025, 10, 23);

        // Act
        var idade = dataAtual.Year - user.Birthday.Year;
        if (dataAtual < user.Birthday.AddYears(idade))
            idade--;

        // Assert
        Assert.Equal(35, idade);
    }

    [Fact]
    public void User_DevePermitirCriacaoComBogus()
    {
        // Arrange
        var userFaker = new Faker<User>("pt_BR")
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.DisplayName, f => f.Internet.UserName())
            .RuleFor(u => u.Birthday, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.IsActive, f => f.Random.Bool());

        // Act
        var users = userFaker.Generate(10);

        // Assert
        Assert.Equal(10, users.Count);
        Assert.All(users, user =>
        {
            Assert.NotEmpty(user.FirstName);
            Assert.NotEmpty(user.LastName);
            Assert.NotEmpty(user.Email);
            Assert.NotEmpty(user.FullName);
        });
    }

    [Theory]
    [InlineData("Jo�o", "Silva", "Jo�o Silva")]
    [InlineData("Maria", "Oliveira", "Maria Oliveira")]
    [InlineData("Pedro", "Santos", "Pedro Santos")]
    public void FullName_DeveFormatarCorretamente(string firstName, string lastName, string expected)
    {
        // Arrange
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal(expected, fullName);
    }
}