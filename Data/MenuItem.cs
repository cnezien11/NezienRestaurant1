﻿using SQLite;

namespace NezienRestaurant1.Data
{
    //MenuItem class with its properties
    public class MenuItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

    }
}