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
using SistemaPatentesCIF.views;

namespace SistemaPatentesCIF
{
    /// <summary>
    /// Lógica de interacción para InitialWindow.xaml
    /// </summary>
    public partial class InitialWindow : Window
    {
        public InitialWindow()
        {
            InitializeComponent();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow historyWindow = new HistoryWindow();
            historyWindow.Show();
        }

        private void ALPRDetectionButton_Click(object sender, RoutedEventArgs e)
        {
            ALPRDetectionWindow alprDetectionWindow = new ALPRDetectionWindow();
            alprDetectionWindow.Show();
        }
        private void IntelligenceButton_Click(object sender, RoutedEventArgs e)
        {
            IntelligenceWindow intelligenceWindow = new IntelligenceWindow();
            intelligenceWindow.Show();
        }
    }
}
