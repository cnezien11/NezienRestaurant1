using CommunityToolkit.Mvvm.ComponentModel;
using System;
using NezienRestaurant1.Data;
using NezienRestaurant1.Models;
using CommunityToolkit.Mvvm.Input;
using MenuItem = NezienRestaurant1.Data.MenuItem;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Messaging;
using System.Reflection;

namespace NezienRestaurant1.ViewModels;

public partial class HomeViewModel : ObservableObject, IRecipient<MenuItemChangedMessage>
{

    //comments section to be done when finish
    private readonly DatabaseService _databaseService;

    private readonly OrdersViewModel _ordersViewModel;
    private readonly SettingsViewModel _settingsViewModel;


    //holds menu categories such as Beverages, main course, desert and fast food
    [ObservableProperty]
    private MenuCategoryModel[] _categories = [];


    //This field stores menu items for the current category.
    [ObservableProperty]
    private MenuItem[] _menuItems = []; 


    //If a category is selected, this fiel will store it.
    [ObservableProperty]
    private MenuCategoryModel? _selectedCategory = null;


    //holds items in the cart when customer selects them.
    public ObservableCollection<CartModel> CartItems { get; set; } = [];


    //Enusure when the subtotal or tax per changes, the amount and total are updated automatically
    [ObservableProperty, NotifyPropertyChangedFor(nameof(TaxAmount)), NotifyPropertyChangedFor(nameof(Total))]
    private decimal _subtotal;

    [ObservableProperty,NotifyPropertyChangedFor(nameof(TaxAmount)), NotifyPropertyChangedFor(nameof(Total))]
    private int _taxPercentage;


    //Calculate the tax rate of the order and the tax amount
    public decimal TaxAmount => (Subtotal * TaxPercentage) / 100;

    public decimal Total => Subtotal + TaxAmount;


    //Default customer name is stored in this field
    [ObservableProperty]
    private string _name = "Guest"; 

    //represent wether the app is loading data or not.
    [ObservableProperty]
    private bool _isLoading;

    public HomeViewModel(DatabaseService databaseService, OrdersViewModel ordersViewModel, SettingsViewModel settingsViewModel) 
    { 
        _databaseService = databaseService;
        _ordersViewModel = ordersViewModel;
        _settingsViewModel = settingsViewModel;
        CartItems.CollectionChanged += CartItems_CollectionChanged;

        WeakReferenceMessenger.Default.Register<MenuItemChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<NameChangedMessage>(this, (recipient, message) => Name = message.Value);

        //Get Tax percentage from preferences
        TaxPercentage = _settingsViewModel.GetTaxPercentage();
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // It will be executed whenever we are adding any item to the cart
        //Removing item
        //Or clear
        RecalculateAmounts();   
    }

    private bool _isInitialized;
    public async ValueTask InitializeAsync() 
    {
        if (_isInitialized)
            return; //already initialized
        _isInitialized = true;

        IsLoading = true;

        Categories = (await _databaseService.GetMenuCategoriesAsync())
                      .Select(MenuCategoryModel.FromEntity)
                      .ToArray();

        Categories[0].IsSelected = true;

        SelectedCategory = Categories[0];

        MenuItems = await _databaseService.GetMenuItemsByCategoryAsync(SelectedCategory.Id);

        IsLoading = false;

        
    }

    //Using this attribute to simplify these implementations and allow a better user interaction with UI.
    [RelayCommand]
    private async Task SelectCategoryAsync(int categoryId) 
    { 
        if(SelectedCategory.Id == categoryId) 
            return; //The current category is already selected 

        var existingSelectedCategory = Categories.First(c=>c.IsSelected);
        existingSelectedCategory .IsSelected = false;

        var newlySelectedCategory = Categories.First(c => c.Id == categoryId);
        newlySelectedCategory.IsSelected = true; 

        SelectedCategory = newlySelectedCategory;
       
        MenuItems = await _databaseService.GetMenuItemsByCategoryAsync(SelectedCategory.Id);

        IsLoading = false;
    }

    [RelayCommand]
    private void AddToCart(MenuItem menuItem) 
    { 
        var cartItem = CartItems.FirstOrDefault(c=>c.ItemId == menuItem.Id);
        if (cartItem == null) 
        {
            //Item does not exist in the cart
            // add item to cart
            cartItem = new CartModel
            {
                ItemId = menuItem.Id,
                Icon = menuItem.Icon,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Quantity = 1

            };

            CartItems.Add(cartItem);    
        }
        else
        {
            //This item exists in cart
            //Increase the quantity for this item in the cart
            cartItem.Quantity++;
            RecalculateAmounts();
        }
        
    }
    [RelayCommand]
    private void IncreaseQuantity(CartModel cartItem) 
    {
        cartItem.Quantity++;
        RecalculateAmounts();
    }
    [RelayCommand]
    private void DecreaseQuantity(CartModel cartItem) 
    {
        cartItem.Quantity--;
        if (cartItem.Quantity == 0)
        { 
            CartItems.Remove(cartItem); 
        }
        else
            RecalculateAmounts() ;
    }
    [RelayCommand]
    private void RemoveItemFromCart(CartModel cartItem) 
    {
        CartItems.Remove(cartItem);
        RecalculateAmounts();
    }


    //Method to clear the cart when user does not want some of them
    [RelayCommand]
    private async  Task ClearCartAsync() 
    {
        if (await Shell.Current.DisplayAlert("Clear Cart?", "Do you really want to clear this cart?", "Yes", "No")) 
        {
            CartItems.Clear();
        }
    }

    private void RecalculateAmounts()
    { 
        Subtotal = CartItems.Sum(c => c.Amount);
    }


    [RelayCommand]
    private async Task TaxPercentageClickAsync() 
    {
        var result = await Shell.Current.DisplayPromptAsync("Tax Percentage", "Enter the applicable tax percentage", placeholder: "10", initialValue: TaxPercentage.ToString());
        if (!string.IsNullOrWhiteSpace(result)) 
        {
            if (!int.TryParse(result, out int enteredTaxPercentage)) 
            {
                await Shell.Current.DisplayAlert("Invalid Value", "Entered tax percentage is invalid", "OK");
                return;
            }
            //it was a valid mnumeric value
            if (enteredTaxPercentage > 100) 
            {
                await Shell.Current.DisplayAlert("Invalid Value", "Tax percentage cannot be more than 100", "OK");
                return; 
            }
            TaxPercentage = enteredTaxPercentage;


            //Save it in preferences
            _settingsViewModel.SetTaxPercentage(enteredTaxPercentage);

        }
    }


    [RelayCommand]
    private async Task PlaceOrderAsync(bool isPaidOnline) 
    {
        IsLoading = true;
        if(await _ordersViewModel.PlaceOrderAsync([..CartItems], isPaidOnline)) 
        {
            //Order creation successfull
            // Then clear the cart items
            CartItems.Clear();
        }
        IsLoading = false;
    }

    public void Receive(MenuItemChangedMessage message)
    {
        var model = message.Value;

        var menuItem = MenuItems.FirstOrDefault(m => m.Id == model.Id);
        if (menuItem != null)
        {
            //This menu item is on the screen 
            //Check if this item has a mapping to selected category

            if (!model.SelectedCategories.Any(c => c.Id == SelectedCategory.Id))
            {
                //This item no longer belongs to the selected category
                //Remove this item from the current UI Menu Items list
                MenuItems = [.. MenuItems.Where(m => m.Id != model.Id)];
                return;
            }
            //then the dtails will be updated
            menuItem.Price = model.Price;
            menuItem.Description = model.Description;
            menuItem.Name = model.Name;
            menuItem.Icon = model.Icon;

            MenuItems = [.. MenuItems];
        }

        else if (model.SelectedCategories.Any(c => c.Id == SelectedCategory.Id))
        {
            //If this item was not on the UI, we will update the item by adding this currently selected category
            //Then add this menu item to the current UI items list
            var newMenuItem = new MenuItem
            {
                Id = model.Id,
                Description = model.Description,
                Icon = model.Icon,
                Name = model.Name,
                Price = model.Price,
            };
            MenuItems = [.. MenuItems, newMenuItem];
        }

        //To check if the updated menu item is added in the cart
        //If yes, the update the infos int the cart
        var cartItem = CartItems.FirstOrDefault(c=> c.ItemId == model.Id);
        if (cartItem != null)
        { 
            cartItem.Price = model.Price;
            cartItem.Icon = model.Icon;
            cartItem.Name = model.Name;

            var itemIndex = CartItems.IndexOf(cartItem);

            //It will trigger CollectionChanged event for replacing this item
            // which will recalculate the amounts\

            CartItems[itemIndex] = cartItem;
        }
    }
}
