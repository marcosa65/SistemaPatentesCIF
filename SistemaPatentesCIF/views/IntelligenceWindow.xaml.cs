using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaPatentesCIF.views
{
    /// <summary>
    /// Lógica de interacción para IntelligenceWindow.xaml
    /// </summary>
    public partial class IntelligenceWindow : Window
    {
        public IntelligenceWindow()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
