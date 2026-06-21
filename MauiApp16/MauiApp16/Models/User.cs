using SQLite;

namespace MauiApp16.Models;

public class User : IEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

    // Профіль
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Country { get; set; }

    /// <summary>Генерується автоматично при реєстрації</summary>
    public string PersonalCode { get; set; }
    public string PhoneNumber { get; set; }

    /// <summary>Шлях до файлу аватара (локальний AppDataDirectory)</summary>
    public string AvatarPath { get; set; }

    // Обчислювані (не в БД)
    [Ignore]
    public string FullName =>
        !string.IsNullOrWhiteSpace(FirstName)
            ? $"{FirstName} {LastName}".Trim() 
            : Email;

    [Ignore]
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(FirstName) ? FirstName : Email;

    //[Ignore]
    //public string DisplayName =>
    //    !string.IsNullOrWhiteSpace(FirstName)
    //        ? $"{FirstName} {LastName}".Trim()
    //        : Email;
}

public interface IEntity
{
    int Id { get; set; }
}