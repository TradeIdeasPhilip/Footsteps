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
    /// This control allows the end user to change a program.  This
    /// also includes options to close the current window and to
    /// start or stop the program from running.
    /// </summary>
    public partial class Editor : UserControl
    {
        /// <summary>
        /// When someone clicks on our home button, we remove this item.
        /// If this is null nothing will happen when you click the home button.
        /// We never gray out the home button.  We could, but it doesn't seem
        /// important because we almost never expect this to be null.
        /// </summary>
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

        /// <summary>
        /// How big are the arrows that we draw as part of the program.
        /// Sometimes we have to shrink them to make them fit.  The new
        /// items should be as small as the ones we already shrunk.  We
        /// use this size for the new ones.
        /// </summary>
        private double _commandFontSize = 48.0;

        /// <summary>
        /// This timer is used to update the program when the user selects animate mode.
        /// </summary>
        private DispatcherTimer _timer = new DispatcherTimer();

        /// <summary>
        /// This updates the animation on the animate button.
        /// (The button itself is animated to make it more like the thing it's trying to represent.)
        /// </summary>
        private DispatcherTimer _buttonTimer = new DispatcherTimer();

        // Start with 1 (the second step) because it looks good as a static image in
        // the designer.
        private int _buttonAnimationStep = 1;

        /// <summary>
        /// The button that asks the computer to animate your soltuion is itself animated.  We show
        /// an icon when we are at the beginning of the journey, with future step grayed out,
        /// an icon at the end with footsteps for history, and one in between.
        /// </summary>
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

        /// <summary>
        /// Something has changed.  See if we need to make the font smaller so the entire
        /// program is visible.
        /// </summary>
        private void CheckFontSizeNow()
        {
            // Ideally we'd make the font bigger when we have space.  But that's a lot harder
            // than what we're doing.  This routine will shrink the font size if we don't have
            // room for everything.  We only make the font size bigger when we first start and
            // when someone hits the clear button.
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
        }

        /// <summary>
        /// There might be several events related to the font size all at once.
        /// Wait for them all to finish.  We don't want to react to an intermedite result.  In
        /// particular, what if the area was temporarily very small but then it immediately
        /// grew again.  Our code isn't smart enought to deal with that.  We'd pick a small
        /// font size and never go back to the big one.
        /// </summary>
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
        }

        private static readonly Dictionary<Command, string> _commandStrings = new Dictionary<Command, string>
        {
            { Command.Up, "↑" },
            { Command.Down, "↓" },
            { Command.Left, "←" },
            { Command.Right, "→" }
        };

        /// <summary>
        /// The user never types these special unicode characters.  The input usually comes from a button.
        /// But we decided to display the arrows as unicode characters.  That was sometimes more convenient
        /// than making images.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static String ToString(Command command)
        {
            return _commandStrings[command];
        }

        /// <summary>
        /// Inserts a new command into the editor where the cursor is.  Displays the new program in our control
        /// and notifies any external listeners.
        /// </summary>
        /// <param name="command"></param>
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

        /// <summary>
        /// At one time we had a way to select a command.  Now we only put the cursor between
        /// two commands.  We might want to reuse this callback to use the mouse to move the
        /// cursor.  If you click on the left side of a command icon, you'll move to the cursor
        /// just to the left of that command.  Same thing on the right.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Notify any external listeners.
        /// 
        /// The name implies that we might use a WakeMeSoon object here.  We know that we sometimes
        /// make more than one change at a time, so we could consolidate the outgoing messages.
        /// </summary>
        void NotifySoon()
        {
            ProgramChanged?.Invoke(this);
        }

        /// <summary>
        /// 0 is to the left of the first command.  If there are no commands, 0 is to the left of
        /// where the first command would go, and no other values are legal here.  3 is between the
        /// 3rd and 4th commands.  3 would only be legal here if we had at least 3 commands.
        /// </summary>
        private int _cursorPosition = 0;

        /// <summary>
        /// Completely reset the program.  Update our own GUI and notify any external listeners.
        /// 
        /// Note that this is the only place where we make the font big again.  Ideally we'd do
        /// that more often, but it's difficult and I don't expect it to be a problem in real life.
        /// </summary>
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

        /// <summary>
        /// Note that this refers to the complete program, not just the part that we're
        /// sharing with the rest of our code.  More precisely, this corresponds to the
        /// FullProgram property, not the Program property.
        /// </summary>
        private List<Command> _program = new List<Command>();

        /// <summary>
        /// The main program isn't directly aware of this option.  The main program can't
        /// tell if the user asked to animate his program, or if he keeps retyping, then
        /// completely clearing his program over and over.
        /// </summary>
        private bool AnimationInProgress
        {
            get
            {
                return modeAnimateRadioButton.IsChecked == true;
            }
        }

        /// <summary>
        /// A small optimization.  So we don't have to create a lot of objects that
        /// are all empty lists.
        /// 
        /// Note that we actually use an array, not a list.  If someone were to try
        /// to change this object, it would fail at runtime.  So this automatically
        /// is a constant list.
        /// </summary>
        private static readonly IList<Command> EMPTY_PROGRAM = new Command[0];

        /// <summary>
        /// This is what we typically share with the main program.  The value depends on
        /// what program the user wrote, and what mode we are in.
        /// </summary>
        public IList<Command> Program
        {
            get
            {
                if (modeInitialRadioButton.IsChecked == true)
                    // "Hard" mode.  We only display the program as a list of steps.  We
                    // don't ask the listeners to display the result until the user switches
                    // modes.  Like running a real program.
                    return EMPTY_PROGRAM;
                else if (AnimationInProgress)
                    // Note that we use the cursor both for editing and for showing where
                    // we are in the animation.  We disable editing while we're displaying
                    // the animation.  That avoids some confusion.
                    return _program.GetRange(0, _cursorPosition);
                else
                    // Display everything.  So the user can see the results of his program.
                    // You can edit in this mode.  That's the default for the "easy" setting.
                    // It can also be interesting to change steps in the middle of your
                    // program while we're in this mode.
                    return FullProgram;
            }
        }

        /// <summary>
        /// Read or set the entire program, regardless of the mode.  This does a complete
        /// reset, including moving the cursor all the way to the left.  Use Add() if you
        /// want to make a more precise change.
        /// </summary>
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

        /// <summary>
        /// This gets called each time the value of the Program property changes.
        /// </summary>
        public event Action<Editor> ProgramChanged;

        /// <summary>
        /// Backspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteLeftButton_Click(object sender, RoutedEventArgs e)
        {
            _program.RemoveAt(_cursorPosition - 1);
            programWrapPanel.Children.RemoveAt(_cursorPosition - 1);
            _cursorPosition--;
            UpdateGuiState();
            NotifySoon();
        }

        /// <summary>
        /// Delete.  Currently this is disabled in the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// It was hard to find the event that I wanted.  I wanted to know any time the space
        /// available for the program changed.  E.g. someone resized the window.  I ended up
        /// creating a control that lives in the same grid cell where we put the program.
        /// This is invisible and it sits behind the program, so the user is not directly aware
        /// of it.  But it is attached to all four walls of the grid cell, and it calls this
        /// functions any time it gets resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sizeHelperImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  size helper size changed");
            _checkFontSize.RequestWakeUp();
        }

        /// <summary>
        /// The program displays itself in a specific grid cell.  It is attached to the bottom.
        /// So when you start, your arrows are at the bottom of the cell.  If it grows to be two
        /// rows tall, the first row moves up, and the second row is on the bottom of the grid
        /// cell.  If we have too much to display, the bottom row will stay at the bottom of the
        /// grid cell.  The top might be off the top of the grid cell in which case it will get
        /// clipped.
        /// 
        /// Any time the size of this element changes, we consider if we need a smaller font.
        /// We attempt to use the smaller font to avoid cutting anything off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void programWrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  wrap panel size changed");
            _checkFontSize.RequestWakeUp();
        }

        /// <summary>
        /// Note that we start the timer in the constructor, not the loaded event.  We assume
        /// that you'll never reuse one of these objects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + ":  Editor unloaded.");
            _timer.Stop();
            _buttonTimer.Stop();
        }

        /// <summary>
        /// If the user clicks on this ordinary Button, pass the message on to the
        /// nearby RadioButton.
        /// 
        /// If you put an image in a RadioButton it looks different from putting
        /// an image in a normal Button.  I wanted the look of a normal button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeInitialButton_Click(object sender, RoutedEventArgs e)
        {
            modeInitialRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Don't show the results of the program yet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeInitialRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            UpdateGuiState();
            NotifySoon();
        }

        /// <summary>
        /// Show the result of running the entire program.  If someone edits the program,
        /// immediately update the result based on the new program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeFinalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            UpdateGuiState();
            NotifySoon();
        }

        /// <summary>
        /// Show the program running one step at a time.  Use the cursor to show where 
        /// we are in the program.  Use a timer to move to the next step.  When we get
        /// to the end, automatically start over.  Disable editing.  The cursor means
        /// something different in animation mode, so this avoids some confusion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Go back to the main menu.  This doesn't just close the editor control.
        /// Typically it closes one or two viewers, and some additional controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
