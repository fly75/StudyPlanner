using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;
using MauiApp16.Views;

namespace MauiApp16.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private User _currentUser;
        private bool _isEditing;

        // Поля редагування
        private string _editFirstName;
        private string _editLastName;
        private DateTime _editBirthDate = DateTime.Today;
        private string _editPhoneNumber;
        private string _editCountry;
        private string _editAvatarPath;
        private string _errorMessage;

        public ProfileViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Профіль";

            LoadCommand = new Command(async () => await LoadAsync());
            EditCommand = new Command(() => StartEditing());
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
            CancelEditCommand = new Command(() => CancelEditing());
            PickAvatarCommand = new Command(async () => await PickAvatarAsync());
            EditAvatarCommand = new Command(async () => await EditAvatarAsync(),
                () => !string.IsNullOrEmpty(_editAvatarPath ?? _currentUser?.AvatarPath));
        }

        // Список країн для Picker
        public List<string> Countries { get; } = new()
        {
            "Україна", "Польща", "Німеччина", "Франція", "США", "Велика Британія",
            "Канада", "Австралія", "Нідерланди", "Швейцарія", "Австрія", "Чехія",
            "Словаччина", "Румунія", "Болгарія", "Угорщина", "Іспанія", "Португалія",
            "Туреччина", "Ізраїль", "Інше"
        };

        // Властивості читання
        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(AvatarSource));
                    OnPropertyChanged(nameof(BirthDateDisplay));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string DisplayName =>
            _currentUser?.FirstName ?? _currentUser?.Email ?? string.Empty;

        public string BirthDateDisplay =>
            _currentUser?.BirthDate.HasValue == true
                ? _currentUser.BirthDate.Value.ToString("dd.MM.yyyy")
                : "Не вказано";

        public ImageSource AvatarSource
        {
            get
            {
                var path = _editAvatarPath ?? _currentUser?.AvatarPath;
                return !string.IsNullOrEmpty(path)
                    ? ImageSource.FromFile(path)
                    : ImageSource.FromFile("user_placeholder.png");
            }
        }

        public bool HasAvatar =>
            !string.IsNullOrEmpty(_editAvatarPath ?? _currentUser?.AvatarPath);

        // Поля редагування
        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (SetProperty(ref _isEditing, value))
                    OnPropertyChanged(nameof(IsNotEditing));
            }
        }
        public bool IsNotEditing => !_isEditing;

        public string EditFirstName
        {
            get => _editFirstName;
            set => SetProperty(ref _editFirstName, value);
        }
        public string EditLastName
        {
            get => _editLastName;
            set => SetProperty(ref _editLastName, value);
        }
        public DateTime EditBirthDate
        {
            get => _editBirthDate;
            set => SetProperty(ref _editBirthDate, value);
        }
        public string EditPhoneNumber
        {
            get => _editPhoneNumber;
            set => SetProperty(ref _editPhoneNumber, value);
        }
        public string EditCountry
        {
            get => _editCountry;
            set => SetProperty(ref _editCountry, value);
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Команди
        public ICommand LoadCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PickAvatarCommand { get; }
        public ICommand EditAvatarCommand { get; }

        // Методи
        public async Task InitializeAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                CurrentUser = user;
            }
            finally { IsBusy = false; }
        }

        private void StartEditing()
        {
            if (_currentUser == null) return;
            EditFirstName = _currentUser.FirstName;
            EditLastName = _currentUser.LastName;
            EditBirthDate = _currentUser.BirthDate ?? DateTime.Today.AddYears(-18);
            EditPhoneNumber = _currentUser.PhoneNumber;
            EditCountry = _currentUser.Country;
            _editAvatarPath = null;
            ErrorMessage = string.Empty;
            IsEditing = true;
            OnPropertyChanged(nameof(HasAvatar));
        }

        private void CancelEditing()
        {
            _editAvatarPath = null;
            ErrorMessage = string.Empty;
            IsEditing = false;
            OnPropertyChanged(nameof(AvatarSource));
            OnPropertyChanged(nameof(HasAvatar));
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName))
            {
                ErrorMessage = "Введіть ім'я та прізвище";
                return;
            }

            IsBusy = true;
            try
            {
                var avatarPath = _editAvatarPath ?? _currentUser?.AvatarPath;

                await _authService.UpdateProfileAsync(
                    EditFirstName, EditLastName,
                    EditBirthDate, EditPhoneNumber,
                    EditCountry, avatarPath);

                // AuthService кешує _currentUser і мутує той самий об'єкт.
                // SetProperty порівнює посилання — вони однакові → не спрацьовує.
                //
                // Рішення: тимчасово обнуляємо поле щоб SetProperty побачив "зміну"
                // null → updatedUser, і сам викличе OnPropertyChanged для всіх залежних
                // властивостей (AvatarSource, BirthDateDisplay, DisplayName).
                var updatedUser = _currentUser;
                _currentUser = null;                  // обходимо кеш SetProperty
                CurrentUser = updatedUser;           // тепер SetProperty бачить зміну → оновлює UI

                _editAvatarPath = null;
                IsEditing = false;

                await Application.Current.MainPage.DisplayAlert("✅", "Профіль збережено", "OK");
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
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
                var destPath = Path.Combine(destFolder,
                    $"avatar_tmp_{DateTime.Now:yyyyMMddHHmmss}.jpg");

                using var stream = await result.OpenReadAsync();
                using var fs = File.OpenWrite(destPath);
                await stream.CopyToAsync(fs);

                // Шлях встановлюємо ТІЛЬКИ після того як користувач підтвердив кроп.
                // Якщо натисне "Скасувати" в редакторі — аватар не зміниться.
                ImageEditorPage.SourceImagePath = destPath;
                ImageEditorPage.OnSaved = (croppedPath) =>
                {
                    _editAvatarPath = croppedPath;
                    OnPropertyChanged(nameof(AvatarSource));
                    OnPropertyChanged(nameof(HasAvatar));
                };
                await Shell.Current.GoToAsync("ImageEditorPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async Task EditAvatarAsync()
        {
            var currentPath = _editAvatarPath ?? _currentUser?.AvatarPath;
            if (string.IsNullOrEmpty(currentPath)) return;

            ImageEditorPage.SourceImagePath = currentPath;
            ImageEditorPage.OnSaved = (croppedPath) =>
            {
                _editAvatarPath = croppedPath;
                OnPropertyChanged(nameof(AvatarSource));
                OnPropertyChanged(nameof(HasAvatar));
            };
            await Shell.Current.GoToAsync("ImageEditorPage");
        }
    }
}