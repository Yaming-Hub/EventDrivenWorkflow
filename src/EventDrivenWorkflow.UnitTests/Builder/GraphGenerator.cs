using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EventDrivenWorkflow.Definitions;

namespace EventDrivenWorkflow.UnitTests.Builder
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

            // Create start node
            var startActivityNodeElement = xmlDocument.CreateElement("Node", ns);
            startActivityNodeElement.SetAttribute("Id", "[START]");
            startActivityNodeElement.SetAttribute("Label", "[START]");
            startActivityNodeElement.SetAttribute("Category", "Activity");
            nodesElement.AppendChild(startActivityNodeElement);

            foreach (var activityDefinition in workflowDefinition.ActivityDefinitions.Values)
            {
                var activityNodeElement = xmlDocument.CreateElement("Node", ns);
                activityNodeElement.SetAttribute("Id", activityDefinition.Name);
                activityNodeElement.SetAttribute("Label", activityDefinition.Name);
                activityNodeElement.SetAttribute("Category", "Activity");

                nodesElement.AppendChild(activityNodeElement);
            }
            
            if (workflowDefinition.CompleteEvent != null)
            {
                var endActivityNodeElement = xmlDocument.CreateElement("Node", ns);
                endActivityNodeElement.SetAttribute("Id", "[END]");
                endActivityNodeElement.SetAttribute("Label", "[END]");
                endActivityNodeElement.SetAttribute("Category", "Activity");
                nodesElement.AppendChild(endActivityNodeElement);
            }

            var linksElement = xmlDocument.CreateElement("Links", ns);
            graphNode.AppendChild(linksElement);

            workflowDefinition.Traverse(
                visit: link =>
                {
                    var source = link.Source?.Name ?? "[START]";
                    var target = link.Target?.Name ?? "[END]";

                    var incomingLinkElement = xmlDocument.CreateElement("Link", ns);
                    incomingLinkElement.SetAttribute("Source", source);
                    incomingLinkElement.SetAttribute("Target", target);

                    var eventLabel = link.Event.PayloadType == null
                        ? link.Event.Name
                        : $"{link.Event.Name} ({link.Event.PayloadType.Name})";

                    incomingLinkElement.SetAttribute("Label", eventLabel);

                    linksElement.AppendChild(incomingLinkElement);
                    
                },
                visitDuplicate: null);

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
