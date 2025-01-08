using SQLite;

namespace NezienRestaurant1.Data
{
    //This is Menu Item Category mapping class with its properties
    public class MenuItemCategoryMapping
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MenuCategoryId { get; set; }
        public int MenuItemId { get; set; }
    }
}