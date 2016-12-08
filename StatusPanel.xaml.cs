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

namespace Footsteps
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            InitializeComponent();
            _wakeMeSoon.OnWakeUp += RefreshNow;
        }

        public Grid AnimationParent;

        private readonly Random _random = new Random();

        private readonly List<FrameworkElement> _sprayItems = new List<FrameworkElement>();

        private void RemoveSpray()
        {
            foreach (FrameworkElement element in _sprayItems)
            {
                Panel parent = element.Parent as Panel;
                if (null != parent)
                    parent.Children.Remove(element);
            }
        }

        private void Spray(Image source)
        {
            if (null == AnimationParent)
                return;
            double width = Math.Max(1.0, AnimationParent.ActualWidth - source.ActualWidth);
            double height = Math.Max(1.0, AnimationParent.ActualHeight - source.ActualHeight);
            Point initialLocation = source.TranslatePoint(new Point(0, 0), AnimationParent);
            int count = 20;
            double lastStartTime = 2;
            Duration duration = new Duration(TimeSpan.FromSeconds(2));

            for (int i = 0; i < count; i++)
            {
                TimeSpan beginTime = TimeSpan.FromSeconds(lastStartTime / (count - 1) * i);

                double finalX = _random.NextDouble() * width;
                double finalY = _random.NextDouble() * height;
                finalX = (finalX - initialLocation.X) * 3 + initialLocation.X;
                finalY = (finalY - initialLocation.Y) * 3 + initialLocation.Y;
                Image newImage = new Image();
                newImage.Source = source.Source;
                /*
                {
                    BitmapImage sourceImage;
                        Uri uri = new Uri("/Footsteps;component/Images/Goal.PNG", UriKind.Relative);
                        sourceImage = new BitmapImage(uri);
                    newImage.Source = sourceImage;
                    }
                    */
                Panel.SetZIndex(newImage, 10);
                newImage.ClipToBounds = false;

                Storyboard storyboard = new Storyboard();

                newImage.Width = source.ActualWidth;
                newImage.Height = source.ActualHeight;
                newImage.HorizontalAlignment = HorizontalAlignment.Left;
                newImage.VerticalAlignment = VerticalAlignment.Top;
                Thickness initialMargin = new Thickness(initialLocation.X, initialLocation.Y, 0, 0);
                newImage.Margin = initialMargin;
                Grid.SetColumnSpan(newImage, int.MaxValue);
                Grid.SetRowSpan(newImage, int.MaxValue);
                AnimationParent.Children.Add(newImage);
                _sprayItems.Add(newImage);

                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.From = initialMargin;
                thicknessAnimation.To = new Thickness(finalX, finalY, 0, 0);
                thicknessAnimation.Duration = duration;
                thicknessAnimation.BeginTime = beginTime;
                storyboard.Children.Add(thicknessAnimation);
                Storyboard.SetTarget(thicknessAnimation, newImage);
                Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Image.MarginProperty));

                double sizeFactor = (_random.NextDouble() + _random.NextDouble()) / 2 * 3 + 1;  // Between 1 and 4, somewhat gausian.
                DoubleAnimation heightAnimation = new DoubleAnimation();
                DoubleAnimation widthAnimation = new DoubleAnimation();
                heightAnimation.From = newImage.Height;
                widthAnimation.From = newImage.Width;
                heightAnimation.To = newImage.Height * sizeFactor;
                widthAnimation.To = newImage.Width * sizeFactor;
                heightAnimation.Duration = duration;
                widthAnimation.Duration = duration;
                heightAnimation.BeginTime = beginTime;
                widthAnimation.BeginTime = beginTime;
                storyboard.Children.Add(heightAnimation);
                storyboard.Children.Add(widthAnimation);
                Storyboard.SetTarget(heightAnimation, newImage);
                Storyboard.SetTarget(widthAnimation, newImage);
                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Image.HeightProperty));
                Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Image.WidthProperty));

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1;
                opacityAnimation.To = 0;
                opacityAnimation.Duration = duration;
                opacityAnimation.BeginTime = beginTime;
                storyboard.Children.Add(opacityAnimation);
                Storyboard.SetTarget(opacityAnimation, newImage);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Image.OpacityProperty));

                storyboard.Completed += delegate { AnimationParent.Children.Remove(newImage); };
                storyboard.Begin();

                /*

                Ellipse ellipse = new Ellipse();
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Stroke = new SolidColorBrush(Colors.Blue);
                Panel.SetZIndex(ellipse, 20);
                ellipse.ClipToBounds = false;
                ellipse.Width = 20; //source.ActualWidth;
                ellipse.Height = 15; // source.ActualHeight;
                ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse.VerticalAlignment = VerticalAlignment.Top;
                ellipse.Margin = new Thickness(finalX, finalY, 0, 0);
                Grid.SetColumnSpan(ellipse, int.MaxValue);
                Grid.SetRowSpan(ellipse, int.MaxValue);
                AnimationParent.Children.Add(ellipse);

                Line myLine = new Line();
                Panel.SetZIndex(myLine, 20);
                myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                myLine.X1 = initialLocation.X;
                myLine.Y1 = initialLocation.Y;
                myLine.X2 = finalX;
                myLine.Y2 = finalY;
                myLine.StrokeThickness = 2;
                Grid.SetColumnSpan(ellipse, int.MaxValue);
                Grid.SetRowSpan(ellipse, int.MaxValue);
                AnimationParent.Children.Add(myLine);
                */
            }
        }

        private ProgramState _previousProgramState = ProgramState.Running;

        private void RefreshNow()
        {
            ProgramState totalState = ProgramState.Won;
            foreach (ProgramStateProvider provider in _providers)
            {
                ProgramState state = provider.ProgramState;
                if (state > totalState)
                    totalState = state;
            }
            string source;
            string message;
            switch (totalState)
            {
                case ProgramState.Lost:
                    source = "Death.PNG";
                    message = "You lost.  Try again.";
                    break;
                case ProgramState.Running:
                    source = null;
                    message = "Move to the star.";
                    break;
                case ProgramState.Won:
                    source = "Goal.PNG";
                    message = "You won!";
                    break;
                default:
                    // Keep the compiler happy.
                    throw new Exception("Unexpected state.");
            }
            BitmapImage sourceImage;
            if (null == source)
                sourceImage = null;
            else
            {
                Uri uri = new Uri("/Footsteps;component/Images/" + source, UriKind.Relative);
                sourceImage = new BitmapImage(uri);
            }
            textBlock.Text = message;
            leftImage.Source = sourceImage;
            rightImage.Source = sourceImage;
            if (_previousProgramState != totalState)
            {
                RemoveSpray();
                if (null != sourceImage)
                    sprayRequired = true;
            }
            _previousProgramState = totalState;
        }

        private bool sprayRequired = false;

        private readonly HashSet<ProgramStateProvider> _providers = new HashSet<ProgramStateProvider>();

        private readonly WakeMeSoon _wakeMeSoon = new WakeMeSoon();

        public void AddProvider(ProgramStateProvider provider)
        {
            _providers.Add(provider);
            provider.ProgramStateChanged += delegate { _wakeMeSoon.RequestWakeUp(); };
            _wakeMeSoon.RequestWakeUp();
        }

        private void leftImage_LayoutUpdated(object sender, EventArgs e)
        {
            if (sprayRequired)
            {
                Spray(leftImage);
                Spray(rightImage);
                sprayRequired = false;
            }
        }
    }
}
