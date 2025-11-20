using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MozaikaApp
{
    public partial class MainWindow : Window
    {
        MozaikaEntities db = new MozaikaEntities();

        public MainWindow()
        {
            InitializeComponent();
            
            listPartner.MouseDoubleClick += ListPartner_MouseDoubleClick;
        }

        private void showOrders_Click(object sender, RoutedEventArgs e)
        {
            listPartner.Items.Clear();
            listPartnerCanvas.Visibility = Visibility.Visible;

            var orders = db.partner_order.ToList();

            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                var partner = order.partner;

                Grid gr = new Grid();
                TextBlock typeAndName = new TextBlock();
                TextBlock totalAmount = new TextBlock();
                TextBlock address = new TextBlock();
                TextBlock phone = new TextBlock();
                TextBlock rating = new TextBlock();

                Border border = new Border
                {
                    Width = 665,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Margin = new Thickness(0, 10, 0, 10),
                    Padding = new Thickness(10),
                    Tag = order.id,
                    Background = Brushes.White
                };

                Button btnOrderItems = new Button
                {
                    Content = "Продукция в заявке",
                    Tag = order.id,
                    Margin = new Thickness(400, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(84, 111, 148)),
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Comic Sans MS"),
                    FontSize = 14,
                    Width = 150,
                    Height = 30
                };

                btnOrderItems.Click += BtnOrderItems_Click;

                gr.Width = 645;

                // Верхняя строка
                typeAndName.Text = $"{partner?.type ?? "Тип не указан"} | {partner?.name ?? "Наименование не указано"}";
                typeAndName.FontSize = 16;
                typeAndName.VerticalAlignment = VerticalAlignment.Center;

                // Стоимость справа
                totalAmount.Text = $"{order.total_amount:F2} руб.";
                totalAmount.FontSize = 16;
                totalAmount.VerticalAlignment = VerticalAlignment.Center;
                totalAmount.HorizontalAlignment = HorizontalAlignment.Right;                                                       

                // Детальная информация
                address.Text = $"Юридический адрес: {partner?.sellingPlaces ?? "Не указан"}";
                address.FontSize = 14;
                address.Margin = new Thickness(0, 5, 0, 0);
                address.TextWrapping = TextWrapping.Wrap;

                phone.Text = $"{partner?.telNumber ?? "Телефон не указан"}";
                phone.FontSize = 14;
                phone.Margin = new Thickness(0, 5, 0, 0);

                rating.Text = $"Рейтинг: {partner?.rating?.ToString() ?? "0"}";
                rating.FontSize = 14;
                rating.Margin = new Thickness(0, 5, 0, 0);

                // Настройка Grid
                for (int r = 0; r < 5; r++)
                    gr.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                gr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gr.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); 

                // Размещение элементов
                Grid.SetColumn(typeAndName, 0); Grid.SetRow(typeAndName, 0);
                Grid.SetColumn(totalAmount, 1); Grid.SetRow(totalAmount, 0); 

                Grid.SetColumn(address, 0); Grid.SetRow(address, 1); Grid.SetColumnSpan(address, 2);
                Grid.SetColumn(phone, 0); Grid.SetRow(phone, 2); Grid.SetColumnSpan(phone, 2);
                Grid.SetColumn(rating, 0); Grid.SetRow(rating, 3); Grid.SetColumnSpan(rating, 2);
                Grid.SetColumn(btnOrderItems, 0); Grid.SetRow(btnOrderItems, 4); Grid.SetColumnSpan(btnOrderItems, 2);

                gr.Children.Add(typeAndName);
                gr.Children.Add(totalAmount);
                gr.Children.Add(address);
                gr.Children.Add(phone);
                gr.Children.Add(rating);
                gr.Children.Add(btnOrderItems);

                border.Child = gr;
                listPartner.Items.Add(border);
            }
        }

        private void BtnOrderItems_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                OrderItemsWindow win = new OrderItemsWindow(orderId);
                win.ShowDialog();
            }
        }

        private void ListPartner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listPartner.SelectedItem is Border border && border.Tag is int orderId)
            {
                EditOrderWindow editWindow = new EditOrderWindow(orderId);
                editWindow.Show();
                this.Close();
            }
        }

        private void addOrder_Click(object sender, RoutedEventArgs e)
        {
            EditOrderWindow editWindow = new EditOrderWindow();
            editWindow.Show();
            this.Close();
        }
        
        private void LoadOrders()
        {
            showOrders_Click(null, null);
        }
    }
}
