using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace TRS2
{
    public partial class DoubleCell : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(DoubleCell),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        private double _lastValidValue;

        public DoubleCell()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _lastValidValue = Value;
            ValueTextBox.Text = Value.ToString(CultureInfo.CurrentCulture);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DoubleCell)d;
            control._lastValidValue = control.Value;

            if (!control.ValueTextBox.IsKeyboardFocusWithin)
            {
                control.ValueTextBox.Text = control.Value.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void OnValueTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var newValue))
            {
                Value = newValue;
                _lastValidValue = newValue;
                ValueTextBox.Text = newValue.ToString(CultureInfo.CurrentCulture);
                return;
            }

            ValueTextBox.Text = _lastValidValue.ToString(CultureInfo.CurrentCulture);
        }
    }
}
