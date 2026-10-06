using Microsoft.Extensions.DependencyInjection;
using Torneo4vs4.ViewModels;

namespace Torneo4vs4.Views.Standings;

public partial class StandingsPage : ContentPage
{
    private StandingsViewModel? _viewModel;

    public StandingsPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Evita di recuperare più volte il ViewModel
        // nel caso in cui l'handler venga modificato.
        if (_viewModel is not null)
            return;

        IServiceProvider? services =
            Handler?.MauiContext?.Services;

        if (services is null)
            return;

        _viewModel =
            services.GetRequiredService<StandingsViewModel>();

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel is null)
            return;

        // Ogni volta che viene aperta la pagina,
        // la classifica viene ricalcolata utilizzando
        // i dati aggiornati presenti su Supabase.
        await _viewModel.LoadAsync();
    }
}