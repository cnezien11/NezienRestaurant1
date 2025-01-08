using NezienRestaurant1.ViewModels;

namespace NezienRestaurant1.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _ordersViewModel;

    public OrdersPage(OrdersViewModel ordersViewModel)
	{
		InitializeComponent();
        _ordersViewModel = ordersViewModel;
        InitializeViewModelAsync();
        BindingContext = _ordersViewModel;
    }

    private async void InitializeViewModelAsync() =>
        await _ordersViewModel.InitializeAsync();

    
}