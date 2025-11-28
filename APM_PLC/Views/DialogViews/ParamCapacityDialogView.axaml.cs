using APM_PLC.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace APM_PLC.Views.DialogViews;

public partial class ParamCapacityDialogView : UserControl
{
    public ParamCapacityDialogView()
    {
        InitializeComponent();
    }

   private void FilterText(object? sender, TextChangedEventArgs e)
    {
        if( sender is TextBox textbox)
        {
            string newstring = FilterTextModel.OnlyFloat(textbox.Text);
            if (newstring is null || newstring is "") newstring = "0";
            if (textbox.Text != newstring)
            {
                var caretIndex = textbox.CaretIndex;
                textbox.Text = newstring;
                textbox.CaretIndex = Math.Min(caretIndex, newstring.Length);
            }
        }
    }
}