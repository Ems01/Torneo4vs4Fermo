using Microsoft.Extensions.DependencyInjection;
using Torneo4vs4.ViewModels;

namespace Torneo4vs4.Views.Teams;

public partial class TeamsPage : ContentPage
{
    private TeamsViewModel? _viewModel;

    public TeamsPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (_viewModel is not null)
            return;

        IServiceProvider? services =
            Handler?.MauiContext?.Services;

        if (services is null)
            return;

        _viewModel =
            services.GetRequiredService<TeamsViewModel>();

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel is null)
            return;

        await _viewModel.LoadAsync();
    }
}