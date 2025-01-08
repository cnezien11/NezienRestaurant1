﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using NezienRestaurant1.Data;
using NezienRestaurant1.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuItem = NezienRestaurant1.Data.MenuItem;


namespace NezienRestaurant1.ViewModels
{
    public partial class ManageMenuItemsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ManageMenuItemsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [ObservableProperty]
        private MenuCategoryModel[] _categories = [];

        [ObservableProperty]
        private MenuItem[] _menuItems = [];

        [ObservableProperty]
        private MenuCategoryModel? _selectedCategory = null;

        [ObservableProperty]
        private bool _isLoading;


        [ObservableProperty]
        private MenuItemModel _menuItem = new();


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

            foreach (var category in Categories) 
            {
                var categoryOfItem = new MenuCategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon,
                    IsSelected = false
                };
                MenuItem.Categories.Add(categoryOfItem);
            }

            IsLoading = false;
        }


        [RelayCommand]
        private async Task SelectCategoryAsync(int categoryId)
        {
            if (SelectedCategory.Id == categoryId)
                return; //The current category is already selected 

            var existingSelectedCategory = Categories.First(c => c.IsSelected);
            existingSelectedCategory.IsSelected = false;

            var newlySelectedCategory = Categories.First(c => c.Id == categoryId);
            newlySelectedCategory.IsSelected = true;

            SelectedCategory = newlySelectedCategory;

            MenuItems = await _databaseService.GetMenuItemsByCategoryAsync(SelectedCategory.Id);

            SetEmptyCategoriesToItem();

            IsLoading = false;
        }

        private void SetEmptyCategoriesToItem() 
        {
            foreach (var category in Categories)
            {
                MenuItem.Categories.Clear();
                var categoryOfItem = new MenuCategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon,
                    IsSelected = false
                };
                MenuItem.Categories.Add(categoryOfItem);
            }
        }

        [RelayCommand]
        private async Task EditMenuItemAsync(MenuItem menuItem) 
        {
            //await Shell.Current.DisplayAlert("Edit", "Edit menu item", "OK");
            var menuItemModel = new MenuItemModel
            {
                Description = menuItem.Description,
                Icon = menuItem.Icon,
                Id = menuItem.Id,   
                Name = menuItem.Name,
                Price = menuItem.Price,

            };

            var itemCategories = await _databaseService.GetCategoriesOfMenuItem(menuItem.Id);
            foreach(var category in Categories) 
            {
                var categoryOfItem = new MenuCategoryModel
                {
                    Icon = category.Icon,
                    Id = category.Id,
                    Name = category.Name,
                };

                if(itemCategories.Any(c=> c.Id == category.Id)) 
                {
                    categoryOfItem.IsSelected = true;
                }
                else 
                    categoryOfItem.IsSelected = false;

                menuItemModel.Categories.Add(categoryOfItem);
            }
            MenuItem = menuItemModel;
        }


        [RelayCommand]
        private void Cancel() 
        {
            MenuItem = new();
            SetEmptyCategoriesToItem();
        }

        [RelayCommand]
        private async Task SaveMenuItemAsync(MenuItemModel model) 
        {
            IsLoading = true;


            // save this item to DB
            var errorMessage = await _databaseService.SaveMenuItemAsync(model);

            if (errorMessage != null)
            {
                await Shell.Current.DisplayAlert("Error", errorMessage, "OK");
            }
            else 
            {

                await Toast.Make("Menu item has bee successfully saved").Show();
                HandleMenuItemChanged(model);

                //Send the updated menu item details to the parts of the app
                WeakReferenceMessenger.Default.Send(MenuItemChangedMessage.From(model));
                Cancel();
            }

            IsLoading = false;
        }

        private void HandleMenuItemChanged(MenuItemModel model)
        {
            var menuItem = MenuItems.FirstOrDefault(m=> m.Id == model.Id);
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
        }


    }


}
