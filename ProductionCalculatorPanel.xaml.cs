using System;
using System.Linq;
using System.Windows;

namespace MozaikaApp
{
    public partial class ProductionCalculatorPanel : Window
    {
        private int materialId;
        private MozaikaEntities db = new MozaikaEntities();

        public ProductionCalculatorPanel(int materialId)
        {
            InitializeComponent();
            this.materialId = materialId;
            LoadProductTypes();
            LoadMaterialTypes();
        }

        private void LoadProductTypes()
        {
            // Загружаем список типов продукции из БД
            cmbProductType.ItemsSource = db.productType.ToList();
        }

        private void LoadMaterialTypes()
        {
            // Загружаем список типов материалов — фильтруем по materialId, если нужно
            // допустим, materialTypeId связан с material
            var mat = db.material.Find(materialId);
            if (mat != null)
            {
                // Если материал связан с типом, можно выбрать только один
                var typeList = db.materialTypes
                    .Where(mt => mt.id == mat.materialTypesId)
                    .ToList();
                cmbMaterialType.ItemsSource = typeList;
                if (typeList.Count == 1)
                    cmbMaterialType.SelectedIndex = 0;
            }
            else
            {
                cmbMaterialType.ItemsSource = db.materialTypes.ToList();
            }
        }

        private void CalcProduction_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбора в комбобоксах
            if (cmbProductType.SelectedItem == null || cmbMaterialType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип продукции и материала");
                return;
            }

            // Пытаемся преобразовать в int и записываем в переменную usedRaw
            if (!int.TryParse(txtUsedRaw.Text, out int usedRaw))
            {
                MessageBox.Show("Введите корректное количество сырья");
                return;
            }

            // Пытаемся параметры в double и записываем в переменные params
            if (!double.TryParse(txtParam1.Text, out double param1) ||
                !double.TryParse(txtParam2.Text, out double param2))
            {
                MessageBox.Show("Введите корректные параметры");
                return;
            }

            int productTypeId = (int)cmbProductType.SelectedValue;
            int materialTypeId = (int)cmbMaterialType.SelectedValue;

            int calc = ProductionCalculator.CalculateProduction(productTypeId, materialTypeId, usedRaw, param1, param2);

            if (calc == -1)
                MessageBox.Show("Ошибка в данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show($"Можно произвести {calc} единиц продукции", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
