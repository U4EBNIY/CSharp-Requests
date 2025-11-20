using System;
using System.Linq;

namespace MozaikaApp
{
    public static class ProductionCalculator
    {
        public static int CalculateProduction(
            int productTypeId,
            int materialTypeId,
            int usedRawAmount,
            double param1,
            double param2)
        {
            // 1. Проверяем входные данные
            if (usedRawAmount <= 0 || param1 <= 0 || param2 <= 0)
                return -1;

            using (var db = new MozaikaEntities())
            {
                // 2. Берём тип продукции
                var productType = db.productType.FirstOrDefault(pt => pt.id == productTypeId);
                if (productType == null) return -1;

                // 3. Берём тип материала
                var materialType = db.materialTypes.FirstOrDefault(mt => mt.id == materialTypeId);
                if (materialType == null) return -1;

                // 4. Проверяем, что коэффициенты есть
                if (!productType.coefficient.HasValue || !materialType.lossPercent.HasValue)
                    return -1;

                double coefficient = productType.coefficient.Value; // вещественное
                double lossPercent = materialType.lossPercent.Value; // процент потерь сырья

                if (coefficient <= 0 || lossPercent < 0)
                    return -1;

                // 5. Считаем сырьё на единицу продукции
                double rawPerUnit = param1 * param2 * coefficient;

                // 6. С учётом потерь
                double rawWithLoss = rawPerUnit * (1 + lossPercent / 100);

                // 7. Сколько продукции можно произвести
                double produced = usedRawAmount / rawWithLoss;

                // 8. Округляем вниз до целого
                return (int)Math.Floor(produced);
            }
        }
    }
}
