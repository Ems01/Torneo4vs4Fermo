using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Torneo4vs4.Models;
using Torneo4vs4.Services;

namespace Torneo4vs4.ViewModels;

public class RulesViewModel : INotifyPropertyChanged
{
    private readonly RuleService _ruleService;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ObservableCollection<RuleSection> Rules
    {
        get;
    } = new();

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;

        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public RulesViewModel(
        RuleService ruleService)
    {
        _ruleService = ruleService;
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            List<RuleSection> rules =
                await _ruleService.GetRulesAsync();

            Rules.Clear();

            foreach (RuleSection rule in rules)
            {
                Rules.Add(rule);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Impossibile caricare il regolamento.\n{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    protected virtual void OnPropertyChanged(
        [CallerMemberName]
        string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}