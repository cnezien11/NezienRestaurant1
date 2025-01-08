using CommunityToolkit.Mvvm.ComponentModel;
using NezienRestaurant1.Data;


namespace NezienRestaurant1.Models
{
    public partial class CartModel : ObservableObject
    { 
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public decimal Price { get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(Amount))]
        private int _quantity; 
        public decimal Amount=> Price * Quantity; 
    }
    
}

