using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TRS2
{
    public class TableGroup : GroupBox
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(TableGroup),
            new PropertyMetadata(null, OnLayoutChanged));

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            nameof(Rows),
            typeof(IEnumerable<GroupRowDefinition>),
            typeof(TableGroup),
            new PropertyMetadata(null, OnLayoutChanged));

        public static readonly DependencyProperty CornerTextProperty = DependencyProperty.Register(
            nameof(CornerText),
            typeof(string),
            typeof(TableGroup),
            new PropertyMetadata(string.Empty, OnLayoutChanged));

        public static readonly DependencyProperty ColumnHeaderPrefixProperty = DependencyProperty.Register(
            nameof(ColumnHeaderPrefix),
            typeof(string),
            typeof(TableGroup),
            new PropertyMetadata(string.Empty, OnLayoutChanged));

        public TableGroup()
        {
            Margin = new Thickness(0, 0, 20, 12);
            Padding = new Thickness(8);
            Loaded += OnLoaded;
        }

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IEnumerable<GroupRowDefinition>? Rows
        {
            get => (IEnumerable<GroupRowDefinition>?)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public string CornerText
        {
            get => (string)GetValue(CornerTextProperty);
            set => SetValue(CornerTextProperty, value);
        }

        public string ColumnHeaderPrefix
        {
            get => (string)GetValue(ColumnHeaderPrefixProperty);
            set => SetValue(ColumnHeaderPrefixProperty, value);
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TableGroup)d).BuildContent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildContent();
        }

        private void BuildContent()
        {
            if (!IsLoaded)
            {
                return;
            }

            var items = ItemsSource?.Cast<object>().ToList() ?? new List<object>();
            var rows = Rows?.ToList() ?? new List<GroupRowDefinition>();
            var grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            for (var columnIndex = 0; columnIndex < items.Count; columnIndex++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            }

            grid.Children.Add(CreateHeaderCell(CornerText, 0, 0));

            for (var columnIndex = 0; columnIndex < items.Count; columnIndex++)
            {
                grid.Children.Add(CreateHeaderCell(GetColumnHeader(columnIndex), 0, columnIndex + 1));
            }

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(CreateHeaderCell(rows[rowIndex].Label, rowIndex + 1, 0));

                for (var columnIndex = 0; columnIndex < items.Count; columnIndex++)
                {
                    var cell = new Border
                    {
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Child = GroupControlFactory.CreateCell(rows[rowIndex], items[columnIndex])
                    };

                    Grid.SetRow(cell, rowIndex + 1);
                    Grid.SetColumn(cell, columnIndex + 1);
                    grid.Children.Add(cell);
                }
            }

            Content = grid;
        }

        private UIElement CreateHeaderCell(string text, int row, int column)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4, 2, 4, 2),
                Child = new TextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            return border;
        }

        private string GetColumnHeader(int columnIndex)
        {
            return string.IsNullOrWhiteSpace(ColumnHeaderPrefix)
                ? (columnIndex + 1).ToString()
                : $"{ColumnHeaderPrefix}{columnIndex + 1}";
        }
    }
}
