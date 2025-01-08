namespace NezienRestaurant1.Controls;

using CommunityToolkit.Mvvm.Input;
using NezienRestaurant1.Models;

public partial class SaveMenuItemFormControl : ContentView
{

    private const string DefaultIcon = "image_add_regular_36.png";

    public SaveMenuItemFormControl()
	{
		InitializeComponent();
	}

    
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(nameof(Item),
        typeof(MenuItemModel), typeof(SaveMenuItemFormControl), new MenuItemModel(), propertyChanged: OnItemChanged);
    public MenuItemModel Item 
    {
        get => (MenuItemModel)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private  static void OnItemChanged(BindableObject bindable, object oldValue, object newValue) 
    {
        if (newValue is MenuItemModel menuItemModel)
        {

            if (bindable is SaveMenuItemFormControl thisControl)
            {
                if (menuItemModel.Id > 0)
                {
                    //thisControl.itemIcon.Source = menuItemModel.Icon;
                    //thisControl.itemIcon.HeightRequest = thisControl.itemIcon.WidthRequest = 100;
                    thisControl.SetIconImage(false, menuItemModel.Icon, thisControl);
                    thisControl.ExistingIcon = menuItemModel.Icon;
                }
                else 
                {
                    //thisControl.itemIcon.Source = "image_add_regular_36.png";
                    //thisControl.itemIcon.HeightRequest = thisControl.itemIcon.WidthRequest = 36;
                    thisControl.SetIconImage(true, null, thisControl);

                }
            }
        }
    }   

    public string? ExistingIcon { get; set; }

    [RelayCommand]
    private void ToggleCategorySelection(MenuCategoryModel category) => category.IsSelected = !category.IsSelected;

    public event Action? OnCancel;

    [RelayCommand]
    private void Cancel() => OnCancel?.Invoke();

    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        var fileResult = await MediaPicker.PickPhotoAsync();
        if (fileResult != null)
        {
            //User selected an image from the image picker dialog
            //Upload/save the image on disc

            var imageStream = await fileResult.OpenReadAsync();

            var localPath = Path.Combine(FileSystem.AppDataDirectory, fileResult.FileName); 

            using var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write);
            await imageStream.CopyToAsync(fs);
            //Update the imageIcon on the UI
            //fs => fileStream

            //itemIcon.Source = localPath;
            //itemIcon.HeightRequest = itemIcon.WidthRequest = 100;

            SetIconImage(isDefault: false, localPath);

            Item.Icon = localPath;
        }
        else
        {
            //User can cancel from Image picker dialog

            
            if (ExistingIcon != null) 
            {
                SetIconImage(isDefault: false, ExistingIcon);
            }
            else 
            {
                SetIconImage(isDefault: true);
            }
        }

    }

    public void SetIconImage( bool isDefault, string? iconSource=null, SaveMenuItemFormControl? control =null) 
    {

        int size = 100;
        if (isDefault)
        {
            iconSource = DefaultIcon;
            size = 36;
        }

        control = control ?? this;

        control.itemIcon.Source = iconSource;
        control.itemIcon.HeightRequest = control.itemIcon.WidthRequest =size;
    }


    public event Action<MenuItemModel>? OnSaveItem;

    [RelayCommand]
    private async Task SaveMenuItemAsync() 
    {
        //Validation logic

        if (string.IsNullOrWhiteSpace(Item.Name) || string.IsNullOrWhiteSpace(Item.Description))
        {
            await ErrorAlertAsync( "Item name and decription are required");
            return;
        }

        if (Item.SelectedCategories.Length == 0) 
        {
            await ErrorAlertAsync("Please chose at least 1 categories");
            return;
        }

        if (Item.Icon == DefaultIcon)
        {
            await ErrorAlertAsync("Icon image is mandatory");
            return;
        }

        OnSaveItem?.Invoke(Item);
        static async Task ErrorAlertAsync(string message) =>
            await Shell.Current.DisplayAlert("Validation Error", message, "OK");
    }
}