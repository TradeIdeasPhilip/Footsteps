using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Footsteps
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void ShowOnTop(UserControl userControl)
        {
            userControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            userControl.VerticalAlignment = VerticalAlignment.Stretch;
            userControl.Width = double.NaN;
            userControl.Height = double.NaN;
            if (null == userControl.Background)
                userControl.Background = Background;
            Panel.SetZIndex(userControl, 2);
            grid.Children.Add(userControl);            
        }

        private void level1Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new MainWindow("___|S__|__G|___"));
        }

        public static bool Easy { get; private set; }

        private void easyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Easy = true;
        }

        private void hardRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Easy = false;
        }

        private void level2Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new MainWindow("_____|S_.__|__._G|_____"));
        }

        private void level3Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new MainWindow("S.___.G|_._X_._|_._._._|_._._._|_X_._X_|___.___"));

        }

        private void buildLevelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new CustomMap());
        }

        private void level4Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new DoubleMap("__.___|S_._G_|______|______", "__.___|S_.__G|______|______"));
        }

        private void level5Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new DoubleMap("S._G|____|_.__|__X_|_.__|____", "S._G|__X_|_.__|____|_.__|____"));
        }
    }
}
