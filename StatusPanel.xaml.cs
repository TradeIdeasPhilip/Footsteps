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
    /// This displays a message like "you won!" with appropriate pictures.
    /// </summary>
    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            InitializeComponent();
            _wakeMeSoon.OnWakeUp += RefreshNow;
        }

        /// <summary>
        /// If this is not null we display animations all over the AnimationParent.
        /// This should not be changed while an animation is in progress.  Ideally
        /// this would be read only and set in the constructor.
        /// </summary>
        public Grid AnimationParent;

        private readonly Random _random = new Random();

        /// <summary>
        /// Items that we've created as part of an animation.  When we start a new animation
        /// the first step is to destroy all items from the previous animation.
        /// </summary>
        private readonly List<FrameworkElement> _sprayItems = new List<FrameworkElement>();

        /// <summary>
        /// When we start a new animation the first step is to destroy all items from the
        /// previous animation.
        /// </summary>
        private void RemoveSpray()
        {
            foreach (FrameworkElement element in _sprayItems)
            {
                Panel parent = element.Parent as Panel;
                if (null != parent)
                    parent.Children.Remove(element);
            }
        }

        /// <summary>
        /// Make a lot of copies of the source image.  Randomly send each copy spraying all
        /// over the AnimationParent.
        /// </summary>
        /// <param name="source">The image to copy.</param>
        private void Spray(Image source)
        {
            if (null == AnimationParent)
                return;
            // Spread these items randomly around the destination control.  Ideally the item
            // would completely fit on the window, that's why we look at the size of the source.
            // If the source is bigger than the AnimationParent, we can't do that.  However,
            // that would an odd case, so our only real concern is not crashing.
            double width = Math.Max(1.0, AnimationParent.ActualWidth - source.ActualWidth);
            double height = Math.Max(1.0, AnimationParent.ActualHeight - source.ActualHeight);
            // We want the animations to start from the position of the initial image.
            Point initialLocation = source.TranslatePoint(new Point(0, 0), AnimationParent);
            // The number of copies to make.
            int count = 20;
            // The items each start moving at a different time.  This says how long in seconds
            // to wait before we start moving the last item.
            double lastStartTime = 2;
            // How long each copy lasts.  This time starts from when the item starts moving.
            Duration duration = new Duration(TimeSpan.FromSeconds(2));

            for (int i = 0; i < count; i++)
            {   // Offset the start times.
                TimeSpan beginTime = TimeSpan.FromSeconds(lastStartTime / (count - 1) * i);

                // Randomly select an initial position from somewhere on the AnimationParent.
                double finalX = _random.NextDouble() * width;
                double finalY = _random.NextDouble() * height;

                // Spread out the images.  The origin is where the initial image is.
                // Each copy will move to be 3x as far from the origin, but in the same
                // direction.
                //
                // If the intial image is in the top left corner, of the AnimationParent, all
                // objects will spray down and/or right.  If the initial image is in the center,
                // objects will spary in all directions.  If the initial image is near but not
                // at the top, we'll allow some activity above the initial image, but most of
                // the activity will be below the initial image.
                finalX = (finalX - initialLocation.X) * 3 + initialLocation.X;
                finalY = (finalY - initialLocation.Y) * 3 + initialLocation.Y;

                // Each new image will have the same bitmap as the original.
                Image newImage = new Image();
                newImage.Source = source.Source;

                // We set this to high on all of our images.  I wish I could make that a global default.
                RenderOptions.SetBitmapScalingMode(newImage, RenderOptions.GetBitmapScalingMode(source));
                       
                // These images will float on top of the rest of the window.
                Panel.SetZIndex(newImage, 10);
                newImage.ClipToBounds = false;
                Grid.SetColumnSpan(newImage, int.MaxValue);
                Grid.SetRowSpan(newImage, int.MaxValue);

                // Create a WPF animation.
                Storyboard storyboard = new Storyboard();

                // Save this in case we have to terminate this animation early.
                _sprayItems.Add(newImage);

                // Initially each copy will be the same size as the original.
                newImage.Width = source.ActualWidth;
                newImage.Height = source.ActualHeight;

                // Initially the copy will be in the same position as the original.
                newImage.HorizontalAlignment = HorizontalAlignment.Left;
                newImage.VerticalAlignment = VerticalAlignment.Top;
                Thickness initialMargin = new Thickness(initialLocation.X, initialLocation.Y, 0, 0);
                newImage.Margin = initialMargin;
                AnimationParent.Children.Add(newImage);

                // Move the copy from the location of the original to the random location we
                // selected above.
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.From = initialMargin;
                thicknessAnimation.To = new Thickness(finalX, finalY, 0, 0);
                thicknessAnimation.Duration = duration;
                thicknessAnimation.BeginTime = beginTime;
                storyboard.Children.Add(thicknessAnimation);
                Storyboard.SetTarget(thicknessAnimation, newImage);
                Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Image.MarginProperty));

                // The copy will grow up to 4x as big as the original.
                // The copy will maintain its aspect ratio.
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

                // The copies will fade as they move and grow.  At the end they will be completely
                // transparent.
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1;
                opacityAnimation.To = 0;
                opacityAnimation.Duration = duration;
                opacityAnimation.BeginTime = beginTime;
                storyboard.Children.Add(opacityAnimation);
                Storyboard.SetTarget(opacityAnimation, newImage);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Image.OpacityProperty));

                // Completely remove the copy once the animation is done.  The image was already
                // transparent, but it might capture mouse clicks.  Someone will think they are clicking
                // on a button, when they are actually clicking on an invisiable picture on top of the
                // button!
                storyboard.Completed += delegate { AnimationParent.Children.Remove(newImage); };

                // Start the countdown for this animation.
                storyboard.Begin();
            }
        }

        /// <summary>
        /// We only want to do an animation when the program state changes.
        /// </summary>
        private ProgramState _previousProgramState = ProgramState.Running;

        /// <summary>
        /// Find the current state of the game and update the GUI accordingly.
        /// </summary>
        private void RefreshNow()
        {
            ProgramState totalState = ProgramState.Won;
            foreach (ProgramStateProvider provider in _providers)
            {   // Some games include multiple maps.  The program state is based on all of the maps.
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

        /// <summary>
        /// You can't call Spray() when you want to.  You have to wait for other events.  Spray
        /// starts it's animation by copying the current size and location of the image.  But
        /// if we just created the image, that information might not be available yet.  So record
        /// our desire, and let a callback take care of it later.
        /// </summary>
        private bool sprayRequired = false;

        /// <summary>
        /// Each map can have its own state.  We compute the program's state by looking at each
        /// of these and summarizing.
        /// </summary>
        private readonly HashSet<ProgramStateProvider> _providers = new HashSet<ProgramStateProvider>();

        /// <summary>
        /// Multiple components all contribute to our total state.  They might notify us of changes
        /// in quick succession.  This will merge all of the requests that come in at approximately
        /// the same time.
        /// </summary>
        private readonly WakeMeSoon _wakeMeSoon = new WakeMeSoon();

        public void AddProvider(ProgramStateProvider provider)
        {
            _providers.Add(provider);
            provider.ProgramStateChanged += delegate { _wakeMeSoon.RequestWakeUp(); };
            _wakeMeSoon.RequestWakeUp();
        }

        /// <summary>
        /// We finally know the starting place for the animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
