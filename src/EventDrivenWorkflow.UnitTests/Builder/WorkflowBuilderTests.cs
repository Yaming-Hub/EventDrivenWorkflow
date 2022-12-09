using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Builder;
using EventDrivenWorkflow.Definitions;

namespace EventDrivenWorkflow.UnitTests.Builder
{
    [TestClass]
    public class WorkflowBuilderTests
    {
        [TestMethod]
        public void BuildSingleActivityWorkflow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.AddActivity("a1").Subscribe("e0");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildSingleActivityWithOutputEventWorkflow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1/", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildWorkflowWithPayload()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent<string>("e1");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1(System.String)/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildSequentialWorkflow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1").Publish("e2");
            wb.AddActivity("a3").Subscribe("e2");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1/a2,a2/e2/a3", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildSplitWorkflow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a3").Subscribe("e2");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1/a2,a1/e2/a3", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildAggregateWorkflow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.RegisterEvent("e3");
            wb.RegisterEvent("e4");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1").Publish("e3");
            wb.AddActivity("a3").Subscribe("e2").Publish("e4");
            wb.AddActivity("a4").Subscribe("e3").Subscribe("e4");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1/a2,a1/e2/a3,a2/e3/a4,a3/e4/a4", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildWorkfowWithLoop()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e0");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Subscribe("e0").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1").Publish("e1");

            var wd = wb.Build();

            Assert.AreEqual("/e0/a1,a1/e1/a2,a2/e1/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void NoActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void MissingTriggerEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.AddActivity("a1");
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void TriggerEventWithProducerActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitDuplicateEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e1");

            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitOrphanEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");

            wb.AddActivity("a1").Publish("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitOrphanActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Subscribe("e1");
            wb.AddActivity("a2");
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DuplicateActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a2").Subscribe("e2");

            wb.Build();
        }

        [TestMethod]
        public void BuildWorkflowWitMoreThanOneComplete()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.RegisterEvent("e3");
            wb.AddActivity("a1").Subscribe("e1").Publish("e2").Publish("e3");

            var wd = wb.Build();

            Assert.AreEqual("/e1/a1,a1/e2/,a1/e3/", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitMoreThanOneTriggerEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Subscribe("e1").Subscribe("e2");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitDuplicateSubscriptionWillThrow()
        {
            var wb = new WorkflowBuilder("Test");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a3").Subscribe("e1");

            wb.Build();
        }
    }
}
