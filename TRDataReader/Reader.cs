using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TRDataReader.Extensions;

namespace TRDataReader
{
    public class Reader
    {
        #region Public Methods and Operators

        public List<Thought> ParseData(string input)
        {
            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(input);
            }
            catch (XmlException e)
            {
                throw new InvalidOperationException("ThinkingRock data file was unable to be parsed", e);
            }

            var dataElement = doc.Element("data");
            if (dataElement == null)
            {
                throw new NullReferenceException("Could not find the data element of the input file");
            }

            var rootActionsBaseNode = dataElement.Element("rootActions");
            var rootActionsReferencePath = rootActionsBaseNode.Attribute("reference");
            var rootActionsNode = (rootActionsReferencePath != null)
                                      ? rootActionsBaseNode.GetRelativeNode(rootActionsReferencePath.Value)
                                      : rootActionsBaseNode;
            var singleActionsNode = rootActionsNode.Element("children");
            var singleActions = GetActions(singleActionsNode);

            var rootProjectsBaseNode = dataElement.Element("rootProject");
            var rootProjectsReferencePath = rootProjectsBaseNode.Attribute("reference");
            var rootProjectsNode = (rootProjectsReferencePath != null)
                                       ? rootProjectsBaseNode.GetRelativeNode(rootProjectsReferencePath.Value)
                                       : rootProjectsBaseNode;
            var projectActionsNode = rootProjectsNode.Element("children");
            var projectActions = GetActions(projectActionsNode);
            
            var results = new List<Thought>();
            results.AddRange(singleActions);
            results.AddRange(projectActions);

            results = results.OrderBy(x => x.Context).ThenBy(x => x.Priority).ThenBy(x => x.Description).ToList();

            return results;
        }

        private static IEnumerable<Thought> GetActions(XElement root)
        {
            IEnumerable<Thought> items = from thought in root.Descendants("action")
                                           //"thought")
                                           where !thought.IsEmpty && thought.Element("done").Value != "true"
                                           ////&& !(thought.Element("parent").Attribute("reference").Value ?? string.Empty).Contains("future")
                                           select
                                               new Thought
                                                   {
                                                       Description = thought.Element("description").Value,
                                                       Done = Convert.ToBoolean(thought.Element("done").Value),
                                                       Node = thought,
                                                       ActionState = thought.Element("state").Attribute("class").Value,
                                                       Context = thought.GetRelativeElementValue("context"),
                                                       Priority = thought.GetRelativeElementValue("priority"),
                                                       Notes = (string)thought.Element("notes")
                                                   };

            return items;
        }

        #endregion
    }
}