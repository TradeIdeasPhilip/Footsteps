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
    /// This is the main window.  This is what the user sees when he first starts the program.
    /// 
    /// Other screens are implemented as UserControl objects and placed on top of the other
    /// controls in this window.  At one time each level was it's own window, but that was
    /// hard to use sometimes in tablet mode.  Hitting the X to close a window is a lot easier
    /// with a mouse than your finger.  It was easy to miss the X and do nothing, or to hit it
    /// twice and close the main program.
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Display one of the levels.  It will completely cover the other controls on this window.
        /// </summary>
        /// <param name="userControl"></param>
        private void ShowOnTop(UserControl userControl)
        {   // Fill the entire window.
            userControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            userControl.VerticalAlignment = VerticalAlignment.Stretch;
            userControl.Width = double.NaN;
            userControl.Height = double.NaN;
            if (null == userControl.Background)
                // If the user control had no background, give it a normal background.
                // This will hide the other controls on the main window, like the list of
                // levels.  It will also prevent clicks from passing through to those
                // controls.
                userControl.Background = Background;
            // Show this above the other controls.
            Panel.SetZIndex(userControl, 2);
            grid.Children.Add(userControl);            
        }

        private void level1Button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new MainWindow("___|S__|__G|___"));
        }

        /// <summary>
        /// If you select easy mode, each time you start a level you will be in the mode where
        /// you see all your moves immediately.  If you select hard mode, each level will start
        /// in the mode where you don't see any results right away.  Of course, you can change
        /// that setting any time you want.  This is only the default for a new level.
        /// </summary>
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ShowOnTop(new MainWindow("  S  | ... |     |XxGxX"));
          
        }
    }
}
