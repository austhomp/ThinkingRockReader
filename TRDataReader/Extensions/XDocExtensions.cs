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

        public static string GetRelativeElementValue(this XElement node, string element)
        {
            var e = node.Element(element);

            if (e == null)
            {
                return "N/A";
            }

            var r = e.Attribute("reference");
            if (r != null)
            {
                return r.GetRelativeValue();
            }
            else
            {
                return (string)e;
            }
        }

        public static string GetRelativeValue(this XAttribute attribute)
        {
            var relativeValue = attribute.Parent.GetRelativeValue(attribute.Value);
            return relativeValue;
        }

        public static string GetRelativeValue(this XElement node, string pathReference, string targetElementName = null)
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
                        var index = int.Parse(part.Substring(opening + 1, ending - opening - 1));

                        // apparently we have to not count any items that are just references themselves. Not obvious nor was it easy to determine.
                        current = current.Descendants(target)
                            .Where(x => x.Elements().Where(y => !y.Value.Contains("reference")).Any())
                            .Skip(index - 1)
                            .First();
                    } 
                    else
                    {
                        var prev = current;
                        current = current.Element(part);
                        if (current == null)
                        {
                            var maybeRef = prev.Attribute("reference");
                            if (maybeRef != null)
                            {
                                return prev.GetRelativeValue(maybeRef.Value, part);
                            }
                        }
                    }
                    
                }
            }

            if (current == null) return "<Unknown>";

            if (targetElementName != null)
            {
                var d = current.Element(targetElementName);
                if (d != null)
                {
                    current = d;
                }
            }

            string val = (string) current.Element("name") ?? current.Value;
            return val;
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