using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SistemaPatentesCIF.views
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
        private MessageBoxButton _buttons;

        private CustomMessageBox(
            string message,
            string title,
            MessageBoxButton buttons,
            MessageBoxImage icon
        )
        {
            InitializeComponent();

            TxtTitle.Text = title.ToUpperInvariant();
            TxtMessage.Text = message;
            _buttons = buttons;

            ConfigureButtons(buttons);
            ConfigureIcon(icon);

            BtnPrimary.IsDefault = true;
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    BtnClose_Click(this, new RoutedEventArgs());
                }
            };

        }

        public static MessageBoxResult Show(
            string message,
            string title = "ALPR Control Center",
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.Information
        )
        {
            var dialog = new CustomMessageBox(message, title, buttons, icon);

            var activeWindow = Application
                .Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive && w != dialog);

            if (activeWindow != null)
            {
                dialog.Owner = activeWindow;
                activeWindow.Opacity = 0.65; // Atenúa la ventana de fondo
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            try
            {
                dialog.ShowDialog();
            }
            finally
            {
                if (activeWindow != null)
                {
                    activeWindow.Opacity = 1.0; // Restaura el brillo al cerrar
                }
            }

            return dialog.Result;
        }

        private void ConfigureButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OKCancel:
                    BtnPrimary.Content = "Aceptar";
                    BtnSecondary.Content = "Cancelar";
                    BtnSecondary.Visibility = Visibility.Visible;
                    break;

                case MessageBoxButton.YesNo:
                    BtnPrimary.Content = "Sí";
                    BtnSecondary.Content = "No";
                    BtnSecondary.Visibility = Visibility.Visible;
                    break;

                default:
                    BtnPrimary.Content = "Aceptar";
                    BtnSecondary.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void ConfigureIcon(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Error:
                    IconPath.Data = Geometry.Parse(
                        "M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"
                    );
                    IconPath.Fill = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EF4444")
                    );
                    break;

                case MessageBoxImage.Warning:
                    IconPath.Data = Geometry.Parse(
                        "M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z"
                    );
                    IconPath.Fill = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F59E0B")
                    );
                    break;

                case MessageBoxImage.Question:
                    IconPath.Data = Geometry.Parse(
                        "M15.07,11.25L14.17,12.17C13.45,12.89 13,13.5 13,15H11V14.5C11,13.39 11.45,12.39 12.17,11.67L13.41,10.41C13.78,10.05 14,9.55 14,9C14,7.89 13.1,7 12,7A2,2 0 0,0 10,9H8A4,4 0 0,1 12,5A4,4 0 0,1 16,9C16,9.88 15.64,10.67 15.07,11.25M13,19H11V17H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z"
                    );
                    break;

                default:
                    // Information usa el vector por defecto y el color AccentAmberBrush del tema activo
                    break;
            }
        }

        private void BtnPrimary_Click(object sender, RoutedEventArgs e)
        {
            Result =
                _buttons == MessageBoxButton.YesNo ? MessageBoxResult.Yes : MessageBoxResult.OK;
            Close();
        }

        private void BtnSecondary_Click(object sender, RoutedEventArgs e)
        {
            Result =
                _buttons == MessageBoxButton.YesNo ? MessageBoxResult.No : MessageBoxResult.Cancel;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Result =
                _buttons == MessageBoxButton.YesNo ? MessageBoxResult.No : MessageBoxResult.Cancel;
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
