using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezienRestaurant1.ViewModels
{
    public class SettingsViewModel
    {

        private const string NameKey = "name";
        private const string TaxPercentageKey = "tax";

        private bool _isInitialized;
        public async ValueTask InitializeAsync() 
        {
            if(_isInitialized) 
                return; 

            _isInitialized = true;

            var name = Preferences.Default.Get<string?>(NameKey, null);
            if (name is null) 
            {
                do
                {
                    name = await Shell.Current.DisplayPromptAsync("Your Name", "Enter your name");
                }

                while (string.IsNullOrEmpty(name));

                Preferences.Default.Set<string>(NameKey, name);
                  
                
            }

            WeakReferenceMessenger.Default.Send(NameChangedMessage.From(name));
        }
        public int GetTaxPercentage() => Preferences.Default.Get<int>(TaxPercentageKey, 0);
        public void SetTaxPercentage(int taxtPercentage) => Preferences.Default.Set<int>(TaxPercentageKey, taxtPercentage);
    }
}
