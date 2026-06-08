using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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

namespace TestWPF
{
    /// <summary>
    /// Логика взаимодействия для GuestWindow.xaml
    /// </summary>
    public partial class GuestWindow : Window
    {
        public GuestWindow()
        {
            InitializeComponent();
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
                                DisCount: discount.ToString()
                            );

                            MainStak.Children.Add(card);
                        }

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
    string DisCount)
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


            // Добавляем всё в стек описания
            DiscriptionStack.Children.Add(TextCategory);
            DiscriptionStack.Children.Add(TextNameProduct);
            DiscriptionStack.Children.Add(TextDiscription);
            DiscriptionStack.Children.Add(TextManufacturer);
            DiscriptionStack.Children.Add(TextSupplier);
            DiscriptionStack.Children.Add(TextPrice);
            DiscriptionStack.Children.Add(TextMeasurement);
            DiscriptionStack.Children.Add(TextQuantityInStock);

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
    }
}
