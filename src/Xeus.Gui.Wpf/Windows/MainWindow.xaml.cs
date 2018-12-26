using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lxna.Gui.Wpf.Extensions;

namespace Lxna.Gui.Wpf.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = this.ListViewMenu.SelectedIndex;
            this.MoveCursorMenu(index);
        }

        private void MoveCursorMenu(int index)
        {
            try
            {
                this.TransitioningContentSlide.OnApplyTemplate();
                this.GridCursor.Margin = new Thickness(0, this.GridCursor.ActualHeight * index, 0, 0);
            }
            catch (Exception)
            {
            }
        }
    }
}
