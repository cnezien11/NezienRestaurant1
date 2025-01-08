using NezienRestaurant1.ViewModels;

namespace NezienRestaurant1.Pages;

public partial class ManageMenuItemPage : ContentPage
{
    private readonly ManageMenuItemsViewModel _manageMenuItemsViewModel;

    public ManageMenuItemPage(ManageMenuItemsViewModel manageMenuItemsViewModel)
	{
		InitializeComponent();
        _manageMenuItemsViewModel = manageMenuItemsViewModel;
        BindingContext = _manageMenuItemsViewModel;
        InitializeAsync();  
    }
    private async void InitializeAsync() => await _manageMenuItemsViewModel.InitializeAsync();

    private async void CategoriesListControl_OnCategorySelected(Models.MenuCategoryModel category) => await _manageMenuItemsViewModel.SelectCategoryCommand.ExecuteAsync(category.Id);
   

    private async void MenuItemsListControl_OnSelectItem(Data.MenuItem menuItem) => await _manageMenuItemsViewModel.EditMenuItemCommand.ExecuteAsync(menuItem);

    private void SaveMenuItemFormControl_OnCancel()
    {
        _manageMenuItemsViewModel.CancelCommand.Execute(null);

    }

    private async void SaveMenuItemFormControl_OnSaveItem(Models.MenuItemModel menuItemModel)
    {
        await _manageMenuItemsViewModel.SaveMenuItemCommand.ExecuteAsync(menuItemModel);
    }
}