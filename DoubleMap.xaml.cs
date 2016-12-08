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
    /// This is the top level control that lets you control two different maps with the
    /// same program.
    /// </summary>
    public partial class DoubleMap : UserControl
    {
        public DoubleMap()
        {
            InitializeComponent();
            statusPanel.AddProvider(worldView1);
            statusPanel.AddProvider(worldView2);
            editor.ElementToClose = this;
            statusPanel.AnimationParent = mainGrid;
        }

        public DoubleMap(string mapStringTop, string mapStringBottom) : this()
        {
            worldView1.MapString = mapStringTop;
            worldView2.MapString = mapStringBottom;
        }

        private void editor_ProgramChanged(Editor obj)
        {
            worldView1.Program = editor.Program;
            worldView2.Program = editor.Program;
        }
    }
}
