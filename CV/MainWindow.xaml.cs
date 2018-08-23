namespace CV
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void PrintCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void PrintCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
                printDlg.PrintVisual(Content as Visual, "CV");
        }

        void UniformGrid_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}