using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestWPF
{
    public partial class AddProduct : Window
    {
        private string connectionString = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";
        private string selectedImagePath = "";

        public AddProduct()
        {
            InitializeComponent();
        }

        // Загрузка изображения
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                selectedImagePath = openDialog.FileName;

                // Показываем выбранное изображение
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedImagePath);
                bitmap.EndInit();

                // Находим Image в XAML и устанавливаем источник
                var image = FindName("ProductImage") as System.Windows.Controls.Image;
                if (image != null)
                {
                    image.Source = bitmap;
                }
            }
        }

        // Добавление товара
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string name = NameProduct.Text;
                string category = NameCategory.Text;
                string description = Deskription.Text;
                string manufacturer = Manufacturer.Text;
                string supplier = provider.Text;
                string price = Price.Text;
                string unit = Izmerenya.Text;
                string stock = QuantityInstock.Text;
                string discount = Descount.Text;

                
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Введите наименование товара");
                    NameProduct.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(price))
                {
                    MessageBox.Show("Введите цену");
                    Price.Focus();
                    return;
                }

                if (!decimal.TryParse(price, out decimal priceValue))
                {
                    MessageBox.Show("Цена должна быть числом");
                    Price.Focus();
                    return;
                }

                if (!int.TryParse(stock, out int stockValue))
                {
                    stockValue = 0;
                }

                if (!decimal.TryParse(discount, out decimal discountValue))
                {
                    discountValue = 0;
                }



                if (string.IsNullOrWhiteSpace(category)) category = "Без категории";
                if (string.IsNullOrWhiteSpace(manufacturer)) manufacturer = "Неизвестен";
                if (string.IsNullOrWhiteSpace(supplier)) supplier = "Неизвестен";
                if (string.IsNullOrWhiteSpace(unit)) unit = "шт";

                
                string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";

                using (var Connection = new SqliteConnection(Connect))
                {
                    Connection.Open();

                    // 1. Добавляем категорию (если нет)
                    long categoryId = 0;
                    string checkCategory = "SELECT category_id FROM Categories WHERE name = @name";
                    using (var cmd = new SqliteCommand(checkCategory, Connection))
                    {
                        cmd.Parameters.AddWithValue("@name", category);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            categoryId = Convert.ToInt64(result);
                        }
                        else
                        {
                            string insertCategory = "INSERT INTO Categories (name) VALUES (@name)";
                            using (var insert = new SqliteCommand(insertCategory, Connection))
                            {
                                insert.Parameters.AddWithValue("@name", category);
                                insert.ExecuteNonQuery();
                            }
                            // Получаем новый ID
                            using (var cmd2 = new SqliteCommand(checkCategory, Connection))
                            {
                                cmd2.Parameters.AddWithValue("@name", category);
                                categoryId = Convert.ToInt64(cmd2.ExecuteScalar());
                            }
                        }
                    }

                    // 2. Добавляем производителя (если нет)
                    long manufacturerId = 0;
                    string checkManufacturer = "SELECT manufacturer_id FROM Manufacturers WHERE name = @name";
                    using (var cmd = new SqliteCommand(checkManufacturer, Connection))
                    {
                        cmd.Parameters.AddWithValue("@name", manufacturer);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            manufacturerId = Convert.ToInt64(result);
                        }
                        else
                        {
                            string insertManufacturer = "INSERT INTO Manufacturers (name) VALUES (@name)";
                            using (var insert = new SqliteCommand(insertManufacturer, Connection))
                            {
                                insert.Parameters.AddWithValue("@name", manufacturer);
                                insert.ExecuteNonQuery();
                            }
                            using (var cmd2 = new SqliteCommand(checkManufacturer, Connection))
                            {
                                cmd2.Parameters.AddWithValue("@name", manufacturer);
                                manufacturerId = Convert.ToInt64(cmd2.ExecuteScalar());
                            }
                        }
                    }

                    // 3. Добавляем поставщика (если нет)
                    long supplierId = 0;
                    string checkSupplier = "SELECT supplier_id FROM Suppliers WHERE name = @name";
                    using (var cmd = new SqliteCommand(checkSupplier, Connection))
                    {
                        cmd.Parameters.AddWithValue("@name", supplier);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            supplierId = Convert.ToInt64(result);
                        }
                        else
                        {
                            string insertSupplier = "INSERT INTO Suppliers (name) VALUES (@name)";
                            using (var insert = new SqliteCommand(insertSupplier, Connection))
                            {
                                insert.Parameters.AddWithValue("@name", supplier);
                                insert.ExecuteNonQuery();
                            }
                            using (var cmd2 = new SqliteCommand(checkSupplier, Connection))
                            {
                                cmd2.Parameters.AddWithValue("@name", supplier);
                                supplierId = Convert.ToInt64(cmd2.ExecuteScalar());
                            }
                        }
                    }

                    // 4. Добавляем единицу измерения (если нет)
                    long unitId = 0;
                    string checkUnit = "SELECT unit_id FROM Units WHERE name = @name";
                    using (var cmd = new SqliteCommand(checkUnit, Connection))
                    {
                        cmd.Parameters.AddWithValue("@name", unit);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            unitId = Convert.ToInt64(result);
                        }
                        else
                        {
                            string insertUnit = "INSERT INTO Units (name) VALUES (@name)";
                            using (var insert = new SqliteCommand(insertUnit, Connection))
                            {
                                insert.Parameters.AddWithValue("@name", unit);
                                insert.ExecuteNonQuery();
                            }
                            using (var cmd2 = new SqliteCommand(checkUnit, Connection))
                            {
                                cmd2.Parameters.AddWithValue("@name", unit);
                                unitId = Convert.ToInt64(cmd2.ExecuteScalar());
                            }
                        }
                    }

                    // 5. Добавляем товар
                    string productQuery = @"
                INSERT INTO Products 
                (name, description, category_id, manufacturer_id, supplier_id, unit_id, 
                 selling_price, discount_percent, stock_quantity, image_path, is_active, created_date)
                VALUES 
                (@name, @description, @category_id, @manufacturer_id, @supplier_id, @unit_id,
                 @price, @discount, @stock, @image, 1, datetime('now'))";

                    using (var cmd = new SqliteCommand(productQuery, Connection))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@category_id", categoryId);
                        cmd.Parameters.AddWithValue("@manufacturer_id", manufacturerId);
                        cmd.Parameters.AddWithValue("@supplier_id", supplierId);
                        cmd.Parameters.AddWithValue("@unit_id", unitId);
                        cmd.Parameters.AddWithValue("@price", priceValue);
                        cmd.Parameters.AddWithValue("@discount", discountValue);
                        cmd.Parameters.AddWithValue("@stock", stockValue);
                        cmd.Parameters.AddWithValue("@image", selectedImagePath);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Товар успешно добавлен!");
                ClearFields();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            
        }

        // Вспомогательный метод для вставки или получения ID
        private long GetOrInsert(SqliteConnection connection, string tableName, string idColumn, string nameColumn, string value)
        {
            // Проверяем, существует ли запись
            string selectQuery = $"SELECT {idColumn} FROM {tableName} WHERE {nameColumn} = @name";
            using (var cmd = new SqliteCommand(selectQuery, connection))
            {
                cmd.Parameters.AddWithValue("@name", value);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt64(result);
                }
            }

            // Если нет - вставляем
            string insertQuery = $"INSERT INTO {tableName} ({nameColumn}) VALUES (@name)";
            using (var cmd = new SqliteCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@name", value);
                cmd.ExecuteNonQuery();
            }

            // Получаем ID новой записи
            using (var cmd = new SqliteCommand(selectQuery, connection))
            {
                cmd.Parameters.AddWithValue("@name", value);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt64(result);
                }
            }

            return 0;
        }

        // Очистка всех полей
        private void ClearFields()
        {
            NameProduct.Text = "";
            NameCategory.Text = "";
            Deskription.Text = "";
            Manufacturer.Text = "";
            provider.Text = "";
            Price.Text = "";
            Izmerenya.Text = "";
            QuantityInstock.Text = "";
            Descount.Text = "";
            selectedImagePath = "";
        }

        // Выход без сохранения
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}