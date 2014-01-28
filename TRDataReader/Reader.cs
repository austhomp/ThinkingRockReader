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

            ////var firstElement = (XElement)doc.FirstNode;
            var singleActionsBaseNode = doc.Element("data").Element("rootActions");
            var singleActionsNode = singleActionsBaseNode.GetRelativeNode(singleActionsBaseNode.Attribute("reference").Value).Element("children");
            var singleActions = GetActions(singleActionsNode);
            
            var projectActionsBaseNode = doc.Element("data").Element("rootProject");
            var projectActionsNode = projectActionsBaseNode.GetRelativeNode(projectActionsBaseNode.Attribute("reference").Value).Element("children");
            var projectActions = GetActions(projectActionsNode);
            
            var results = new List<Thought>();
            results.AddRange(singleActions);
            results.AddRange(projectActions);

            results = results.OrderBy(x => x.Context).ThenBy(x => x.Priority).ThenBy(x => x.Description).ToList();

            return results;
        }

        private static IEnumerable<Thought> GetActions(XElement root)
        {
            IEnumerable<Thought> myitems = from thought in root.Descendants("action")
                                           //"thought")
                                           where !thought.IsEmpty && thought.Element("done").Value != "true"
                                           ////&& !(thought.Element("parent").Attribute("reference").Value ?? string.Empty).Contains("future")
                                           select
                                               new Thought
                                                   {
                                                       Description = thought.Element("description").Value,
                                                       Done = Convert.ToBoolean(thought.Element("done").Value),
                                                       Node = thought,
                                                       ////ParentReference =
                                                       ////    (thought.Element("parent") != null)
                                                       ////        ? (string)
                                                       ////          thought.Element("parent").Attribute("reference")
                                                       ////        : string.Empty,
                                                       ActionState = thought.Element("state").Attribute("class").Value,
                                                       Context =
                                                           (thought.Element("context").Attribute("reference") != null)
                                                               ? (string)
                                                                 thought.Element("context").Attribute("reference").
                                                                     GetRelativeValue()
                                                               : (string)thought.Element("context"),
                                                       Priority =
                                                           thought.Element("priority") != null
                                                               ? (thought.Element("priority").Attribute("reference") != null)
                                                                     ? (string)
                                                                       thought.Element("priority").Attribute("reference").
                                                                           GetRelativeValue()
                                                                     : (string)thought.Element("priority")
                                                               : "N/A"
                                                       ////Context = (thought.Element("context").Attribute("reference") != null) ? (string)thought.Element("context").Attribute("reference") : (string)thought.Element("context")
                              
                                                       ////Name = thought.Element("Name").Value,
                                                       ////Description = thought.Element("description").Value
                                                   };

            var results =
                myitems.Where(
                    x => x.ActionState.Contains("ASAP") && !x.ParentReference.EmptyIfNull().Contains("future"));
                    //OrderBy(x => x.Context).ThenBy(x => x.Description);
            return results;
        }

        #endregion
    }
}