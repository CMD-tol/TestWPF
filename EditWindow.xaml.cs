using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestWPF
{
    public partial class EditWindow : Window
    {
        private int _productId;  // ID редактируемого товара
        private string _selectedImagePath;
        private string connectionString = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";

        // Конструктор с передачей ID товара
        public EditWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductData();  // Загружаем данные товара
        }

        // Загрузка данных товара из БД
        private void LoadProductData()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // ✅ Явно перечисляем все нужные колонки
                string query = @"
            SELECT 
                p.product_id,
                p.name,
                p.description,
                p.selling_price,
                p.discount_percent,
                p.stock_quantity,
                p.image_path,
                c.name AS category_name,
                m.name AS manufacturer_name,
                s.name AS supplier_name,
                u.name AS unit_name
            FROM Products p
            LEFT JOIN Categories c ON p.category_id = c.category_id
            LEFT JOIN Manufacturers m ON p.manufacturer_id = m.manufacturer_id
            LEFT JOIN Suppliers s ON p.supplier_id = s.supplier_id
            LEFT JOIN Units u ON p.unit_id = u.unit_id
            WHERE p.product_id = @id";

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", _productId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Теперь product_id точно есть
                            NameProduct.Text = reader.GetString(reader.GetOrdinal("name"));
                            NameCategory.Text = reader.IsDBNull(reader.GetOrdinal("category_name")) ? "" : reader.GetString(reader.GetOrdinal("category_name"));
                            Deskription.Text = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description"));
                            Manufacturer.Text = reader.IsDBNull(reader.GetOrdinal("manufacturer_name")) ? "" : reader.GetString(reader.GetOrdinal("manufacturer_name"));
                            provider.Text = reader.IsDBNull(reader.GetOrdinal("supplier_name")) ? "" : reader.GetString(reader.GetOrdinal("supplier_name"));
                            Price.Text = reader.GetDecimal(reader.GetOrdinal("selling_price")).ToString();
                            Izmerenya.Text = reader.IsDBNull(reader.GetOrdinal("unit_name")) ? "шт" : reader.GetString(reader.GetOrdinal("unit_name"));
                            QuantityInstock.Text = reader.GetInt32(reader.GetOrdinal("stock_quantity")).ToString();
                            Descount.Text = reader.GetDecimal(reader.GetOrdinal("discount_percent")).ToString();

                            // Загружаем изображение
                            string imagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? "" : reader.GetString(reader.GetOrdinal("image_path"));

                            if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                            {
                                ProductImage.Source = new BitmapImage(new Uri(imagePath));
                                _selectedImagePath = imagePath;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Товар с ID {_productId} не найден");
                        }
                    }
                }
            }
        }
        // Загрузка нового изображения
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                _selectedImagePath = openDialog.FileName;
                ProductImage.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        // Сохранение изменений
        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(NameProduct.Text))
                {
                    MessageBox.Show("Введите наименование товара");
                    NameProduct.Focus();
                    return;
                }

                if (!decimal.TryParse(Price.Text, out decimal priceValue))
                {
                    MessageBox.Show("Цена должна быть числом");
                    Price.Focus();
                    return;
                }

                if (!int.TryParse(QuantityInstock.Text, out int stockValue))
                {
                    stockValue = 0;
                }

                if (!decimal.TryParse(Descount.Text, out decimal discountValue))
                {
                    discountValue = 0;
                }

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Обновляем товар
                    string updateQuery = @"
                        UPDATE Products 
                        SET name = @name,
                            description = @description,
                            selling_price = @price,
                            discount_percent = @discount,
                            stock_quantity = @stock,
                            image_path = @image
                        WHERE product_id = @id";

                    using (var cmd = new SqliteCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", _productId);
                        cmd.Parameters.AddWithValue("@name", NameProduct.Text);
                        cmd.Parameters.AddWithValue("@description", Deskription.Text);
                        cmd.Parameters.AddWithValue("@price", priceValue);
                        cmd.Parameters.AddWithValue("@discount", discountValue);
                        cmd.Parameters.AddWithValue("@stock", stockValue);
                        cmd.Parameters.AddWithValue("@image", _selectedImagePath);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Товар успешно обновлён!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}