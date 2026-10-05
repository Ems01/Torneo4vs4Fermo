using Microsoft.Extensions.DependencyInjection;
using Torneo4vs4.ViewModels;

namespace Torneo4vs4.Views.Matches;

public partial class MatchesPage : ContentPage
{
    private MatchesViewModel? _viewModel;

    public MatchesPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Se abbiamo già recuperato il ViewModel,
        // non dobbiamo farlo nuovamente.
        if (_viewModel is not null)
            return;

        IServiceProvider? services =
            Handler?.MauiContext?.Services;

        if (services is null)
            return;

        // Recupera il ViewModel tramite Dependency Injection.
        _viewModel =
            services.GetRequiredService<MatchesViewModel>();

        // Collega il ViewModel alla pagina XAML.
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel is null)
            return;

        // Ogni volta che entriamo nella pagina
        // rileggiamo le partite dal database.
        await _viewModel.LoadAsync();
    }
}