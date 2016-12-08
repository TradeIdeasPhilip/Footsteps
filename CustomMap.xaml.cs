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
    /// Interaction logic for CustomMap.xaml
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
