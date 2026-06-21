using System.Windows.Input;
using MauiApp16.Services;
using MauiApp16.Views;

namespace MauiApp16.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        // Акаунт
        private string _email;
        private string _password;
        private string _confirmPassword;

        // Профіль
        private string _firstName;
        private string _lastName;
        private DateTime _birthDate = DateTime.Today.AddYears(-18);
        private string _phoneNumber;
        private string _country;
        private string _avatarPath;
        private string _errorMessage;

        // Для редактора фото
        public static string PendingImagePath { get; set; }

        public RegistrationViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Реєстрація";
            Country = "Інше";

            RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
            PickAvatarCommand = new Command(async () => await PickAvatarAsync());
            EditAvatarCommand = new Command(async () => await EditAvatarAsync(), () => !string.IsNullOrEmpty(_avatarPath));
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        // Властивості
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }
        public DateTime BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }
        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }
        public string AvatarPath
        {
            get => _avatarPath;
            set
            {
                if (SetProperty(ref _avatarPath, value))
                {
                    OnPropertyChanged(nameof(AvatarSource));
                    OnPropertyChanged(nameof(HasAvatar));
                    ((Command)EditAvatarCommand).ChangeCanExecute();
                }
            }
        }
        public bool HasAvatar => !string.IsNullOrEmpty(_avatarPath);
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ImageSource AvatarSource =>
            !string.IsNullOrEmpty(_avatarPath)
                ? ImageSource.FromFile(_avatarPath)
                : ImageSource.FromFile("user_placeholder.png");

        // Список країн для Picker
        public List<string> Countries { get; } = new()
        {
            "Україна", "Польща", "Німеччина", "Франція", "США", "Велика Британія",
            "Канада", "Австралія", "Нідерланди", "Швейцарія", "Австрія", "Чехія",
            "Словаччина", "Румунія", "Болгарія", "Угорщина", "Іспанія", "Португалія",
            "Туреччина", "Ізраїль", "Інше"
        };

        // Команди
        public ICommand RegisterCommand { get; }
        public ICommand PickAvatarCommand { get; }
        public ICommand EditAvatarCommand { get; }
        public ICommand BackCommand { get; }

        // Методи
        private async Task RegisterAsync()
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Введіть email";
                return;
            }
            if (!Email.Contains('@') || !Email.Contains('.'))
            {
                ErrorMessage = "Введіть коректний email";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введіть пароль";
                return;
            }
            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль має бути не менше 6 символів";
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Паролі не співпадають";
                return;
            }
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Введіть ім'я та прізвище";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // RegisterAsync тепер також генерує PersonalCode
                var success = await _authService.RegisterAsync(Email.Trim(), Password);
                if (!success)
                {
                    ErrorMessage = "Користувач з таким email вже існує";
                    return;
                }

                await _authService.UpdateProfileAsync(
                    FirstName, LastName,
                    BirthDate == DateTime.Today.AddYears(-18) ? (DateTime?)null : BirthDate,
                    PhoneNumber, Country, _avatarPath);

                if (Application.Current.MainPage is AppShell shell)
                    shell.EnableFlyout();

                await Shell.Current.GoToAsync("//DashboardPage");
            }
            catch (Exception ex) { ErrorMessage = $"Помилка: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        private async Task PickAvatarAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Оберіть фото профілю"
                });
                if (result == null) return;

                var destFolder = Path.Combine(FileSystem.AppDataDirectory, "avatars");
                Directory.CreateDirectory(destFolder);
                var destPath = Path.Combine(destFolder, $"avatar_tmp_{DateTime.Now:yyyyMMddHHmmss}.jpg");

                using var stream = await result.OpenReadAsync();
                using var fs = File.OpenWrite(destPath);
                await stream.CopyToAsync(fs);

                // Відкриваємо редактор — шлях встановлюється тільки після підтвердження кропу
                ImageEditorPage.SourceImagePath = destPath;
                ImageEditorPage.OnSaved = (croppedPath) =>
                {
                    AvatarPath = croppedPath;  // ← встановлюємо тільки після збереження
                };
                await Shell.Current.GoToAsync("ImageEditorPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Помилка", $"Не вдалося завантажити фото: {ex.Message}", "OK");
            }
        }

        private async Task EditAvatarAsync()
        {
            var sourcePath = _avatarPath;
            if (string.IsNullOrEmpty(sourcePath)) return;

            ImageEditorPage.SourceImagePath = sourcePath;
            ImageEditorPage.OnSaved = (croppedPath) =>
            {
                AvatarPath = croppedPath;
            };
            await Shell.Current.GoToAsync("ImageEditorPage");
        }

        // Викликається після повернення з ImageEditorPage
        public void ApplyEditedAvatar()
        {
            if (!string.IsNullOrEmpty(PendingImagePath))
            {
                AvatarPath = PendingImagePath;
                PendingImagePath = null;
            }
        }
    }
}