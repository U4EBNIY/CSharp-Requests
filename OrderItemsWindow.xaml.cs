using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MozaikaApp
{
    public partial class OrderItemsWindow : Window
    {
        private int orderId;
        private MozaikaEntities db = new MozaikaEntities();

        public OrderItemsWindow(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            LoadOrderItems();
        }

        private void LoadOrderItems()
        {
            try
            {
                var order = db.partner_order.Find(orderId);
                var partner = order?.partner;

                txtTitle.Text = $"Продукция в заявке №{orderId} - {partner?.name ?? "Неизвестный партнер"}";

                var orderItems = db.order_item.Where(oi => oi.order_id == orderId).ToList();
                decimal totalAmount = 0;

                foreach (var item in orderItems)
                {
                    var product = item.product;

                    Grid gr = new Grid();
                    gr.Margin = new Thickness(0, 5, 0, 5);

                    // Настройка колонок
                    gr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    gr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    TextBlock productName = new TextBlock
                    {
                        Text = product?.name ?? "Неизвестная продукция",
                        FontSize = 14,
                        FontFamily = new FontFamily("Comic Sans MS"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5)
                    };

                    TextBlock quantity = new TextBlock
                    {
                        Text = $"× {item.quantity}",
                        FontSize = 14,
                        FontFamily = new FontFamily("Comic Sans MS"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(5)
                    };

                    TextBlock unitPrice = new TextBlock
                    {
                        Text = $"{item.unit_price:F2} руб./шт",
                        FontSize = 14,
                        FontFamily = new FontFamily("Comic Sans MS"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(5)
                    };

                    TextBlock totalPrice = new TextBlock
                    {
                        Text = $"{item.total_price:F2} руб.",
                        FontSize = 14,
                        FontFamily = new FontFamily("Comic Sans MS"),
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Color.FromRgb(84, 111, 148)) // ИСПРАВЛЕНО
                    };

                    Grid.SetColumn(productName, 0);
                    Grid.SetColumn(quantity, 1);
                    Grid.SetColumn(unitPrice, 2);
                    Grid.SetColumn(totalPrice, 3);

                    gr.Children.Add(productName);
                    gr.Children.Add(quantity);
                    gr.Children.Add(unitPrice);
                    gr.Children.Add(totalPrice);

                    Border border = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Gray,
                        Background = Brushes.White,
                        Padding = new Thickness(5),
                        Margin = new Thickness(0, 2, 0, 2),
                        Child = gr
                    };

                    ItemsStackPanel.Children.Add(border);
                    totalAmount += item.total_price;
                }

                txtTotal.Text = $"Итоговая стоимость заявки: {totalAmount:F2} руб.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продукции: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}