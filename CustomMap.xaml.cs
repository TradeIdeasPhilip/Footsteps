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
    /// This is the top level control that lets the end user change the map while the program
    /// is running.
    /// </summary>
    public partial class CustomMap : UserControl
    {
        public CustomMap()
        {
            InitializeComponent();
            editor.ElementToClose = this;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            worldView.MapString = textBox.Text;
        }

        private void editor_ProgramChanged(Editor obj)
        {
            worldView.Program = editor.Program;
        }
    }
}
