using System.Linq;
using System.Windows;

namespace MozaikaApp
{
    public partial class MaterialSuppliersWindow : Window
    {
        private MozaikaEntities db = new MozaikaEntities();
        private int materialId;

        public MaterialSuppliersWindow(int materialId)
        {
            InitializeComponent();
            this.materialId = materialId;
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            // Пример: таблицы suppliers и materialSuppliers должны существовать
            var suppliers = from ms in db.material_supplier
                            join s in db.supplier on ms.supplierId equals s.id
                            where ms.materialId == materialId
                            select new
                            {
                                Name = s.supplierName,
                                Rating = s.rating,
                                StartDate = s.startDealingDate
                            };

            listSuppliers.ItemsSource = suppliers.ToList();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenMaterialSuppliers_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно калькулятора и передаём materialId
            var calcWin = new ProductionCalculatorPanel(this.materialId);
            calcWin.Owner = this; // чтобы окно знало родителя
            calcWin.ShowDialog();
        }

    }
}
