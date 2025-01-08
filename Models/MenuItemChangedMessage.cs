using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezienRestaurant1.Models
{
    //Used to communicate a change in the MenuItemModel object
    public class MenuItemChangedMessage : ValueChangedMessage<MenuItemModel>
    {
        public MenuItemChangedMessage(MenuItemModel value) : base(value)
        {
        }
        public static MenuItemChangedMessage From(MenuItemModel value) => new(value);
    }

    public class NameChangedMessage : ValueChangedMessage<string>
    {
        public NameChangedMessage(string value) : base(value)
        {

        }
        public static NameChangedMessage From(string value) => new(value);
    }
}
