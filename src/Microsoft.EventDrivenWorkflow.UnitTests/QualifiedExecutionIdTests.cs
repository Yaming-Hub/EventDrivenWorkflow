using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.UnitTests
{
    [TestClass]
    public class QualifiedExecutionIdTests
    {
        private const string IdStr = "2cfbf5df-4854-488c-a3cc-23031b64bc72";
        private static readonly Guid Id = Guid.Parse(IdStr);

        [TestMethod]
        public void ToStringWithPartition()
        {
            var qeid = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            Assert.AreEqual($"[p1]w1/activities/a1/{IdStr}", qeid.ToString());
        }
        [TestMethod]
        public void ToStringWithoutPartition()
        {
            var qeid = new QualifiedExecutionId { WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            Assert.AreEqual($"w1/activities/a1/{IdStr}", qeid.ToString());
        }

        [TestMethod]
        public void TestTryParseWithPartition()
        {
            Assert.IsTrue(QualifiedExecutionId.TryParse($"[p1]w1/activities/a1/{IdStr}", out QualifiedExecutionId qeid));
            Assert.IsTrue(qeid.Equals(new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id }));
        }

        [TestMethod]
        public void TestTryParseWithoutPartition()
        {
            Assert.IsTrue(QualifiedExecutionId.TryParse($"w1/activities/a1/{IdStr}", out QualifiedExecutionId qeid));
            Assert.IsTrue(qeid.Equals(new QualifiedExecutionId { WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id }));
        }

        [TestMethod]
        public void TestEquals()
        {
            var qeid1 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            var qeid2 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            var qeid3 = new QualifiedExecutionId { PartitionKey = "p2", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            var qeid4 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w2", ActivityName = "a1", ExecutionId = Id };
            var qeid5 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a2", ExecutionId = Id };
            var qeid6 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Guid.NewGuid() };
            var qeid7 = new QualifiedExecutionId { WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };

            Assert.IsTrue(qeid1.Equals(qeid2));
            Assert.IsFalse(qeid1.Equals(qeid3));
            Assert.IsFalse(qeid1.Equals(qeid4));
            Assert.IsFalse(qeid1.Equals(qeid5));
            Assert.IsFalse(qeid1.Equals(qeid6));
            Assert.IsFalse(qeid1.Equals(qeid7));
            Assert.IsFalse(qeid1.Equals(string.Empty));
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var qeid1 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            var qeid2 = new QualifiedExecutionId { PartitionKey = "p1", WorkflowName = "w1", ActivityName = "a1", ExecutionId = Id };
            Assert.AreEqual(qeid1.GetHashCode(), qeid2.GetHashCode());
        }
    }
}
