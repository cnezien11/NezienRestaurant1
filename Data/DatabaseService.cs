using NezienRestaurant1.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezienRestaurant1.Data
{
    //In this class I am using SQLite for local storage of data.
    public  class DatabaseService : IAsyncDisposable
    {

        //Actual SQLite conection
        private readonly SQLiteAsyncConnection _connection;
        public DatabaseService() 
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NezienRest.db1");
           _connection = new SQLiteAsyncConnection(dbPath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
                
        }


        //DB Initialization of instances and creation of tables for different entities
        public async Task InitializeDatabaseAsync() 
        {
            //Creation of specified type <T> SQLite tables below.
            await _connection.CreateTableAsync<MenuCategory>();
            await _connection.CreateTableAsync<MenuItem>();
            await _connection.CreateTableAsync<MenuItemCategoryMapping>();
            await _connection.CreateTableAsync<Order>();
            await _connection.CreateTableAsync<OrderItem>();

            await SeedDataAsync();
        }

        private async Task SeedDataAsync() 
        {
            var firstCategory = await _connection.Table<MenuCategory>().FirstOrDefaultAsync();

            if (firstCategory != null)
                return; // Database already seeded

            var categories = SeedData.GetMenuCategories();
            var menuItems = SeedData.GetMenuItems();
            var mappings = SeedData.GetMenuItemCategoryMappings();

            await _connection.InsertAllAsync(categories);
            await _connection.InsertAllAsync(menuItems);
            await _connection.InsertAllAsync(mappings);
        }

        public async Task<MenuCategory[]>GetMenuCategoriesAsync() =>
            await _connection.Table<MenuCategory>().ToArrayAsync();

        public async Task<MenuItem[]>GetMenuItemsByCategoryAsync(int categoryId)
        {
            var query = @"
                           SELECT menu.*
                           FROM MenuItem As menu
                           INNER JOIN MenuItemCategoryMapping AS mapping 
                            ON menu.Id = mapping.MenuItemId
                                WHERE mapping.MenuCategoryId = ?
                        ";
            var menuItems = await _connection.QueryAsync<MenuItem>(query, categoryId);
            return [..menuItems];
        }


        //It will return error message or null (if the operation was successfull)
        public async Task<String?> PlaceOrderAsync(OrderModel model) 
        {
            var order = new Order 
            {
                 
                OrderDate = model.OrderDate,
                PaymentMode = model.PaymentMode,
                TotalAmountPaid = model.TotalAmountPaid,
                TotalItemsCount = model.TotalItemsCount,
            };


            
            if(await _connection.InsertAsync(order) > 0) 
            {
                //Order Inserted with success
                // Now we have newly inserted order Id in the order.Id
                //Now we can add the orderId to the orderItems and insert orderItems in the database
                foreach (var item in model.Items) 
                {
                    item.OrderId = order.Id;
                }
                if(await _connection.InsertAllAsync(model.Items) ==0) 
                { 
                    //OrderItems insert operationfailed
                    //Remove the newly inserted order in this method
                    await _connection.DeleteAsync(order);
                    return "Error in inserting order items";
                }
            }
            else 
            {
                return "Error in inserting the order";
            }
            model.Id = order.Id;    
            return null;
        }


        public async Task<Order[]>GetOrdersAsync() =>
            await _connection.Table<Order>().ToArrayAsync();


        public async Task<OrderItem[]> GetOrderItemAsync(long orderId) => 
            await _connection.Table<OrderItem>().Where(oi=> oi.OrderId == orderId).ToArrayAsync();
        
        public async Task<MenuCategory[]>GetCategoriesOfMenuItem(int menuItemId) 
        {
            //SQL Query that retrieves the MenuCategory data for the provided menuItemId.
            var query = @"
                      SELECT cat.*
                      FROM MenuCategory cat
                      INNER JOIN MenuItemCategoryMapping map 
                      ON cat.Id = map.MenuCategoryId
                      WHERE map.MenuItemId = ?
                        ";
            var categories = await _connection.QueryAsync<MenuCategory>(query, menuItemId); //Query execution
            return [.. categories];
        }

        public async Task<string?> SaveMenuItemAsync(MenuItemModel model) 
        {
            if(model.Id == 0) 
            {
                //Creating a new menu Item
                MenuItem menuItem = new()
                {
                    Id = model.Id,
                    Description = model.Description,
                    Icon = model.Icon,
                    Name = model.Name,
                    Price = model.Price,
                };
                if(await _connection.InsertAsync(menuItem) > 0) 
                {
                    var categoryMapping = model.SelectedCategories
                                            .Select(c=> new MenuItemCategoryMapping 
                                            { 
                                                Id = c.Id,
                                                MenuCategoryId = c.Id,
                                                MenuItemId = menuItem.Id,

                                            });

                    if (await _connection.InsertAllAsync(categoryMapping) > 0)
                    {
                        model.Id = menuItem.Id;
                        return null;
                    }

                    else 
                    {
                        //Menu item insertion was successfull 
                        //but category mapping insert operation failed 
                        // then we should remove the newly inserted menu item from the database
                        await _connection.DeleteAsync(menuItem);    
                    }
                }

                return "Error in saving menu item";
            }
            else 
            {
                //Updating an existing menu item

                string? errorMessage = null; 
                await _connection.RunInTransactionAsync(db => 
                {
                    var menuItem = db.Find<MenuItem>(model.Id);
                    menuItem.Description = model.Description;
                    menuItem.Icon = model.Icon;
                    menuItem.Name = model.Name;
                    menuItem.Price = model.Price;

                    if (db.Update(menuItem) == 0) //if this op fails then error message will be thrown
                    {
                        errorMessage = "Error in Updating menu item";
                        throw new Exception();
                    }

                    var deleteQuery = @"
                                DELETE FROM MenuItemCategoryMapping
                                WHERE MenuItemId = ?";
                    db.Execute(deleteQuery, menuItem.Id);

                    var categoryMapping = model.SelectedCategories
                                            .Select(c => new MenuItemCategoryMapping
                                            {
                                                Id = c.Id,
                                                MenuCategoryId = c.Id,
                                                MenuItemId = menuItem.Id,

                                            });
                    if(db.InsertAll(categoryMapping) == 0)
                    {
                        errorMessage = "Error in Updating menu item";
                        throw new Exception(); //if this op fails then error message will be thrown
                    }
                });

                return errorMessage; 
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null) 
            {
                await _connection.CloseAsync();
            }
        }


    }
}
