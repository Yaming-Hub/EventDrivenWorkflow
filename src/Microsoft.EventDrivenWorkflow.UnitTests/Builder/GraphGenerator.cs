using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EventDrivenWorkflow.Definitions;

namespace Microsoft.EventDrivenWorkflow.UnitTests.Builder
{
    public class GraphGenerator
    {
        public static string GenerateText(WorkflowDefinition workflowDefinition)
        {
            var xmlDocument = Generate(workflowDefinition);
            var sb = new StringBuilder(1024);
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
            };

            using (var writer = XmlWriter.Create(sb, xmlWriterSettings))
            {
                xmlDocument.WriteTo(writer);
            }

            return sb.ToString();
        }

        public static XmlDocument Generate(WorkflowDefinition workflowDefinition)
        {
            const string ns = "http://schemas.microsoft.com/vs/2009/dgml";
            var xmlDocument = new XmlDocument();
            var declaration = xmlDocument.CreateXmlDeclaration(version: "1.0", encoding: "utc-8", standalone: null);
            xmlDocument.AppendChild(declaration);

            var graphNode = xmlDocument.CreateElement("DirectedGraph", ns);
            xmlDocument.AppendChild(graphNode);

            var nodesElement = xmlDocument.CreateElement("Nodes", ns);
            graphNode.AppendChild(nodesElement);

            foreach (var activityDefinition in workflowDefinition.ActivityDefinitions.Values)
            {
                var activityNodeElement = xmlDocument.CreateElement("Node", ns);
                activityNodeElement.SetAttribute("Id", activityDefinition.Name);
                activityNodeElement.SetAttribute("Label", activityDefinition.Name);
                activityNodeElement.SetAttribute("Category", "Activity");

                nodesElement.AppendChild(activityNodeElement);
            }

            var linksElement = xmlDocument.CreateElement("Links", ns);
            graphNode.AppendChild(linksElement);

            workflowDefinition.Traverse(link =>
            {
                if (link.Source != null)
                {
                    var incomingLinkElement = xmlDocument.CreateElement("Link", ns);
                    incomingLinkElement.SetAttribute("Source", link.Source.Name);
                    incomingLinkElement.SetAttribute("Target", link.Target.Name);

                    var eventLabel = link.Event.PayloadType == null
                        ? link.Event.Name
                        : $"{link.Event.Name} ({link.Event.PayloadType.Name})";

                    incomingLinkElement.SetAttribute("Label", eventLabel);

                    linksElement.AppendChild(incomingLinkElement);
                }
            });

            var categoriesElement = xmlDocument.CreateElement("Categories", ns);
            graphNode.AppendChild(categoriesElement);

            var activityCategoryElement = xmlDocument.CreateElement("Category", ns);
            activityCategoryElement.SetAttribute("Id", "Activity");
            activityCategoryElement.SetAttribute("Background", "Blue");
            categoriesElement.AppendChild(activityCategoryElement);

            return xmlDocument;
        }
    }
}
