using System.Xml.Serialization;

namespace Editor.Models
{
    [XmlRoot("Root")]
    public class Root
    {
        [XmlArray("Commands")]
        [XmlArrayItem("ZPLCommand")]
        public required List<ZplCommand> Commands { get; set; }
    }

    public class ZplCommand
    {
        [XmlElement("Name")]
        public required string Name { get; set; }

        [XmlElement("Category")]
        public required string Category { get; set; }

        [XmlElement("ShortDesc")]
        public required string ShortDesc { get; set; }

        [XmlElement("LongDesc")]
        public required string LongDesc { get; set; }

        [XmlElement("Usage")]
        public required string Usage { get; set; }

        [XmlElement("Command")]
        public required string Command { get; set; }

        [XmlArray("ZPLCommandParameter")]
        [XmlArrayItem("TextBox", typeof(TextBox))]
        [XmlArrayItem("NumericBox", typeof(NumericBox))]
        [XmlArrayItem("ComboBox", typeof(ComboBox))]
        public required List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        [XmlAttribute("Name")]
        public required string Name { get; set; }

        [XmlAttribute("Description")]
        public required string Description { get; set; }
    }

    public class TextBox : Parameter
    {
        [XmlAttribute("AcceptedValue")]
        public required string AcceptedValue { get; set; }
    }

    public class NumericBox : Parameter
    {
        [XmlAttribute("AcceptedValue")]
        public required string AcceptedValue { get; set; }
    }

    public class ComboBox : Parameter
    {
        [XmlArray("ComboBoxValues")]
        [XmlArrayItem("ComboBoxValue", typeof(ComboBoxValue))]
        public required List<ComboBoxValue> Values { get; set; }
    }

    public class ComboBoxValue
    {
        [XmlAttribute("Value")]
        public required string Value { get; set; }

        [XmlText]
        public required string Text { get; set; }
    }
}
