using System.Diagnostics;
using System.Windows.Navigation;

namespace AcctISGenerator;

public partial class HelpWindow
{
    public HelpWindow()
    {
        InitializeComponent();
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri){UseShellExecute = true});
        e.Handled = true;
    }
}