using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TRS2
{
    public partial class ChoiceCell : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(ChoiceCell),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ChoicesProperty = DependencyProperty.Register(
            nameof(Choices),
            typeof(IEnumerable<string>),
            typeof(ChoiceCell),
            new PropertyMetadata(null));

        public ChoiceCell()
        {
            InitializeComponent();
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public IEnumerable<string>? Choices
        {
            get => (IEnumerable<string>?)GetValue(ChoicesProperty);
            set => SetValue(ChoicesProperty, value);
        }
    }
}
