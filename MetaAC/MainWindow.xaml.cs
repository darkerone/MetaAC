
using Fluent;
using System.Windows;

namespace MetaAC
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        PrincipalViewModel principalViewModel;
        public MainWindow()
        {
            InitializeComponent();
            
            principalViewModel = new PrincipalViewModel();

            this.DataContext = principalViewModel;
            
        }
    }
}
