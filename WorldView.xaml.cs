using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Footsteps
{
    /// <summary>
    /// Interaction logic for WorldView.xaml
    /// </summary>
    public partial class WorldView : UserControl, ProgramStateProvider
    {
        public WorldView()
        {
            InitializeComponent();
            MapString = "";
        }

        private String _mapString;
        private Map _map;

        // The Editor line below is supposed to give me a multi-line text box to edit this property.
        // It doesn't work.  It seems to be ignored.  I can only enter a single line.  :(
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public String MapString
        {
            get { return _mapString; }
            set
            {
                _mapString = value;
                _map = Map.FromOneString(value);
                mainGrid.Children.Clear();
                mainGrid.Children.Add(playerImage);
                mainGrid.ColumnDefinitions.Clear();
                for (int i = 0; i < _map.ColumnCount; i++)
                    mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                mainGrid.RowDefinitions.Clear();
                for (int i = 0; i < _map.RowCount; i++)
                    mainGrid.RowDefinitions.Add(new RowDefinition());
                for (int x = 0; x < _map.ColumnCount; x++)
                    for (int y = 0; y < _map.RowCount; y++)
                    {
                        CellType cellType = _map.GetCellType(x, y);
                        string source = null;
                        Stretch stretch = Stretch.Uniform;
                        switch (cellType)
                        {
                            case CellType.Blocked:
                                source = "Fence.PNG";
                                stretch = Stretch.UniformToFill;
                                break;
                            case CellType.Death:
                                source = "Death.PNG";
                                break;
                            case CellType.Goal:
                                source = "Goal.PNG";
                                break;
                        }
                        if (null != source)
                        {
                            Uri uri = new Uri("/Footsteps;component/Images/" + source, UriKind.Relative);
                            Image image = new Image();
                            image.Source = new BitmapImage(uri);
                            image.Stretch = stretch;
                            image.HorizontalAlignment = HorizontalAlignment.Center;
                            mainGrid.Children.Add(image);
                            Grid.SetColumn(image, x);
                            Grid.SetRow(image, y);
                        }
                    }
                RunProgram();
            }
        }

        private int _playerX;
        private int _playerY;

        private void CantMoveThere()
        {
            // TODO make a sound or something.
        }

        private void Die()
        {
            ProgramState = ProgramState.Lost; 
        }

        private void Success()
        {
            ProgramState = ProgramState.Won;
        }

        private List<UIElement> _footSteps = new List<UIElement>();

        private void TryToMove(int dx, int dy)
        {
            int previousX = _playerX;
            int previousY = _playerY;
            int newX = _playerX + dx;
            int newY = _playerY + dy;
            switch (_map.GetCellType(newX, newY))
            {
                case CellType.Blocked:
                    CantMoveThere();
                    break;
                case CellType.Death:
                    SetPlayerPosition(newX, newY);
                    Die();
                    break;
                case CellType.Goal:
                    SetPlayerPosition(newX, newY);
                    Success();
                    break;
                case CellType.Free:
                    SetPlayerPosition(newX, newY);
                    break;
            }
            if (((_playerX != previousX) || (_playerY != previousY)) 
                && (null != _footSteps))
            {
                double angle = Math.Atan2(dy, dx) / Math.PI * 180.0 + 90.0;
                angle = angle - 10 + _random.NextDouble() * 20;
                //Uri uri = new Uri("/Footsteps;component/Images/Death.PNG", UriKind.Relative);
                Uri uri = new Uri("/Footsteps;component/Images/foot_prints.png", UriKind.Relative);
                Image image = new Image();
                image.Source = new BitmapImage(uri);
                RotateTransform rotate = new RotateTransform(angle/*, 0.5, 0.5*/);
                image.RenderTransform = rotate;
                // If you set the origin of the RotateTransform object, that's measured in
                // pixels.  This is relative to the entire object.  0.5, 0.5 here means 
                // the center of the object.
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                image.Opacity = 0.15 + _random.NextDouble() * 0.1;
                // We want the foot steps image to take up 5/6 of the height of the grid cell.
                // The remaining 1/6 will be split between the space above and below the
                // image.  We randomly choose how much goes above and the rest below.  We use
                // a similar rule for the width of the image.  The Margin property would work
                // well if we wanted to reserve a certain number of pixels, but this reserves
                // a certain percentage of the entire space, and the number of pixels can
                // change if the user resizes the window.  Note that the system will
                // automatically add padding above and below or to the left and right of the
                // image to maintain the aspect ratio of the image.  We want to take 5/6 of
                // the space that remains after the system has taken away that space.  
                Grid offsetGrid = new Grid();
                double leftSpace = _random.NextDouble();
                ColumnDefinition leftPadding = new ColumnDefinition();
                leftPadding.Width = new GridLength(leftSpace, GridUnitType.Star);
                offsetGrid.ColumnDefinitions.Add(leftPadding);
                ColumnDefinition centerColumn = new ColumnDefinition();
                centerColumn.Width = new GridLength(5, GridUnitType.Star);
                offsetGrid.ColumnDefinitions.Add(centerColumn);
                ColumnDefinition rightPadding = new ColumnDefinition();
                rightPadding.Width = new GridLength(1 - leftSpace, GridUnitType.Star);
                offsetGrid.ColumnDefinitions.Add(rightPadding);
                double topSpace = _random.NextDouble();
                RowDefinition topPadding = new RowDefinition();
                topPadding.Height = new GridLength(topSpace, GridUnitType.Star);
                offsetGrid.RowDefinitions.Add(topPadding);
                RowDefinition centerRow = new RowDefinition();
                centerRow.Height = new GridLength(5, GridUnitType.Star);
                offsetGrid.RowDefinitions.Add(centerRow);
                RowDefinition bottomPadding = new RowDefinition();
                bottomPadding.Height = new GridLength(1 - topSpace, GridUnitType.Star);
                offsetGrid.RowDefinitions.Add(bottomPadding);
                offsetGrid.Children.Add(image);
                Grid.SetColumn(image, 1);
                Grid.SetRow(image, 1);
                mainGrid.Children.Add(offsetGrid);
                Grid.SetColumn(offsetGrid, previousX);
                Grid.SetRow(offsetGrid, previousY);
                // Save the foot steps here.  We often recreate all the foot steps from
                // scratch.  Before we do that we want to erase the old ones.
                _footSteps.Add(offsetGrid);
                /*
                mainGrid.Children.Add(image);
                Grid.SetColumn(image, previousX);
                Grid.SetRow(image, previousY);
                _images.Add(image);
                */
            }
        }

        private void SetPlayerPosition(int x, int y)
        {
            _playerX = x;
            _playerY = y;
            Grid.SetColumn(playerImage, _playerX);
            Grid.SetRow(playerImage, _playerY);
        }

        private IEnumerable<Command> _program = new List<Command>();

        public IEnumerable< Command > Program
        {
            get { return _program; }
            set
            {
                _program = value;
                RunProgram();
            }
        }

        private Random _random;

        private void RunProgram()
        {   // Reset the random number generator.  If we repeat the exact same steps
            // on the same board we should get the exact same results.  In particular,
            // if someone just adds one step, we will probably repeat all of the steps
            // from the beginning when we draw the board.  So we want to have the same
            // results.
            _random = new Random(GetHashCode());
            if (null != _footSteps)
            {
                foreach (UIElement item in _footSteps)
                    mainGrid.Children.Remove(item);
                _footSteps.Clear();
            }
            ProgramState = ProgramState.Running;
            SetPlayerPosition(_map.InitialX, _map.InitialY);
            foreach (Command command in _program)
            {
                if (ProgramState != ProgramState.Running)
                    break;
                switch (command)
                {
                    case Command.Up:
                        TryToMove(0, -1);
                        break;
                    case Command.Down:
                        TryToMove(0, 1);
                        break;
                    case Command.Left:
                        TryToMove(-1, 0);
                        break;
                    case Command.Right:
                        TryToMove(1, 0);
                        break;
                }
            }
        }

        private ProgramState _programState;

        public ProgramState ProgramState
        {
            get { return _programState; }
            private set
            {
                _programState = value;
                ProgramStateChanged?.Invoke();
            }
        }

        public event Action ProgramStateChanged;
    }

    public enum Command { Up, Down, Left, Right } 
}
