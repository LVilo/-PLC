using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AWS;

public partial class Dialog : Window
{
    public Dialog()
    {
        InitializeComponent();
    }
  
    public bool Dialog_result { get; private set; }
    public bool Dialog_Cancel { get; private set; }
    public float Range { get; private set; } = 0f;
    public float coef_trans { get; private set; } = 10f;
    public void SetImageSource(string path)
    {
        Image_Panel.Source = LoadEmbeddedImage(path);
    }
    private static Bitmap LoadEmbeddedImage(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);
        foreach (var name in assembly.GetManifestResourceNames())
            Console.WriteLine(name);
        if (stream == null)
            throw new FileNotFoundException($"Ресурс не найден: {resourceName}");

        return new Bitmap(stream);
    }
    private async void OK_Click(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("OK_Click");
        if (((TextBox_Range.Text == null || TextBox_Range.Text == "") && TextBlock_range.IsVisible) || ((TextBox_Coef_Trans.Text == null || TextBox_Coef_Trans.Text == "") && TextBox_Coef_Trans.IsVisible))
        {
            Console.WriteLine("if");
            WindowInfo info = new WindowInfo("Введите число","Предупреждение");
            Console.WriteLine("info");
            info.Show();
            Console.WriteLine("Show");
            return;
        }
        Dialog_result = true;
        Range = Convert.ToSingle(TextBox_Range.Text);
        coef_trans = Convert.ToSingle(TextBox_Coef_Trans.Text);
        Close();
    }

    private async void Skip_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_result = false;
        Close();
    }
    private async void Canel_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_Cancel = true;
        Close();
    }
    private void Range_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var digitsOnly = FilterText(textBox.Text);

            if (textBox.Text != digitsOnly)
            {
                var caretIndex = textBox.CaretIndex;
                textBox.Text = digitsOnly;
                textBox.CaretIndex = Math.Min(caretIndex, digitsOnly.Length);
            }
        }
    }
    private void Coef_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var digitsOnly = FilterText(textBox.Text);

            if (textBox.Text != digitsOnly)
            {
                var caretIndex = textBox.CaretIndex;
                textBox.Text = digitsOnly;
                textBox.CaretIndex = Math.Min(caretIndex, digitsOnly.Length);
            }
        }
    }
    private static string FilterText(string text)
    {
        string filteredText = "";
        bool foundComma = false;
        int commaCount = 0;
        if (text.StartsWith(',')) { text = text[1..]; }
        foreach (char c in text)
        {
           
            if (char.IsDigit(c) || (c == ',' && !foundComma))
            {
                filteredText += c;
                if (c == ',')
                {
                    foundComma = true;
                    commaCount++;
                }
            }
            if (filteredText.Length == 9) break;
        }
        int commaIndex = filteredText.IndexOf(',');
        if (commaIndex != -1 && filteredText.Length - commaIndex > 4) // 3, потому что один символ для запятой
        {
            filteredText = filteredText.Substring(0, commaIndex + 4);
        }
        return filteredText;
    }
}