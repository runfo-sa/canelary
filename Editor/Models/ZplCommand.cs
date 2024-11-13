using System.Xml.Serialization;

namespace Editor.Models
{
    [XmlRoot("Root")]
    public class Root
    {
        [XmlArray("Commands")]
        [XmlArrayItem("ZPLCommand")]
        public List<ZplCommand> Commands { get; set; }
    }

    public class ZplCommand
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Category")]
        public string Category { get; set; }

        [XmlElement("ShortDesc")]
        public string ShortDesc { get; set; }

        [XmlElement("LongDesc")]
        public string LongDesc { get; set; }

        [XmlElement("Usage")]
        public string Usage { get; set; }

        [XmlElement("Command")]
        public string Command { get; set; }

        [XmlArray("ZPLCommandParameter")]
        [XmlArrayItem("TextBox", typeof(TextBox))]
        [XmlArrayItem("NumericBox", typeof(NumericBox))]
        [XmlArrayItem("ComboBox", typeof(ComboBox))]
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }
    }

    public class TextBox : Parameter
    {
        [XmlAttribute("AcceptedValue")]
        public string AcceptedValue { get; set; }
    }

    public class NumericBox : Parameter
    {
        [XmlAttribute("AcceptedValue")]
        public string AcceptedValue { get; set; }
    }

    public class ComboBox : Parameter
    {
        [XmlArray("ComboBoxValues")]
        [XmlArrayItem("ComboBoxValue", typeof(ComboBoxValue))]
        public List<ComboBoxValue> Values { get; set; }
    }

    public class ComboBoxValue
    {
        [XmlAttribute("Value")]
        public string Value { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}