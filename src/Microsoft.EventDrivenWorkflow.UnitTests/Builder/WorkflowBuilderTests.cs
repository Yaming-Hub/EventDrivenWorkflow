using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Builder;
using Microsoft.EventDrivenWorkflow.Definitions;

namespace Microsoft.EventDrivenWorkflow.UnitTests.Builder
{
    [TestClass]
    public class WorkflowBuilderTests
    {
        [TestMethod]
        public void BuildSingleActivityWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.AddActivity("a1");

            var wd = wb.Build();

            Assert.AreEqual("a1", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildSequentialWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1").Publish("e2");
            wb.AddActivity("a3").Subscribe("e2");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/a2,a2/e2/a3", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildSplitWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a3").Subscribe("e2");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/a2,a1/e2/a3", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildAggregateWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.RegisterEvent("e3");
            wb.RegisterEvent("e4");
            wb.AddActivity("a1").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1").Publish("e3");
            wb.AddActivity("a3").Subscribe("e2").Publish("e4");
            wb.AddActivity("a4").Subscribe("e3").Subscribe("e4");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/a2,a1/e2/a3,a2/e3/a4,a3/e4/a4", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildStaticWorkflowWithLoopWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1").Publish("e1");

            wb.Build();
        }

        [TestMethod]
        public void BuildDynamicWorkfowWithLoop()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Dynamic);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1").Publish("e1");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/a2,a2/e1/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildChildWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Publish("e1");

            var x = wb.AddWorkflow("x");
            x.RegisterEvent("xe1");
            x.AddActivity("a1").Subscribe("e1").Publish("xe1");
            x.AddActivity("a2").Subscribe("xe1").Publish("e2");

            wb.AddActivity("a2").Subscribe("e2");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/x.a1,x.a1/xe1/x.a2,x.a2/e2/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        public void BuildTwoLevelChildWorkflow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.RegisterEvent("e3");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e2").Subscribe("e3");

            var x = wb.AddWorkflow("x");
            x.RegisterEvent("xe1");
            x.RegisterEvent("xe2");
            x.AddActivity("a1").Subscribe("e1").Publish("xe1").Publish("xe2");
            x.AddActivity("a2").Subscribe("xe1").Publish("e2");

            var y = x.AddWorkflow("y");
            y.RegisterEvent("ye1");
            y.AddActivity("a1").Subscribe("xe2").Publish("ye1");
            y.AddActivity("a2").Subscribe("ye1").Publish("e3");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1/x.a1,x.a1/xe1/x.a2,x.a1/xe2/x.y.a1,x.a2/e2/a2,x.y.a1/ye1/x.y.a2,x.y.a2/e3/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void NoActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void DuplicateEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e1");

            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void OrphanEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");

            wb.AddActivity("a1").Publish("e1");

            wb.Build();
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void DuplicateActivityWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1").Publish("e2");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a2").Subscribe("e2");

            wb.Build();
        }

        [TestMethod]
        public void BuildWorkflowWithStartEvent()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWithoutStartEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Dynamic);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitMoreThanOneStartActivityEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Dynamic);
            wb.AddActivity("a1");
            wb.AddActivity("a2");
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitMoreThanOneStartEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Dynamic);
            wb.RegisterEvent("e1");
            wb.RegisterEvent("e2");
            wb.AddActivity("a1").Subscribe("e1").Subscribe("e2");

            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void BuildWorkflowWitBothStartActivityAndStartEventWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Dynamic);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Subscribe("e1");
            wb.AddActivity("a2");
            wb.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void DuplicateSubscriptionWillThrow()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent("e1");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");
            wb.AddActivity("a3").Subscribe("e1");

            wb.Build();
        }

        [TestMethod]
        public void BuildWorkflowWithPayload()
        {
            var wb = new WorkflowBuilder("Test", WorkflowType.Static);
            wb.RegisterEvent<string>("e1");
            wb.AddActivity("a1").Publish("e1");
            wb.AddActivity("a2").Subscribe("e1");

            var wd = wb.Build();

            Assert.AreEqual("a1,a1/e1(System.String)/a2", wd.GetSignature(out bool _));
            Trace.WriteLine(GraphGenerator.GenerateText(wd));
        }
    }
}
