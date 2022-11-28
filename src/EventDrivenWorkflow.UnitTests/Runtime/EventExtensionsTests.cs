using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Runtime;

namespace EventDrivenWorkflow.UnitTests.Runtime
{
    [TestClass]
    public class EventExtensionsTests
    {
        [TestMethod]
        public void GetPayload()
        {
            var @event = new Event<string>()
            {
                Payload = "the-payload"
            };

            object payload = @event.GetPayload(typeof(string));
            Assert.AreEqual("the-payload", payload);
        }

        [TestMethod]
        public void SetPayload()
        {
            var sourceEvent = new Event()
            {
                Id = Guid.NewGuid(),
                DelayDuration = TimeSpan.FromSeconds(3),
                Name = "TestEvent",
                SourceEngineId = "TestEngine"
            };

            object newEvent = sourceEvent.SetPayload(typeof(string), "the-payload");
            var targetEvent = newEvent as Event<string>;
            Assert.IsNotNull(targetEvent);
            Assert.AreEqual(sourceEvent.Id, targetEvent.Id);
            Assert.AreEqual(sourceEvent.DelayDuration, targetEvent.DelayDuration);
            Assert.AreEqual(sourceEvent.Name, targetEvent.Name);
            Assert.AreEqual(sourceEvent.SourceEngineId, targetEvent.SourceEngineId);
            Assert.AreEqual("the-payload", targetEvent.Payload);
        }
    }
}
