using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OfficeOpenXml;

namespace WpfLb1
{
    public partial class ShiftTransferTab : UserControl
    {
        public ObservableCollection<ShiftTransferRow> ShiftTransferData { get; set; }


        public ShiftTransferTab()
        {
            InitializeComponent();
            ShiftTransferData = new ObservableCollection<ShiftTransferRow>();
            ShiftTransferDataGrid.ItemsSource = ShiftTransferData;
            ExcelPackage.License.SetNonCommercialPersonal("TudaSuda");
        }

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var table = new ObservableCollection<ShiftTransferRow>();

                if (filePath.EndsWith(".txt"))
                {
                    // Обработка текстового файла
                    var fileContent = File.ReadAllLines(filePath);
                    foreach (var line in fileContent)
                    {
                        var columns = line.Split('\t'); // Разделитель - табуляция

                        // Гарантируем, что будет ровно 4 столбца
                        table.Add(new ShiftTransferRow
                        {
                            Column1 = columns.ElementAtOrDefault(0)?.Trim() ?? string.Empty,
                            Column2 = columns.ElementAtOrDefault(1)?.Trim() ?? string.Empty,
                            Column3 = columns.ElementAtOrDefault(2)?.Trim() ?? string.Empty,
                            Column4 = columns.ElementAtOrDefault(3)?.Trim() ?? string.Empty
                        });
                    }
                }
                else if (filePath.EndsWith(".xlsx"))
                {
                    // Обработка Excel-файла
                    using var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets[0]; // Первый лист

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // Пропускаем заголовок
                    {
                        table.Add(new ShiftTransferRow
                        {
                            Column1 = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty,
                            Column2 = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty,
                            Column3 = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty,
                            Column4 = worksheet.Cells[row, 4].Text?.Trim() ?? string.Empty
                        });
                    }
                }

                // Устанавливаем источник данных для DataGrid
                ShiftTransferDataGrid.ItemsSource = table;

                MessageBox.Show("Файл успешно загружен и обработан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CopyColumn1_Click(object sender, RoutedEventArgs e) => CopyColumn(0);
        private void CopyColumn2_Click(object sender, RoutedEventArgs e) => CopyColumn(1);
        private void CopyColumn3_Click(object sender, RoutedEventArgs e) => CopyColumn(2);
        private void CopyColumn4_Click(object sender, RoutedEventArgs e) => CopyColumn(3);

        private void CopyColumn(int columnIndex)
        {
            if (ShiftTransferDataGrid.ItemsSource is ObservableCollection<ShiftTransferRow> table)
            {
                var columnData = table.Select(row => columnIndex switch
                {
                    0 => row.Column1,
                    1 => row.Column2,
                    2 => row.Column3,
                    3 => row.Column4,
                    _ => null
                }).Where(data => data != null);

                Clipboard.SetText(string.Join(Environment.NewLine, columnData));
            }
        }

        private void ExportToExcel(ObservableCollection<ShiftTransferRow> table)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("ShiftTransfer");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Column1";
            worksheet.Cells[1, 2].Value = "Column2";
            worksheet.Cells[1, 3].Value = "Column3";
            worksheet.Cells[1, 4].Value = "Column4";

            // Данные
            for (int i = 0; i < table.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = table[i].Column1;
                worksheet.Cells[i + 2, 2].Value = table[i].Column2;
                worksheet.Cells[i + 2, 3].Value = table[i].Column3;
                worksheet.Cells[i + 2, 4].Value = table[i].Column4;
            }

            // Сохранение файла
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "ShiftTransfer.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                MessageBox.Show("Файл успешно сохранен!");
            }
        }
        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (ShiftTransferDataGrid.ItemsSource is ObservableCollection<ShiftTransferRow> table)
            {
                ExportToExcel(table);
            }
            else
            {
                MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ShiftTransferRow
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
    }
}