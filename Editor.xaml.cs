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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Footsteps
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        public FrameworkElement ElementToClose;

        public Editor()
        {
            InitializeComponent();
            UpdateGuiState();
            _checkFontSize.OnWakeUp += CheckFontSizeNow;
            _timer.Interval = TimeSpan.FromSeconds(0.75);
            _timer.Tick += _timer_Tick;
            _buttonTimer.Interval = _timer.Interval;
            _buttonTimer.Tick += _buttonTimer_Tick;
            _buttonTimer.Start();
            if (MainMenu.Easy)
                modeFinalRadioButton.IsChecked = true;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            UpdateAnimationNow();
        }

        private void UpdateAnimationNow()
        {
            if (modeAnimateRadioButton.IsChecked == true)
            {
                _cursorPosition++;
                if (_cursorPosition > _program.Count)
                    _cursorPosition = 0;
                UpdateGuiState();
                NotifySoon();
            }
        }

        private double _commandFontSize = 48.0;

        private DispatcherTimer _timer = new DispatcherTimer();

        private DispatcherTimer _buttonTimer = new DispatcherTimer();

        // Start with 1 (the second step) because it looks good as a static image in
        // the designer.
        private int _buttonAnimationStep = 1;

        private string[] _buttonImages = new string[] { "Initial.png", "Center.png", "Final.png" };

        private void _buttonTimer_Tick(object sender, EventArgs e)
        {
            UpdateButtonAnimation();
        }

        private void UpdateButtonAnimation()
        {
            _buttonAnimationStep = (_buttonAnimationStep + 1) % _buttonImages.Length;
            Uri uri = new Uri("/Footsteps;component/Images/ModeButtons/" + _buttonImages[_buttonAnimationStep], UriKind.Relative);
            animateImage.Source = new BitmapImage(uri);
        }

        private void CheckFontSizeNow()
        {
            // Ideally we'd make the font bigger when we have space.  But that's a lot harder
            // than what we're doing.  This routine will shrink the font size if we don't have
            // room for everything.  We only make the font size bigger when we first start and
            // when someone hits the clear button.
            /*
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString()
                + ":  Starting CheckFontSizeNow() Available=" + sizeHelper.ActualHeight
                + ", Used=" + programWrapPanel.ActualHeight);
                */
            if (programWrapPanel.ActualHeight > sizeHelper.ActualHeight)
            {
                double newFontSize;
                if (_commandFontSize == 48.0)
                    newFontSize = 36.0;
                else if (_commandFontSize == 36.0)
                    newFontSize = 24.0;
                else if (_commandFontSize == 24.0)
                    newFontSize = 18.0;
                else
                    newFontSize = 12.0;
                //System.Diagnostics.Debug.WriteLine("Initial font size:  " + _commandFontSize
                //    + ", new font size:  " + newFontSize);
                if (_commandFontSize != newFontSize)
                {
                    _commandFontSize = newFontSize;
                    foreach (UIElement element in programWrapPanel.Children)
                    {   // WrapPanel doesn't have a FontSize property.  If it did I could just set
                        // the font size in one place and let the various elements inherit it.
                        // Presumably we're doing a similar amount of work, just most typing for
                        // me.
                        Control control = element as Control;
                        if (null != control)
                            control.FontSize = newFontSize;
                    }
                }
            }
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString()
            //    + ":  CheckFontSizeNow() finished");
        }

        private readonly WakeMeSoon _checkFontSize = new WakeMeSoon();

        /// <summary>
        /// Update the various controls in the GUI to match the internal state of this object.
        /// 
        /// For example, if the cursor is all the way on the left, disable the button that lets
        /// you move left, otherwise enable that button.
        /// 
        /// This is common in a lot of my windows.  Most of the functions in this program don't
        /// directly change the GUI.  When the GUI needs to match the STATE of this object,
        /// put the code in here.  This makes sure that EVERYTHING is right.  You don't have to
        /// think about which ACTIONS will affect which GUI elements.  If you tried, it would
        /// be easy to miss something.  But the bigger problem is that the program keeps
        /// changing, and you don't want to look in a lot places each time you make a change.
        /// 
        /// Note:  This is more than just looks.  By enabling the disabling certain controls we
        /// enforce some rules.  The code behind the delete left button, for example, doesn't
        /// check if it's legal to delete left or not.  If you tried it and it wasn't legal, we
        /// would probably fail badly.  UpdateGuiState() disables the button when it shouldn't
        /// be used.
        /// </summary>
        private void UpdateGuiState()
        {
            if (!IsInitialized)
                // Sometimes we call UpdateGuiState during the load process.  For example, if
                // you set a radio button to checked in the designer, you'll probably get this
                // call immediately after that radio button is created, before some other elements
                // in this user control have been created.  If UpdateGuiState tries to access
                // one of those elements that hasn't been created yet, this will fail badly.
                // Note that our constructor calls UpdateGuiState() right after the initialization
                // process finishes, so we aren't missing anything.
                return;

            deleteLeftButton.IsEnabled = moveLeftButton.IsEnabled = (_cursorPosition > 0) && !AnimationInProgress;
            deleteRightButton.IsEnabled = moveRightButton.IsEnabled = (_cursorPosition < _program.Count) && !AnimationInProgress;
            deleteAllButton.IsEnabled = _program.Count > 0;

            int currentPosition = programWrapPanel.Children.IndexOf(cursorLabel);
            if (currentPosition != _cursorPosition)
            {
                programWrapPanel.Children.RemoveAt(currentPosition);
                programWrapPanel.Children.Insert(_cursorPosition, cursorLabel);
            }

            /*
            int index = 0;
            foreach (UIElement child in programWrapPanel.Children)
            {
                Control control = child as Control;
                if (null != child)
                {
                    bool isSelected = index == _selectionIndex;
                    if (isSelected)
                    {
                        control.Foreground = SystemColors.HighlightTextBrush;
                        control.Background = SystemColors.HighlightBrush;
                    }
                    else
                    {
                        control.Foreground = SystemColors.ControlTextBrush;
                        control.Background = SystemColors.ControlLightBrush;
                    }
                }
                index++;
            }
            */
        }

        private static readonly Dictionary<Command, string> _commandStrings = new Dictionary<Command, string>
        {
            { Command.Up, "↑" },
            { Command.Down, "↓" },
            { Command.Left, "←" },
            { Command.Right, "→" }
        };

        public static String ToString(Command command)
        {
            return _commandStrings[command];
        }

        private void Add(Command command)
        {
            _program.Insert(_cursorPosition, command);
            Label commandIcon = new Label();
            commandIcon.Content = _commandStrings[command];
            commandIcon.FontSize = _commandFontSize;
            commandIcon.Margin = new Thickness(2);
            commandIcon.MouseDown += CommandIcon_MouseDown;
            programWrapPanel.Children.Insert(_cursorPosition, commandIcon);
            _cursorPosition++;
            UpdateGuiState();
            NotifySoon();
        }

        private void CommandIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            int newIndex = programWrapPanel.Children.IndexOf((UIElement)sender);
            if (newIndex == _selectionIndex)
                _selectionIndex = -1;
            else
                _selectionIndex = newIndex;
            UpdateEnabled();
            if ((modeSelectedRadioButton.IsChecked == true) || (modeAnimateRadioButton.IsChecked == true))
                NotifySoon();
            modeSelectedRadioButton.IsChecked = true;
            */
        }

        void NotifySoon()
        {
            ProgramChanged?.Invoke(this);
        }

        private int _cursorPosition = 0;

        public void Clear()
        {
            _commandFontSize = 48.0;
            _program.Clear();
            programWrapPanel.Children.Clear();
            programWrapPanel.Children.Add(cursorLabel);
            _cursorPosition = 0;
            cursorLabel.FontSize = _commandFontSize;
            UpdateGuiState();
            NotifySoon();
        }

        private List<Command> _program = new List<Command>();

        private bool AnimationInProgress
        {
            get
            {
                return modeAnimateRadioButton.IsChecked == true;
            }
        }

        private static readonly IList<Command> EMPTY_PROGRAM = new Command[0];

        public IList<Command> Program
        {
            get
            {
                if (modeInitialRadioButton.IsChecked == true)
                    return EMPTY_PROGRAM;
                else if (AnimationInProgress)
                    return _program.GetRange(0, _cursorPosition);
                else
                    return FullProgram;
            }
        }

        public IList<Command> FullProgram
        {
            get { return _program; }
            set
            {
                Clear();
                foreach (Command command in _program)
                    Add(command);
            }
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            Add(Command.Up);
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            Add(Command.Left);
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            Add(Command.Down);
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            Add(Command.Right);
        }

        public event Action<Editor> ProgramChanged;

        private void deleteLeftButton_Click(object sender, RoutedEventArgs e)
        {
            _program.RemoveAt(_cursorPosition - 1);
            programWrapPanel.Children.RemoveAt(_cursorPosition - 1);
            _cursorPosition--;
            UpdateGuiState();
            NotifySoon();
        }

        private void deleteRightButton_Click(object sender, RoutedEventArgs e)
        {
            _program.RemoveAt(_cursorPosition);
            programWrapPanel.Children.RemoveAt(_cursorPosition + 1);
            UpdateGuiState();
            NotifySoon();
        }

        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void sizeHelperImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  size helper size changed");
            _checkFontSize.RequestWakeUp();
        }

        private void programWrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  wrap panel size changed");
            _checkFontSize.RequestWakeUp();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  Editor unloaded.");
            _timer.Stop();
            _buttonTimer.Stop();
        }

        private void modeInitialButton_Click(object sender, RoutedEventArgs e)
        {
            modeInitialRadioButton.IsChecked = true;
            UpdateGuiState();
            NotifySoon();
        }

        private void modeInitialRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            UpdateGuiState();
            NotifySoon();
        }

        private void modeFinalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            UpdateGuiState();
            NotifySoon();
        }

        private void modeAnimateRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Start();
            UpdateAnimationNow();
            NotifySoon();
        }

        private void modeFinalButton_Click(object sender, RoutedEventArgs e)
        {
            modeFinalRadioButton.IsChecked = true;
        }

        private void modeAnimateButton_Click(object sender, RoutedEventArgs e)
        {
            modeAnimateRadioButton.IsChecked = true;
        }

        private void moveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            _cursorPosition--;
            UpdateGuiState();
        }

        private void moveRightButton_Click(object sender, RoutedEventArgs e)
        {
            _cursorPosition++;
            UpdateGuiState();
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != ElementToClose)
            {
                Panel parent = ElementToClose.Parent as Panel;
                if (null != parent)
                    parent.Children.Remove(ElementToClose);
            }
        }
    }
}
