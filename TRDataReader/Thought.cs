using System.Xml.Linq;

namespace TRDataReader
{
    public class Thought
    {
        public string Description { get; set; }

        public bool Done { get; set; }

        public string Priority { get; set; }

        public string ActionState { get; set; }

        public string ParentReference { get; set; }

        public string Context { get; set; }

        public XElement Node { get; set; }
         
    }
}