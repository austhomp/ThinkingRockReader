using System;
using System.Xml.Linq;
using System.Linq;

namespace TRDataReader.Extensions
{
    public static class XDocExtensions
    {
        public static string GetPath(this XElement node)
        {
            string path = node.Name.ToString();
            XElement currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                path = currentNode.Name.ToString() + @"\" + path;
            }
            return path;
        }

        public static string GetAttributeOrValue(this XElement node, string attributeName)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }

            var attribute = node.Attribute(attributeName);
            return attribute != null ? attribute.Value : node.Value;
        }

        public static string GetRelativeValue(this XAttribute attribute)
        {
            return attribute.Parent.GetRelativeValue(attribute.Value);
        }

        public static string GetRelativeValue(this XElement node, string pathReference)
        {
            var parts = pathReference.Split(new string[] { "/"}, StringSplitOptions.RemoveEmptyEntries);
            XElement current = node;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (part == "..")
                {
                    current = current.Parent;
                } 
                else
                {
                    if (part.Contains("["))
                    {
                        var opening = part.IndexOf("[");
                        var target = part.Substring(0, opening);
                        var ending = part.IndexOf("]");
                        var index = int.Parse(part.Substring(opening + 1, ending - opening-1));

                        current = current.Descendants(target).Skip(index-1).First();
                    } 
                    else
                    {
                        current = current.Element(part);    
                    }
                    
                }
            }
            if (current == null) return "<Unknown>";
            return (string)current.Element("name") ?? current.Value as string;
        }

        public static XElement GetRelativeNode(this XAttribute attribute)
        {
            return attribute.Parent.GetRelativeNode(attribute.Value);
        }

        public static XElement GetRelativeNode(this XElement node, string pathReference)
        {
            if (!pathReference.Contains("..")) return node;

            var parts = pathReference.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            XElement current = node;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (part == "..")
                {
                    current = current.Parent;
                }
                else
                {
                    if (part.Contains("["))
                    {
                        var opening = part.IndexOf("[");
                        var target = part.Substring(0, opening);
                        var ending = part.IndexOf("]");
                        var index = int.Parse(part.Substring(opening + 1, ending - opening - 1));

                        current = current.Descendants(target).Skip(index - 1).First();
                    }
                    else
                    {
                        current = current.Element(part);
                    }

                }
            }

            return current;
        }

        public static XElement GetRelativeNodeFromAttribute(this XElement node, string attributeWithPossibleReference)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var attribute = node.Attribute(attributeWithPossibleReference);
            return attribute != null ? node.GetRelativeNode(attribute.Value) : node;
        }
    }
}