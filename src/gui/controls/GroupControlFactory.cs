using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TRS2
{
    internal static class GroupControlFactory
    {
        public static FrameworkElement CreateCell(GroupRowDefinition row, object source)
        {
            FrameworkElement element = row.Kind switch
            {
                CellKind.String => new StringCell(),
                CellKind.Int => new IntCell(),
                CellKind.Double => new DoubleCell(),
                CellKind.Check => new CheckCell(),
                CellKind.Choice => new ChoiceCell
                {
                    Choices = row.Choices ?? Array.Empty<string>()
                },
                _ => new TextBlock()
            };

            BindingOperations.SetBinding(
                element,
                GetValueProperty(row.Kind),
                new Binding(row.PropertyName)
                {
                    Source = source,
                    Mode = BindingMode.TwoWay
                });

            return element;
        }

        private static DependencyProperty GetValueProperty(CellKind kind)
        {
            return kind switch
            {
                CellKind.String => StringCell.ValueProperty,
                CellKind.Int => IntCell.ValueProperty,
                CellKind.Double => DoubleCell.ValueProperty,
                CellKind.Check => CheckCell.ValueProperty,
                CellKind.Choice => ChoiceCell.ValueProperty,
                _ => StringCell.ValueProperty
            };
        }
    }
}
