using SQLite;

namespace NezienRestaurant1.Data
{
    // MenuCategory class with its properties
    public class MenuCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}