using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace TRS2
{
    public partial class IntCell : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(IntCell),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        private int _lastValidValue;

        public IntCell()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _lastValidValue = Value;
            ValueTextBox.Text = Value.ToString(CultureInfo.CurrentCulture);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IntCell)d;
            control._lastValidValue = control.Value;

            if (!control.ValueTextBox.IsKeyboardFocusWithin)
            {
                control.ValueTextBox.Text = control.Value.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void OnValueTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ValueTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var newValue))
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
