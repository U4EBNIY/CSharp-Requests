using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Validation_LIB;

namespace MozaikaApp
{
    public partial class AddMaterialPanel : Window
    {
        private MozaikaEntities db = new MozaikaEntities();
        private int? editingMaterialId = null; // если null — добавление, иначе - редактирование

        public AddMaterialPanel()
        {
            InitializeComponent();
            LoadMaterialTypes();
        }

        // Конструктор для редактирования существующего материала
        public AddMaterialPanel(int materialId) : this()
        {
            editingMaterialId = materialId;
            LoadMaterialData(materialId);
        }

        private void LoadMaterialTypes()
        {
            try
            {
                var types = db.materialTypes.ToList();
                cmbType.ItemsSource = types;
                cmbType.DisplayMemberPath = "name";
                cmbType.SelectedValuePath = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMaterialData(int materialId)
        {
            var mat = db.material.Find(materialId);
            if (mat == null) return;

            txtName.Text = mat.name;
            txtPrice.Text = mat.pricePerOne.ToString();
            txtMeasurement.Text = mat.measurement;
            cmbType.SelectedValue = mat.materialTypesId;

            var stash = db.stash_material.FirstOrDefault(sm => sm.materialId == materialId);
            if (stash != null)
            {
                txtStashAmount.Text = stash.amount.ToString();
                txtMinStashAmount.Text = stash.minStashAmount.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string price = txtPrice.Text.Trim();
            string measurement = txtMeasurement.Text.Trim();
            string stashAmountStr = txtStashAmount.Text.Trim();
            string minStashStr = txtMinStashAmount.Text.Trim();

            if (cmbType.SelectedItem == null || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(price) || string.IsNullOrWhiteSpace(measurement) ||
                string.IsNullOrWhiteSpace(stashAmountStr) || string.IsNullOrWhiteSpace(minStashStr))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(stashAmountStr, out int stash) || !int.TryParse(minStashStr, out int minStash))
            {
                MessageBox.Show("Введите корректные числа для количества", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(price, out decimal cost))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedType = cmbType.SelectedItem as materialTypes;
            int typeId = selectedType.id;

            try
            {
                material mat;
                stash_material stashMat;

                if (editingMaterialId.HasValue)
                {
                    // редактируем
                    mat = db.material.Find(editingMaterialId.Value);
                    mat.name = name;
                    mat.pricePerOne = (int)cost;
                    mat.measurement = measurement;
                    mat.materialTypesId = typeId;

                    stashMat = db.stash_material.FirstOrDefault(sm => sm.materialId == editingMaterialId.Value);
                    if (stashMat != null)
                    {
                        stashMat.amount = stash;
                        stashMat.minStashAmount = minStash;
                    }
                }
                else
                {
                    // добавляем новый
                    mat = new material
                    {
                        name = name,
                        pricePerOne = (int)cost,
                        measurement = measurement,
                        materialTypesId = typeId,
                        image = "noImage.png"
                    };
                    db.material.Add(mat);
                    db.SaveChanges(); // сохраняем, чтобы получить ID

                    stashMat = new stash_material
                    {
                        materialId = mat.id,
                        amount = stash,
                        minStashAmount = minStash
                    };
                    db.stash_material.Add(stashMat);
                }

                db.SaveChanges();
                MessageBox.Show("Материал успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
