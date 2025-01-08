using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NezienRestaurant1.Data;
using NezienRestaurant1.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezienRestaurant1.ViewModels
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;



        public OrdersViewModel(DatabaseService databaseService) 
        {
            _databaseService = databaseService;
        }


        public ObservableCollection<OrderModel> Orders { get; set; } = [];

        //Return true if the order creation was successfull, false otherwise
        public async Task<bool> PlaceOrderAsync(CartModel[] cartItems, bool isPaidOnline) 
        {
            var orderItems = cartItems.Select(c => new OrderItem
            { 
                Icon = c.Icon,
                ItemId = c.ItemId,
                Name = c.Name,
                Price = c.Price,
                Quantity = c.Quantity,
            }).ToArray();


            var orderModel = new OrderModel
            {
                OrderDate = DateTime.Now,
                PaymentMode = isPaidOnline ? "Online" : "Cash",
                TotalAmountPaid = cartItems.Sum(c => c.Amount),
                TotalItemsCount = cartItems.Length,
                Items = orderItems
            }; 

            var errorMessage = await _databaseService.PlaceOrderAsync(orderModel);
            if (!string.IsNullOrEmpty(errorMessage)) 
            {
                //Order creation failed
                await Shell.Current.DisplayAlert("Error", errorMessage, "OK");
                return false;
            }
            // Creating order was a success

            Orders.Add(orderModel);
            await Toast.Make("Order placed successfully").Show();
            return true;
        }

        private bool _isInitialized;

        [ObservableProperty]
        private bool _isLoading;
        public async ValueTask InitializeAsync() 
        {
            if (_isInitialized)
                return;
            _isInitialized = true;
            IsLoading = true; 
            Orders.Clear();
            var dborders = await _databaseService.GetOrdersAsync();
            var orders = dborders.Select(o => new OrderModel
            {
                Id = o.Id,
                OrderDate = DateTime.Now,
                PaymentMode = o.PaymentMode,
                TotalAmountPaid = o.TotalAmountPaid,
                TotalItemsCount = o.TotalItemsCount,

            });


            foreach (var order in orders)
            { 
                Orders.Add(order);
            }
            IsLoading = false;
        }

        [ObservableProperty]
        private OrderItem[] _orderItems = [];


        [RelayCommand]
        private async Task SelectOrderAsync(OrderModel? order)
        {

            
            var preSelectedOrder = Orders.FirstOrDefault(o=> o.IsSelected);
            if (preSelectedOrder != null)
            { 
                preSelectedOrder.IsSelected = false;
                if (preSelectedOrder.Id == order?.Id)
                {
                    OrderItems = [];
                    return;
                }
            }

            if (order == null || order.Id == 0)
            {
                OrderItems = [];
                return;
            }
            IsLoading = true; 
            order.IsSelected = true;
            OrderItems = await _databaseService.GetOrderItemAsync(order.Id); 
            IsLoading = false;
        }
    }
}
