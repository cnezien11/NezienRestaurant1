using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NezienRestaurant1.Models
{
    public partial class MenuItemModel : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private decimal _price;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _icon;

        public ObservableCollection<MenuCategoryModel> Categories { get; set; } = [];

        public MenuCategoryModel[] SelectedCategories => Categories.Where(c => c.IsSelected).ToArray();
    }
    
}

