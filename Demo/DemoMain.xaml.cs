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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Footsteps.Demo
{
    /// <summary>
    /// Draw the interactive help animations.
    /// </summary>
    public partial class DemoMain : UserControl
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        public DemoMain()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
            _timer.Tick += delegate { DoNextStep(); };
            DoNextStep();
        }

        private readonly List<Command> _program = new List<Command>();

        private void ClearProgram()
        {
            programStackPanel.Children.Clear();
            programStackPanel.Children.Add(sizePlaceHolder);
            _program.Clear();
            worldView.Program = _program;
        }

        private TextBlock Move(Command command)
        {
            _program.Add(command);
            worldView.Program = _program;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Editor.ToString(command);
            textBlock.FontSize = sizePlaceHolder.FontSize;
            programStackPanel.Children.Add(textBlock);
            return textBlock;
        }

        private int _demoStep = 0;

        private enum WhichDemo { FindTheGoal, Fences, Monsters, FindTheGoalLong, Monsters2 };

        private WhichDemo _whichDemo = WhichDemo.FindTheGoal;

        private void DemoComplete()
        {
            _demoStep = 0;
            if (_whichDemo == WhichDemo.Monsters2)
                _whichDemo = WhichDemo.FindTheGoal;
            else
                _whichDemo++;
        }

        private void Show(UIElement toChange, UIElement shouldBeVisible)
        {
            toChange.Visibility = (toChange == shouldBeVisible) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// In the designer you see three rows of instructions.  We only want to show one at
        /// a time to the user.
        /// </summary>
        /// <param name="instructions">The row we want to be visible.</param>
        private void ShowInstructions(UIElement instructions)
        {
            Show(instructionsPrincess, instructions);
            Show(instructionsFence, instructions);
            Show(instructionsMonster, instructions);
        }

        /// <summary>
        /// Slowly change the item to become mostly transparent.
        /// </summary>
        /// <param name="toFade"></param>
        private void AnimatedFade(UIElement toFade)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.25;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTarget(myDoubleAnimation, toFade);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Rectangle.OpacityProperty));
            myStoryboard.Begin();
        }

        private void DoNextStep()
        {
            int originalStep = _demoStep;
            _demoStep++;
            switch (_whichDemo)
            {
                case WhichDemo.FindTheGoal:
                    switch (originalStep)
                    {
                        case 0:
                            ClearProgram();
                            worldView.MapString = "S   |    |   G";
                            ShowInstructions(instructionsPrincess);
                            break;
                        case 1:
                            Move(Command.Right);
                            break;
                        case 2:
                            Move(Command.Down);
                            break;
                        case 3:
                            Move(Command.Down);
                            break;
                        case 4:
                            Move(Command.Right);
                            break;
                        case 5:
                            Move(Command.Right);
                            break;
                        default:
                            DemoComplete();
                            break;
                    }
                    break;
                case WhichDemo.Fences:
                    switch (originalStep)
                    {
                        case 0:
                            ClearProgram();
                            worldView.MapString = "S .   |  . . |    .G";
                            ShowInstructions(instructionsFence);
                            break;
                        case 1:
                            Move(Command.Down);
                            break;
                        case 2:
                            Move(Command.Right);
                            break;
                        case 3:
                            AnimatedFade(Move(Command.Right));
                            break;
                        case 4:
                            AnimatedFade(Move(Command.Right));
                            break;
                        case 5:
                            Move(Command.Down);
                            break;
                        case 6:
                            Move(Command.Right);
                            break;
                        case 7:
                            Move(Command.Right);
                            break;
                        case 8:
                            AnimatedFade(Move(Command.Right));
                            break;
                        case 9:
                            Move(Command.Up);
                            break;
                        case 10:
                            AnimatedFade(Move(Command.Left));
                            break;
                        case 11:
                            AnimatedFade(Move(Command.Right));
                            break;
                        case 12:
                            Move(Command.Up);
                            break;
                        case 13:
                            Move(Command.Right);
                            break;
                        case 14:
                            Move(Command.Right);
                            break;
                        case 15:
                            Move(Command.Down);
                            break;
                        case 16:
                            Move(Command.Down);
                            break;
                        case 17:
                            DemoComplete();
                            break;
                    }
                    break;
                case WhichDemo.Monsters:
                    switch (originalStep)
                    {
                        case 0:
                            ClearProgram();
                            worldView.MapString = "S   |  X |  XG";
                            ShowInstructions(instructionsMonster);
                            break;
                        case 1:
                            Move(Command.Down);
                            break;
                        case 2:
                            Move(Command.Right);
                            break;
                        case 3:
                            Move(Command.Right);
                            break;
                        default:
                            DemoComplete();
                            break;
                    }
                    break;
                case WhichDemo.FindTheGoalLong:
                    if (originalStep == 0)
                    {
                        ClearProgram();
                        worldView.MapString = "S   |    |   G";
                        ShowInstructions(instructionsPrincess);
                    }
                    else if (originalStep > FIND_THE_GOAL_LONG_STEPS.Length)
                        DemoComplete();
                    else
                        Move(FIND_THE_GOAL_LONG_STEPS[originalStep - 1]);
                    break;
                case WhichDemo.Monsters2:
                    if (originalStep == 0)
                    {
                        ClearProgram();
                        worldView.MapString = "S   | X  |   G";
                        ShowInstructions(instructionsMonster);
                    }
                    else if (originalStep > MONSTERS2_STEPS.Length)
                        DemoComplete();
                    else
                        Move(MONSTERS2_STEPS[originalStep - 1]);
                    break;
            }
        }

        private static readonly Command[] FIND_THE_GOAL_LONG_STEPS = new Command[] { Command.Right, Command.Right, Command.Right, Command.Down, Command.Left, Command.Down,
            Command.Left, Command.Left, Command.Up, Command.Right, Command.Up, Command.Right, Command.Right, Command.Down, Command.Down };

        private static readonly Command[] MONSTERS2_STEPS = new Command[] { Command.Right, Command.Right, Command.Down, Command.Down, Command.Left, Command.Left, Command.Up, Command.Up, Command.Right, Command.Right, Command.Right, Command.Down, Command.Down };

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }
    }
}
