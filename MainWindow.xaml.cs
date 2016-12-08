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

namespace Footsteps
{
    /// <summary>
    /// This shows a simple level.  It has one WorldView, one Editor, and one StatusPanel.
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow(string mapString = null)
        {
            InitializeComponent();
            if (null != mapString)
                worldView1.MapString = mapString;
            statusPanel.AddProvider(worldView1);
            editor1.ElementToClose = this;
            statusPanel.AnimationParent = mainGrid;
        }

        private void editor1_ProgramChanged(Editor obj)
        {
            worldView1.Program = editor1.Program;
        }
    }
}
