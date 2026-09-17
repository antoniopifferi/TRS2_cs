using System.Collections.Generic;

namespace TRS2
{
    public class GroupRowDefinition
    {
        public GroupRowDefinition()
        {
        }

        public GroupRowDefinition(string label, CellKind kind, string propertyName)
        {
            Label = label;
            Kind = kind;
            PropertyName = propertyName;
        }

        public GroupRowDefinition(string label, CellKind kind, string propertyName, IEnumerable<string> choices)
        {
            Label = label;
            Kind = kind;
            PropertyName = propertyName;
            Choices = choices;
        }

        public string Label { get; set; } = string.Empty;

        public CellKind Kind { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public IEnumerable<string>? Choices { get; set; }
    }
}
