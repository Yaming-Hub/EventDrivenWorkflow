using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.UnitTests
{
    [TestClass]
    public class QualifiedExecutionIdTests
    {
        private const string ExeIdStr = "136b00bb-b78b-44c7-95f4-71ff40d58c9d";
        private const string WfIdStr = "a4086b00-f65c-4d0f-be93-f51f97a3426b";
        private const string ActIdStr = "2cfbf5df-4854-488c-a3cc-23031b64bc72";

        private static readonly Guid ExeId = Guid.Parse(ExeIdStr);
        private static readonly Guid WfId = Guid.Parse(WfIdStr);
        private static readonly Guid Id = Guid.Parse(ActIdStr);

        [TestMethod]
        public void ToStringWithPartition()
        {
            var qeid = new QualifiedActivityExecutionId
            {
                PartitionKey = "p1",
                ExecutionId = ExeId,
                WorkflowName = "w1",
                WorkflowId = WfId,
                ActivityName = "a1",
                ActivityId = Id
            };
            Assert.AreEqual($"[p1]{ExeIdStr}:w1/{WfIdStr}/activities/a1/{ActIdStr}", qeid.ToString());
        }
        [TestMethod]
        public void ToStringWithoutPartition()
        {
            var qeid = new QualifiedActivityExecutionId
            {
                WorkflowName = "w1",
                ExecutionId = ExeId,
                WorkflowId = WfId,
                ActivityName = "a1",
                ActivityId = Id
            };
            Assert.AreEqual($"{ExeIdStr}:w1/{WfIdStr}/activities/a1/{ActIdStr}", qeid.ToString());
        }

        [TestMethod]
        public void TestTryParseWithPartition()
        {
            Assert.IsTrue(QualifiedActivityExecutionId.TryParse($"[p1]{ExeIdStr}:w1/{WfIdStr}/activities/a1/{ActIdStr}", out QualifiedActivityExecutionId qeid));
            Assert.IsTrue(qeid.Equals(new QualifiedActivityExecutionId
            {
                PartitionKey = "p1",
                ExecutionId = ExeId,
                WorkflowName = "w1",
                WorkflowId = WfId,
                ActivityName = "a1",
                ActivityId = Id
            }));
        }

        [TestMethod]
        public void TestTryParseWithoutPartition()
        {
            Assert.IsTrue(QualifiedActivityExecutionId.TryParse($"{ExeIdStr}:w1/{WfIdStr}/activities/a1/{ActIdStr}", out QualifiedActivityExecutionId qeid));
            Assert.IsTrue(qeid.Equals(new QualifiedActivityExecutionId
            {
                WorkflowName = "w1",
                ExecutionId = ExeId,
                WorkflowId = WfId,
                ActivityName = "a1",
                ActivityId = Id
            }));
        }

        [TestMethod]
        public void TestEquals()
        {
            var qeid1 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            var qeid2 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            var qeid3 = new QualifiedActivityExecutionId { PartitionKey = "p2", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            var qeid4 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w2", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            var qeid5 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a2", ActivityId = Id };
            var qeid6 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Guid.NewGuid() };
            var qeid7 = new QualifiedActivityExecutionId { WorkflowName = "w1", ExecutionId = ExeId, WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };

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
            var qeid1 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            var qeid2 = new QualifiedActivityExecutionId { PartitionKey = "p1", ExecutionId = ExeId, WorkflowName = "w1", WorkflowId = WfId, ActivityName = "a1", ActivityId = Id };
            Assert.AreEqual(qeid1.GetHashCode(), qeid2.GetHashCode());
        }
    }
}
