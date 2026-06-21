using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels
{
    public class SelectableItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class RecommendationsViewModel : BaseViewModel
    {
        private readonly IAIRecommendationService _aiService;

        // Вбудований ключ (замість введення вручну)
        // Використовується за замовчуванням, коли UseCustomApiKey = false
        private static readonly string _builtinKey = "";

        // Стан опитування
        private int _currentStep = 1;
        private string _subjectName = string.Empty;
        private string _subjectDescription = string.Empty;
        private int _taskCount = 10;
        private string _selectedDifficulty = "Середній";
        private string _selectedStudyTime = "В різний час";
        private string _additionalPrefs = string.Empty;
        private string _apiKey = string.Empty;

        // Стан UI
        private bool _useCustomApiKey = false;
        private bool _showApiKeyText = false;

        // Стан генерації
        private string _statusMessage = string.Empty;
        private string _generatedFilePath = string.Empty;
        private bool _isGenerated = false;
        private bool _isGenerationError = false;

        //  Кроки
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(IsStep1));
                    OnPropertyChanged(nameof(IsStep2));
                    OnPropertyChanged(nameof(IsStep3));
                    OnPropertyChanged(nameof(IsStep4));
                    OnPropertyChanged(nameof(IsStep5));
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(IsLastStep));
                    OnPropertyChanged(nameof(IsNotLastStep));
                    OnPropertyChanged(nameof(StepProgressText));
                    OnPropertyChanged(nameof(StepProgressValue));
                }
            }
        }

        public bool IsStep1 => CurrentStep == 1;
        public bool IsStep2 => CurrentStep == 2;
        public bool IsStep3 => CurrentStep == 3;
        public bool IsStep4 => CurrentStep == 4;
        public bool IsStep5 => CurrentStep == 5;
        public bool CanGoBack => CurrentStep > 1;
        public bool IsLastStep => CurrentStep == 5;
        public bool IsNotLastStep => CurrentStep < 5;
        public string StepProgressText => $"Крок {CurrentStep} з 5";
        public double StepProgressValue => CurrentStep / 5.0;

        //  Поля опитування
        public string SubjectName
        {
            get => _subjectName;
            set
            {
                SetProperty(ref _subjectName, value);
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(SummarySubject));
            }
        }

        public string SubjectDescription
        {
            get => _subjectDescription;
            set => SetProperty(ref _subjectDescription, value);
        }

        public int TaskCount
        {
            get => _taskCount;
            set
            {
                var clamped = Math.Clamp(value, 5, 25);
                SetProperty(ref _taskCount, clamped);
                OnPropertyChanged(nameof(TaskCountDisplay));
                OnPropertyChanged(nameof(SummaryTaskCount));
            }
        }

        public string TaskCountDisplay => $"{_taskCount} завдань";

        public string SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                SetProperty(ref _selectedDifficulty, value);
                OnPropertyChanged(nameof(IsDifficultyEasy));
                OnPropertyChanged(nameof(IsDifficultyMedium));
                OnPropertyChanged(nameof(IsDifficultyHard));
                OnPropertyChanged(nameof(SummaryDifficulty));
                OnPropertyChanged(nameof(SummaryDifficultyEmoji));
            }
        }

        public bool IsDifficultyEasy => _selectedDifficulty == "Легкий";
        public bool IsDifficultyMedium => _selectedDifficulty == "Середній";
        public bool IsDifficultyHard => _selectedDifficulty == "Складний";

        public string SelectedStudyTime
        {
            get => _selectedStudyTime;
            set
            {
                SetProperty(ref _selectedStudyTime, value);
                OnPropertyChanged(nameof(IsTimeMorning));
                OnPropertyChanged(nameof(IsTimeAfternoon));
                OnPropertyChanged(nameof(IsTimeEvening));
                OnPropertyChanged(nameof(IsTimeAny));
                OnPropertyChanged(nameof(SummaryStudyTime));
            }
        }

        public bool IsTimeMorning => _selectedStudyTime == "Вранці";
        public bool IsTimeAfternoon => _selectedStudyTime == "Вдень";
        public bool IsTimeEvening => _selectedStudyTime == "Ввечері";
        public bool IsTimeAny => _selectedStudyTime == "В різний час";

        public string AdditionalPrefs
        {
            get => _additionalPrefs;
            set => SetProperty(ref _additionalPrefs, value);
        }

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                SetProperty(ref _apiKey, value);
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        //  Кастомний API ключ — toggle + eye
        public bool UseCustomApiKey
        {
            get => _useCustomApiKey;
            set
            {
                SetProperty(ref _useCustomApiKey, value);
                OnPropertyChanged(nameof(IsApiKeyVisible));
                OnPropertyChanged(nameof(CanGenerate));
                OnPropertyChanged(nameof(ApiKeyToggleLabel));
            }
        }

        public bool ShowApiKeyText
        {
            get => _showApiKeyText;
            set
            {
                SetProperty(ref _showApiKeyText, value);
                OnPropertyChanged(nameof(IsPasswordMode));
                OnPropertyChanged(nameof(EyeIcon));
            }
        }

        public bool IsApiKeyVisible => UseCustomApiKey;
        public bool IsPasswordMode => !ShowApiKeyText;
        public string EyeIcon => ShowApiKeyText ? "👁" : "🙈";
        public string ApiKeyToggleLabel => UseCustomApiKey
            ? "🔑 Використовую свій API-ключ"
            : "🤖 Використовую вбудований ключ";

        //  Типи завдань
        public ObservableCollection<SelectableItem> TaskTypes { get; }

        //  Summary (попередній перегляд на кроці 5)
        public string SummarySubject =>
            string.IsNullOrWhiteSpace(SubjectName) ? "—" : SubjectName;

        public string SummaryTaskCount => $"{TaskCount} завдань";

        public string SummaryDifficulty => SelectedDifficulty;

        public string SummaryDifficultyEmoji => SelectedDifficulty switch
        {
            "Легкий" => "🟢",
            "Складний" => "🔴",
            _ => "🟡",
        };

        public string SummaryStudyTime => SelectedStudyTime;

        public string SummaryTaskTypes
        {
            get
            {
                var selected = TaskTypes?
                    .Where(t => t.IsSelected)
                    .Select(t => t.Emoji)
                    .ToList();
                return selected?.Any() == true
                    ? string.Join(" ", selected)
                    : "—";
            }
        }

        // Оновлення Summary при зміні вибору типів
        public void NotifySummaryTaskTypes() =>
            OnPropertyChanged(nameof(SummaryTaskTypes));

        //  Стан генерації
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string GeneratedFilePath
        {
            get => _generatedFilePath;
            set => SetProperty(ref _generatedFilePath, value);
        }

        public bool IsGenerated
        {
            get => _isGenerated;
            set => SetProperty(ref _isGenerated, value);
        }

        public bool IsGenerationError
        {
            get => _isGenerationError;
            set => SetProperty(ref _isGenerationError, value);
        }

        //  Валідація
        public bool CanGoNext =>
            !string.IsNullOrWhiteSpace(SubjectName) || CurrentStep > 1;

        // Генерувати можна: якщо вбудований ключ АБО введено кастомний
        public bool CanGenerate =>
            (!UseCustomApiKey || !string.IsNullOrWhiteSpace(ApiKey)) && !IsBusy;

        //  Команди
        public ICommand NextStepCommand { get; }
        public ICommand PrevStepCommand { get; }
        public ICommand SelectDifficultyCommand { get; }
        public ICommand SelectStudyTimeCommand { get; }
        public ICommand ToggleTaskTypeCommand { get; }
        public ICommand IncreaseTaskCountCommand { get; }
        public ICommand DecreaseTaskCountCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand GoToHelpCommand { get; }
        public ICommand SaveApiKeyCommand { get; }
        public ICommand ToggleUseCustomApiKeyCommand { get; }
        public ICommand ToggleApiKeyVisibilityCommand { get; }

        // Подія для тостів
        public event Action<string, string>? ShowToastRequested;

        //  Конструктор
        public RecommendationsViewModel(IAIRecommendationService aiService)
        {
            _aiService = aiService;
            Title = "AI-Рекомендації";

            TaskTypes = new ObservableCollection<SelectableItem>
            {
                new SelectableItem { Emoji = "🔬", Name = "Практичні завдання",   IsSelected = true  },
                new SelectableItem { Emoji = "📖", Name = "Теоретичні питання",    IsSelected = true  },
                new SelectableItem { Emoji = "📝", Name = "Тести та опитування",   IsSelected = false },
                new SelectableItem { Emoji = "🗂",  Name = "Проєкти та роботи",     IsSelected = false },
                new SelectableItem { Emoji = "📚", Name = "Читання матеріалів",    IsSelected = false },
                new SelectableItem { Emoji = "🗒",  Name = "Конспекти та нотатки",  IsSelected = false },
                new SelectableItem { Emoji = "🔁", Name = "Повторення пройденого", IsSelected = false },
            };

            // Оновлюємо Summary при зміні вибору типів
            foreach (var item in TaskTypes)
                item.PropertyChanged += (_, _) => NotifySummaryTaskTypes();

            NextStepCommand = new Command(OnNextStep, () => !IsBusy);
            PrevStepCommand = new Command(OnPrevStep, () => !IsBusy);
            SelectDifficultyCommand = new Command<string>(OnSelectDifficulty);
            SelectStudyTimeCommand = new Command<string>(OnSelectStudyTime);
            ToggleTaskTypeCommand = new Command<SelectableItem>(OnToggleTaskType);
            IncreaseTaskCountCommand = new Command(() => TaskCount++);
            DecreaseTaskCountCommand = new Command(() => TaskCount--);
            GenerateCommand = new Command(async () => await OnGenerateAsync(),
                                                        () => CanGenerate);
            GoToHelpCommand = new Command(async () =>
                                                await Shell.Current.GoToAsync("HelpPage"));
            SaveApiKeyCommand = new Command(async () => await SaveApiKeyAsync());
            ToggleUseCustomApiKeyCommand = new Command(() =>
                                                UseCustomApiKey = !UseCustomApiKey);
            ToggleApiKeyVisibilityCommand = new Command(() =>
                                                ShowApiKeyText = !ShowApiKeyText);

            _ = LoadSavedApiKeyAsync();
        }

        //  Обробники команд
        private void OnNextStep()
        {
            if (CurrentStep == 1 && string.IsNullOrWhiteSpace(SubjectName))
            {
                StatusMessage = "⚠️ Введіть назву предмету";
                return;
            }
            StatusMessage = string.Empty;

            if (CurrentStep < 5)
            {
                CurrentStep++;
                FireToastForStep(CurrentStep);
            }
        }

        private void OnPrevStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
                StatusMessage = string.Empty;
            }
        }

        private void OnSelectDifficulty(string difficulty) => SelectedDifficulty = difficulty;
        private void OnSelectStudyTime(string time) => SelectedStudyTime = time;

        private void OnToggleTaskType(SelectableItem item)
        {
            if (item == null) return;
            item.IsSelected = !item.IsSelected;
        }

        private async Task OnGenerateAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectName))
            {
                StatusMessage = "⚠️ Поверніться до кроку 1 і введіть назву предмету.";
                IsGenerationError = true;
                return;
            }

            // Визначаємо ключ
            var keyToUse = UseCustomApiKey ? ApiKey.Trim() : _builtinKey;

            if (string.IsNullOrWhiteSpace(keyToUse))
            {
                StatusMessage = "⚠️ Введіть API-ключ Anthropic.";
                IsGenerationError = true;
                return;
            }

            IsBusy = true;
            IsGenerated = false;
            IsGenerationError = false;
            StatusMessage = "⏳ Генерую навчальний план... Зачекайте до 60 секунд.";
            GeneratedFilePath = string.Empty;

            try
            {
                var selectedTypes = TaskTypes
                    .Where(t => t.IsSelected)
                    .Select(t => t.Name)
                    .ToList();

                var request = new AIRecommendationRequest
                {
                    SubjectName = SubjectName.Trim(),
                    SubjectDescription = SubjectDescription.Trim(),
                    TaskCount = TaskCount,
                    DifficultyLevel = SelectedDifficulty,
                    SelectedTaskTypes = selectedTypes,
                    StudyTime = SelectedStudyTime,
                    AdditionalPreferences = AdditionalPrefs.Trim()
                };

                var backup = await _aiService.GenerateStudyPlanAsync(request, keyToUse);

                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(backup, jsonOptions);

                var safeName = SubjectName
                    .Replace(" ", "_")
                    .Replace("/", "-")
                    .Replace("\\", "-");
                var fileName = $"ai_plan_{safeName}_{DateTime.Now:yyyyMMdd_HHmm}.json";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(filePath, json);

                GeneratedFilePath = filePath;
                IsGenerated = true;
                StatusMessage =
                    $"✅ Готово! {backup.Courses.Count} предмет та {backup.Tasks.Count} завдань.";

                ShowToastRequested?.Invoke("🎉",
                    "Файл збережено! Перейдіть в Налаштування → «Імпортувати бекап»");
            }
            catch (Exception ex)
            {
                IsGenerationError = true;
                StatusMessage = $"❌ {ex.Message}";
                ShowToastRequested?.Invoke("⚠️",
                    "Помилка генерації. Перевірте з'єднання та спробуйте ще раз.");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        //  Тости по кроках
        private void FireToastForStep(int step)
        {
            switch (step)
            {
                case 2:
                    ShowToastRequested?.Invoke("💾",
                        "Результат — JSON-файл, який ви завантажите в Налаштуваннях");
                    break;
                case 3:
                    ShowToastRequested?.Invoke("✏️",
                        "Після імпорту кожне завдання можна відредагувати — просто натисніть на нього");
                    break;
                case 4:
                    ShowToastRequested?.Invoke("🎯",
                        "Чим точніші ваші відповіді, тим кращий план згенерує AI");
                    break;
                case 5:
                    ShowToastRequested?.Invoke("📂",
                        "Збережіть файл — він знадобиться для завантаження через Налаштування");
                    break;
            }
        }

        //  SecureStorage
        private async Task LoadSavedApiKeyAsync()
        {
            try
            {
                var saved = await SecureStorage.Default.GetAsync("anthropic_api_key");
                if (!string.IsNullOrWhiteSpace(saved))
                    ApiKey = saved;
            }
            catch { /* SecureStorage може бути недоступний */ }
        }

        private async Task SaveApiKeyAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiKey)) return;
            try
            {
                await SecureStorage.Default.SetAsync("anthropic_api_key", ApiKey.Trim());
                ShowToastRequested?.Invoke("🔑", "API-ключ збережено на пристрої");
            }
            catch { /* ігноруємо */ }
        }
    }
}
