using NezienRestaurant1.Data;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezienRestaurant1.Models
{
    public partial class OrderModel: ObservableObject
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalItemsCount { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string PaymentMode { get; set; } // Cash or Online
        public OrderItem[] Items { get; set; }


        [ObservableProperty]
        private bool _isSelected;
    }
}
