using CommunityToolkit.Maui.Views;

namespace NezienRestaurant1.Controls;

public partial class HelpPopup : Popup
{

	public const string Email = "belebiePCAD13@gmail.com";
	public const string Phone = "(347)-888-8888";

    public HelpPopup()
	{
		InitializeComponent();
	}

    private async void CloseLabel_Tapped(object sender, TappedEventArgs e)
    {
		await this.CloseAsync();
    }

	private async void CopyEmail_Tapped(Object sender, TappedEventArgs e) 
	{
		await Clipboard.Default.SetTextAsync(Email);
        copyEmailLabel.Text = "Copied";
		await Task.Delay(2000);  //Wait for 2 seconds to reset to copy to clipboard
		copyEmailLabel.Text = "Copy to Clipboard";
	}

    private async void CopyPhone_Tapped(Object sender, TappedEventArgs e)
    {
        await Clipboard.Default.SetTextAsync(Phone);
        copyPhonelLabel.Text = "Copied";
        await Task.Delay(2000);  //Wait for 2 seconds to reset to copy to clipboard
        copyPhonelLabel.Text = "Copy to Clipboard";
    }

}