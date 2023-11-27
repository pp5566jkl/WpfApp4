using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace WpfApp4
{
    /// <summary>
    /// MyDocumentViewer.xaml 的互動邏輯
    /// </summary>
    public partial class MyDocumentViewer : Window
    {
        Color fontColor = Colors.Black;
        public MyDocumentViewer()
        {
            InitializeComponent();
            fontColorPicker.SelectedColor = fontColor;
            foreach (var fontFamily in Fonts.SystemFontFamilies)
            {
                fontFamilyComboBox.Items.Add(fontFamily);
            }
            fontFamilyComboBox.SelectedIndex = 8;

            fontSizeComboBox.ItemsSource = new List<Double> { 8, 9, 10, 11, 12, 20, 24, 32, 40, 50, 60, 70, 80 };
            fontSizeComboBox.SelectedIndex = 4;
        }
         
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MyDocumentViewer myDocumentViewer = new MyDocumentViewer();
            myDocumentViewer.Show();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Rich Text Format (*.rtf)|*.rtf";
            openFileDialog.DefaultExt = "rtf";
            openFileDialog.AddExtension = true;

            if (openFileDialog.ShowDialog() == true)
            {
                // 使用 FileStream 來讀取文件
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    TextRange textRange = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    textRange.Load(fileStream, DataFormats.Rtf);
                }
            }
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "HTML Files|*.html|RTF Files|*.rtf|All Files|*.*";
            saveFileDialog.DefaultExt = "html";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string fileExtension = Path.GetExtension(filePath)?.ToLower();

                if (fileExtension == ".html")
                {
                    string xamlText = ConvertFlowDocumentToHtml(rtbEditor.Document);
                    string htmlText = ConvertXamlToHtml(xamlText);
                    File.WriteAllText(filePath, htmlText, Encoding.UTF8);
                }
                else if (fileExtension == ".rtf")
                {
                    TextRange textRange = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        textRange.Save(fileStream, DataFormats.Rtf);
                    }
                }
              
            }
        }

        

        private string ConvertFlowDocumentToHtml(FlowDocument flowDocument)
        {
            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            string xamlText;

            using (MemoryStream ms = new MemoryStream())
            {
                textRange.Save(ms, DataFormats.Xaml);
                xamlText = Encoding.Default.GetString(ms.ToArray());
            }

            // 這裡需要一個自定義的 XAML 到 HTML 轉換邏輯，以下僅為示例
            string htmlText = ConvertXamlToHtml(xamlText);

            return htmlText;
        }

        private string ConvertXamlToHtml(string xaml)
        {
            // 這裡需要實現一個自定義的 XAML 到 HTML 轉換邏輯
            // 可以使用正則表達式或其他方法進行基本轉換，但要處理所有可能的情況可能會比較複雜
            // 這只是一個簡單的示例，實際中需要更多的處理邏輯
            return xaml.Replace("<Paragraph>", "<p>").Replace("</Paragraph>", "</p>").Replace("<Run>", "").Replace("</Run>", "").Replace("<FlowDocument>", "").Replace("</FlowDocument>", "");
        }



        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object property = rtbEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            boldButton.IsChecked = (property is FontWeight && (FontWeight)property == FontWeights.Bold);

            
            property = rtbEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            italicButton.IsChecked = (property is FontStyle && (FontStyle)property == FontStyles.Italic);

            
            property = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            underlineButton.IsChecked = (property != DependencyProperty.UnsetValue && property == TextDecorations.Underline);

            //依據RicheTextBox所選擇的文字色彩，同步改變fontColorPicker的狀態
            SolidColorBrush? fontBrush = rtbEditor.Selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush;
            if (fontBrush != null)
            {
                fontColorPicker.SelectedColor = fontBrush.Color;
            }

            //依據RicheTextBox所選擇的文字字型，同步改變fontFamilyComboBox的狀態
            property = rtbEditor.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            fontFamilyComboBox.SelectedItem = property;

            //依據RicheTextBox所選擇的文字大小，同步改變fontSizeComboBox的狀態
            property = rtbEditor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            fontSizeComboBox.SelectedItem = property;
        }

        private void fontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSizeComboBox.SelectedItem);
        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamilyComboBox.SelectedItem);
        }

        private void fontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            fontColor = (Color)e.NewValue;
            SolidColorBrush fontBrush = new SolidColorBrush(fontColor);
            rtbEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, fontBrush);
        }



        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            rtbEditor.Document.Blocks.Clear();
        }

        private void BackgroundPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color backgroundColor = e.NewValue ?? Colors.Transparent; // 設定預設顏色，例如透明
            SolidColorBrush backgroundBrush = new SolidColorBrush(backgroundColor);

            if (rtbEditor != null)
            {
                rtbEditor.Background = backgroundBrush;
            }
        }
    }
}
