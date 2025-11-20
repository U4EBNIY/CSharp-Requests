using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MozaikaApp
{
    public partial class EditOrderWindow : Window
    {
        private MozaikaEntities db = new MozaikaEntities();
        private int? editingOrderId = null;
        private List<OrderItemViewModel> orderItems = new List<OrderItemViewModel>();

        public class OrderItemViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public EditOrderWindow()
        {
            InitializeComponent();
            db = new MozaikaEntities();
            LoadProducts();
        }

        public EditOrderWindow(int orderId) : this()
        {
            editingOrderId = orderId;
            LoadOrderData(orderId);
        }

        protected override void OnClosed(EventArgs e)
        {
            db?.Dispose();
            base.OnClosed(e);
        }

        private void LoadProducts()
        {
            try
            {
                var products = db.product.ToList(); 
                cmbProduct.ItemsSource = products;
                cmbProduct.DisplayMemberPath = "name";
                cmbProduct.SelectedValuePath = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продукции: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderData(int orderId)
        {
            try
            {
                var order = db.partner_order.Find(orderId);
                if (order == null) return;

                var partner = order.partner;
                if (partner != null)
                {
                    // Заполняем поля партнера
                    cmbPartnerType.Text = partner.type;
                    txtName.Text = partner.name;
                    txtDirector.Text = partner.directorFIO;
                    txtAddress.Text = partner.sellingPlaces;
                    txtRating.Text = partner.rating?.ToString() ?? "0";
                    txtPhone.Text = partner.telNumber;
                    txtEmail.Text = partner.email;
                }

                // Загружаем позиции заявки
                var items = db.order_item.Where(oi => oi.order_id == orderId).ToList();
                foreach (var item in items)
                {
                    var product = item.product;
                    orderItems.Add(new OrderItemViewModel
                    {
                        ProductId = item.product_id,
                        ProductName = product?.name ?? "Неизвестно",
                        Quantity = item.quantity,
                        UnitPrice = item.unit_price,
                        TotalPrice = item.total_price
                    });
                }

                RefreshOrderItemsList();
                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedProduct = cmbProduct.SelectedItem as product;
            if (selectedProduct == null) return;

            decimal unitPrice = selectedProduct.minCostForPartner ?? 0;
            decimal totalPrice = unitPrice * quantity;

            var existingItem = orderItems.FirstOrDefault(oi => oi.ProductId == selectedProduct.id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                orderItems.Add(new OrderItemViewModel
                {
                    ProductId = selectedProduct.id,
                    ProductName = selectedProduct.name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            RefreshOrderItemsList();
            UpdateTotalAmount();
            
            cmbProduct.SelectedIndex = -1;
            txtQuantity.Text = "1";
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                var itemToRemove = orderItems.FirstOrDefault(oi => oi.ProductId == productId);
                if (itemToRemove != null)
                {
                    orderItems.Remove(itemToRemove);
                    RefreshOrderItemsList();
                    UpdateTotalAmount();
                }
            }
        }

        private void RefreshOrderItemsList()
        {
            lstOrderItems.ItemsSource = null;
            lstOrderItems.ItemsSource = orderItems;
        }

        private void UpdateTotalAmount()
        {
            decimal total = orderItems.Sum(oi => oi.TotalPrice);
            txtOrderTotal.Text = $"Итоговая стоимость: {total:F2} руб.";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных партнера
            if (string.IsNullOrWhiteSpace(cmbPartnerType.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtDirector.Text))
            {
                MessageBox.Show("Заполните обязательные поля: тип партнера, наименование и ФИО директора",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtRating.Text, out int rating) || rating < 0)
            {
                MessageBox.Show("Рейтинг должен быть целым неотрицательным числом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (orderItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну позицию продукции в заявку",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new MozaikaEntities())
                {
                    partner partnerEntity;
                    partner_order orderEntity;

                    if (editingOrderId.HasValue)
                    {
                        // Редактирование существующей заявки
                        orderEntity = db.partner_order.Find(editingOrderId.Value);
                        if (orderEntity == null)
                        {
                            MessageBox.Show("Заявка не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        partnerEntity = orderEntity.partner;
                        if (partnerEntity == null)
                        {
                            MessageBox.Show("Партнер не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Обновляем партнера
                        partnerEntity.type = cmbPartnerType.Text;
                        partnerEntity.name = txtName.Text;
                        partnerEntity.directorFIO = txtDirector.Text;
                        partnerEntity.sellingPlaces = txtAddress.Text ?? "";
                        partnerEntity.rating = rating;
                        partnerEntity.telNumber = txtPhone.Text ?? "";
                        partnerEntity.email = txtEmail.Text ?? "";
                        partnerEntity.organizationName = txtName.Text;

                        // Удаляем старые позиции заявки
                        var oldItems = db.order_item.Where(oi => oi.order_id == editingOrderId.Value).ToList();
                        db.order_item.RemoveRange(oldItems);
                    }
                    else
                    {
                        // Создание нового партнера
                        partnerEntity = new partner
                        {
                            type = cmbPartnerType.Text,
                            name = txtName.Text,
                            organizationName = txtName.Text,
                            directorFIO = txtDirector.Text,
                            sellingPlaces = txtAddress.Text ?? "",
                            rating = rating,
                            telNumber = txtPhone.Text ?? "",
                            email = txtEmail.Text ?? "",
                            inn = "0000000000",
                            logo = null 
                        };
                        db.partner.Add(partnerEntity);
                        db.SaveChanges(); 

                        // Создание новой заявки
                        orderEntity = new partner_order
                        {
                            partner_id = partnerEntity.id,
                            total_amount = orderItems.Sum(oi => oi.TotalPrice),
                            status = "Новая",
                            order_date = DateTime.Now,
                            created_date = DateTime.Now
                        };
                        db.partner_order.Add(orderEntity);
                    }

                    db.SaveChanges();

                    // Добавляем позиции заявки
                    foreach (var item in orderItems)
                    {
                        var orderItem = new order_item
                        {
                            order_id = orderEntity.id,
                            product_id = item.ProductId,
                            quantity = item.Quantity,
                            unit_price = item.UnitPrice,
                            total_price = item.TotalPrice
                        };
                        db.order_item.Add(orderItem);
                    }

                    // Обновляем общую стоимость заявки
                    orderEntity.total_amount = orderItems.Sum(oi => oi.TotalPrice);

                    db.SaveChanges();

                    MessageBox.Show("Заявка успешно сохранена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Обработка ошибок
                var errorMessages = new List<string>();
                foreach (var validationResult in ex.EntityValidationErrors)
                {
                    foreach (var error in validationResult.ValidationErrors)
                    {
                        errorMessages.Add($"{error.PropertyName}: {error.ErrorMessage}");
                    }
                }
                MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Обработка ошибок
                string errorMessage = $"Ошибка обновления БД: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Общая обработка ошибок
                string errorMessage = $"Ошибка при сохранении заявки: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email) && !Validation_LIB.Validator.checkMail(email))
            {
                txtEmail.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
            }
            else
            {
                txtEmail.Background = new SolidColorBrush(Color.FromRgb(171, 207, 206));
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            int? materialId = null;
            if (cmbProduct.SelectedItem is product selectedProduct)
            {
                materialId = selectedProduct.materialId;
            }
            ProductionCalculatorPanel calculator = new ProductionCalculatorPanel(materialId ?? 0);
            calculator.ShowDialog();
        }
    }
}
