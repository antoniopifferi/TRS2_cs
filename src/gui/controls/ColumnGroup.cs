using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TRS2
{
    public class ColumnGroup : GroupBox
    {
        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(
            nameof(DataItem),
            typeof(object),
            typeof(ColumnGroup),
            new PropertyMetadata(null, OnLayoutChanged));

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            nameof(Rows),
            typeof(IEnumerable<GroupRowDefinition>),
            typeof(ColumnGroup),
            new PropertyMetadata(null, OnLayoutChanged));

        public ColumnGroup()
        {
            Margin = new Thickness(0, 0, 20, 12);
            Padding = new Thickness(8);
            Loaded += OnLoaded;
        }

        public object? DataItem
        {
            get => GetValue(DataItemProperty);
            set => SetValue(DataItemProperty, value);
        }

        public IEnumerable<GroupRowDefinition>? Rows
        {
            get => (IEnumerable<GroupRowDefinition>?)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColumnGroup)d).BuildContent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildContent();
        }

        private void BuildContent()
        {
            if (!IsLoaded || DataItem is null)
            {
                return;
            }

            var rows = Rows?.ToList() ?? new List<GroupRowDefinition>();
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var row = rows[rowIndex];
                var label = new TextBlock
                {
                    Text = row.Label,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 2, 8, 2)
                };

                Grid.SetRow(label, rowIndex);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var cell = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    Child = GroupControlFactory.CreateCell(row, DataItem)
                };

                Grid.SetRow(cell, rowIndex);
                Grid.SetColumn(cell, 1);
                grid.Children.Add(cell);
            }

            Content = grid;
        }
    }
}
