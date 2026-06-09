using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
using System.Windows.Shapes;

// 

namespace TestWPF
{
 
    public partial class CataloWindow : Window
    {
        public CataloWindow()
        {
            InitializeComponent();
            //путь к БД 
            string Connect = @"Data Source =E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";
            using (var Connection = new SqliteConnection(Connect))
            {
                Connection.Open();
                string Request = "SELECT * FROM Products p JOIN Manufacturers m on p.manufacturer_id = m.manufacturer_id";
                using (var command = new SqliteCommand(Request, Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {


                        while (reader.Read())
                        {
                            // Получаем значения из БД
                            int id = reader.GetInt32(reader.GetOrdinal("product_id"));
                            string name = reader.GetString(reader.GetOrdinal("name"));
                            decimal price = reader.GetDecimal(reader.GetOrdinal("selling_price"));
                            decimal discount = reader.GetDecimal(reader.GetOrdinal("discount_percent"));
                            int stock = reader.GetInt32(reader.GetOrdinal("stock_quantity"));
                            string imagePath = reader.IsDBNull(reader.GetOrdinal("image_path"))
                                ? ""
                                : reader.GetString(reader.GetOrdinal("image_path"));
                            string manufacturer = reader.GetString(reader.GetOrdinal("name"));
                            string description = reader.IsDBNull(reader.GetOrdinal("description"))
                                ? "Нет описания"
                                : reader.GetString(reader.GetOrdinal("description"));

                            // Создаём карточку с данными из БД
                            Border card = CreateNiceCard(
                                Category: "Обувь",                          
                                NameProduct: name,                         
                                Discription: description,                 
                                Manufacturer: manufacturer,                
                                Supplier: "Поставщик",                      
                                Price: price.ToString(),                    
                                Measurement: "шт",                         
                                QuantityInStock: stock.ToString(),          
                                PathToImg: imagePath,                       
                                DisCount: discount.ToString(),
                                id
                            );

                            MainStak.Children.Add(card);
                        }

                    } 
                }
            }

             

        

        }
        public void SetUserName(string Name)
        {
            UserName.Text = Name;
        }
        private void SerchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string BoxShearh = SerchBox.Text;

            string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";
            using (var Connetction = new SqliteConnection(Connect))
            {
                Connetction.Open();
                string Request = @"
            SELECT 
                p.name,
                p.selling_price,
                p.discount_percent,
                p.stock_quantity,
                p.image_path,
                p.product_id,
                m.name AS manufacturer_name
            FROM Products p 
            JOIN Manufacturers m ON p.manufacturer_id = m.manufacturer_id
            WHERE p.name LIKE @search 
               OR m.name LIKE @search";

                using (var command = new SqliteCommand(Request, Connetction))
                {
                    command.Parameters.AddWithValue("@search", $"%{BoxShearh}%");
                    using (var reader = command.ExecuteReader())
                    {
                        MainStak.Children.Clear();

                        while (reader.Read())
                        {
                            Border card = CreateNiceCard(
                                Category: reader["name"].ToString(),
                                NameProduct: reader["name"].ToString(),
                                Discription: "Описание товара",
                                Manufacturer: reader["manufacturer_name"].ToString(),
                                Supplier: "Поставщик",
                                Price: reader["selling_price"].ToString(),
                                Measurement: "шт",
                                QuantityInStock: reader["stock_quantity"].ToString(),
                                PathToImg: reader["image_path"]?.ToString() ?? "",
                                DisCount: reader["discount_percent"].ToString(),
                                ID: reader.GetInt32(reader.GetOrdinal("product_id"))
                            );
                            MainStak.Children.Add(card);
                        }
                    }
                }
            }
        }
        //ButtonLogout
        //
        //ButtonEdit_Click

       
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        //ButtonLogout
        //ButtonAddProduct
        //ButtonEdit_Click
        //ButtonDeleted_Click

        private void ButtonDeleted_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверяем, что поле не пустое
            if (string.IsNullOrWhiteSpace(IDBox.Text))
            {
                MessageBox.Show("Введите ID товара");
                return;
            }

            // 2. Парсим ID
            if (!int.TryParse(IDBox.Text, out int id))
            {
                MessageBox.Show("ID должен быть числом");
                return;
            }

            // 3. Подтверждение удаления
            var result = MessageBox.Show($"Удалить товар с ID {id}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";

            using (var Connection = new SqliteConnection(Connect))
            {
                Connection.Open();

                // 4. ПРОВЕРКА: есть ли товар в заказах?
                string checkQuery = "SELECT COUNT(*) FROM OrderDetails WHERE product_id = @id";
                using (var checkCmd = new SqliteCommand(checkQuery, Connection))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    int orderCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (orderCount > 0)
                    {
                        MessageBox.Show($"Невозможно удалить товар! Он есть в {orderCount} заказах.");
                        return;
                    }
                }

                // 5. Удаляем товар (только если нет в заказах)
                string Request = @"DELETE FROM Products WHERE product_id = @id";
                using (var command = new SqliteCommand(Request, Connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Товар с ID {id} успешно удалён!");
                        RefreshProducts();  // Обновляем список
                    }
                    else
                    {
                        MessageBox.Show($"Товар с ID {id} не найден");
                    }
                }
            }
        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)  // ← RoutedEventArgs!
        {
            EditWindow editWindow = new EditWindow(int.Parse(IDBox.Text));
            editWindow.Owner = this;
            editWindow.ShowDialog(); 

        }
        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)  // ← RoutedEventArgs!
        {
            AddProduct addProduct = new AddProduct();
            addProduct.Show();

        }
        private void ButtonLogout_Click(object sender, RoutedEventArgs e)  // ← RoutedEventArgs!
        {

            MainWindow authWindow = new MainWindow();

            // Показываем окно
            authWindow.Show();
            this.Close();

        }
        private void RefreshProducts()
        {
            string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";

            using (var Connection = new SqliteConnection(Connect))
            {
                Connection.Open();

               
                string order = "ASC";  
                if (SortDescending != null && SortDescending.IsChecked == true)
                {
                    order = "DESC";
                }

                string Request = $@"
            SELECT 
                p.product_id,
                p.name,
                p.description,
                p.selling_price,
                p.discount_percent,
                p.stock_quantity,
                p.image_path,
                m.name AS manufacturer_name
            FROM Products p 
            JOIN Manufacturers m ON p.manufacturer_id = m.manufacturer_id
            ORDER BY p.stock_quantity {order}";

                using (var command = new SqliteCommand(Request, Connection))
                using (var reader = command.ExecuteReader())
                {
                    MainStak.Children.Clear();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("product_id"));
                        string name = reader.GetString(reader.GetOrdinal("name"));
                        decimal price = reader.GetDecimal(reader.GetOrdinal("selling_price"));
                        decimal discount = reader.GetDecimal(reader.GetOrdinal("discount_percent"));
                        int stock = reader.GetInt32(reader.GetOrdinal("stock_quantity"));
                        string imagePath = reader.IsDBNull(reader.GetOrdinal("image_path"))
                            ? "" : reader.GetString(reader.GetOrdinal("image_path"));
                        string manufacturer = reader.GetString(reader.GetOrdinal("manufacturer_name"));
                        string description = reader.IsDBNull(reader.GetOrdinal("description"))
                            ? "Нет описания" : reader.GetString(reader.GetOrdinal("description"));

                        Border card = CreateNiceCard(
                            Category: "Обувь",
                            NameProduct: name,
                            Discription: description,
                            Manufacturer: manufacturer,
                            Supplier: "Поставщик",
                            Price: price.ToString(),
                            Measurement: "шт",
                            QuantityInStock: stock.ToString(),
                            PathToImg: imagePath,
                            DisCount: discount.ToString(),
                            ID: id
                        );

                        MainStak.Children.Add(card);
                    }
                }
            }
        }
        private void Sort_Checked(object sender, RoutedEventArgs e)
        {
            string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";

            using (var Connection = new SqliteConnection(Connect))
            {
                Connection.Open();

                string order = "ASC";  // по умолчанию

                if (SortDescending.IsChecked == true)
                {
                    order = "DESC";  // по убыванию
                }

                string Request = $@"
            SELECT 
                p.name,
                p.selling_price,
                p.discount_percent,
                p.stock_quantity,
                p.image_path,
                p.product_id,
                m.name AS manufacturer_name
            FROM Products p 
            JOIN Manufacturers m ON p.manufacturer_id = m.manufacturer_id
            ORDER BY p.stock_quantity {order}";

                using (var command = new SqliteCommand(Request, Connection))
                using (var reader = command.ExecuteReader())
                {
                    MainStak.Children.Clear();

                    while (reader.Read())
                    {
                        Border card = CreateNiceCard(
                            Category: reader["name"].ToString(),
                            NameProduct: reader["name"].ToString(),
                            Discription: "Описание товара",
                            Manufacturer: reader["manufacturer_name"].ToString(),
                            Supplier: "Поставщик",
                            Price: reader["selling_price"].ToString(),
                            Measurement: "шт",
                            QuantityInStock: reader["stock_quantity"].ToString(),
                            PathToImg: reader["image_path"]?.ToString() ?? "",
                            DisCount: reader["discount_percent"].ToString(),
                            ID:reader.GetInt32(reader.GetOrdinal("product_id"))
                        );
                        MainStak.Children.Add(card);
                    }
                }
            }
        }


        private Border CreateNiceCard(
      string Category,
      string NameProduct,
      string Discription,
      string Manufacturer,      
      string Supplier,
      string Price,
      string Measurement,
      string QuantityInStock,
      string PathToImg,
      string DisCount,
      int ID
            )
        {
            Border BorderS = new Border();
            BorderS.BorderBrush = Brushes.Gray;
            BorderS.BorderThickness = new Thickness(1);
            BorderS.CornerRadius = new CornerRadius(8);
            BorderS.Padding = new Thickness(10);
            BorderS.Margin = new Thickness(0, 0, 0, 10);
            BorderS.Background = new SolidColorBrush(Colors.LightGray);
           

            

            StackPanel MainStack = new StackPanel();
            MainStack.Orientation = Orientation.Horizontal;

       
            Image Img = new Image();
            Img.Width = 250;
            Img.Height = 250;
            Img.Margin = new Thickness(5);
            Img.Source = new BitmapImage(new Uri(PathToImg, UriKind.RelativeOrAbsolute));
          
            
            StackPanel DiscriptionStack = new StackPanel();
            DiscriptionStack.Orientation = Orientation.Vertical;

            
            TextBlock TextCategory = new TextBlock();
            TextCategory.Text = $"Категория: {Category}";

            TextBlock TextNameProduct = new TextBlock();
            TextNameProduct.Text = $"Название: {NameProduct}";
            TextNameProduct.FontWeight = FontWeights.Bold;

            TextBlock TextDiscription = new TextBlock();
            TextDiscription.Text = $"Описание товара: {Discription}";
            TextDiscription.TextWrapping = TextWrapping.Wrap;

            TextBlock TextManufacturer = new TextBlock();
            TextManufacturer.Text = $"Производитель: {Manufacturer}";

            TextBlock TextSupplier = new TextBlock();
            TextSupplier.Text = $"Поставщик: {Supplier}";

            TextBlock TextPrice = new TextBlock();
            TextPrice.Text = $"Цена: {Price} руб.";

            TextBlock TextMeasurement = new TextBlock();
            TextMeasurement.Text = $"Единица измерения: {Measurement}";

            TextBlock TextQuantityInStock = new TextBlock();
            TextQuantityInStock.Text = $"Количество на складе: {QuantityInStock} шт.";

            TextBlock TextPathToImg = new TextBlock();
            TextPathToImg.Text = PathToImg;
            TextPathToImg.Visibility = Visibility.Collapsed; // Скрываем путь к изображению

            TextBlock TextDisCount = new TextBlock();
            TextDisCount.Text = $"Скидка:\n {DisCount}%";

            TextBlock TextID = new TextBlock();
            TextID.Text = $"ID: {ID}";  // ← теперь правильно
            TextID.Margin = new Thickness(0, 5, 0, 0);
            TextID.FontSize = 12;
            TextID.Foreground = Brushes.Gray;


            // Добавляем всё в стек описания
            DiscriptionStack.Children.Add(TextCategory);
            DiscriptionStack.Children.Add(TextNameProduct);
            DiscriptionStack.Children.Add(TextDiscription);
            DiscriptionStack.Children.Add(TextManufacturer);
            DiscriptionStack.Children.Add(TextSupplier);
            DiscriptionStack.Children.Add(TextPrice);
            DiscriptionStack.Children.Add(TextMeasurement);
          
            DiscriptionStack.Children.Add(TextQuantityInStock);
            DiscriptionStack.Children.Add(TextID);


            if (int.Parse(DisCount) > 15) // <-- значения могут отличаеться как в задании
            {
                BorderS.Background = Brushes.Green; // <-- цвет тоже 
            }
            if (int.Parse(QuantityInStock) < 10)  //<-- и тут тоже самое 
            {
                TextQuantityInStock.Background = (Brush)new BrushConverter().ConvertFromString("#40E0D0");
            }
            StackPanel DisCountStack = new StackPanel(); 
            DisCountStack.Orientation = Orientation.Vertical;
            DisCountStack.Children.Add(TextDisCount);
            
          
            MainStack.Children.Add(Img);
            MainStack.Children.Add(DiscriptionStack);
            MainStack.Children.Add(DisCountStack);


            BorderS.Child = MainStack;

            return BorderS;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
