using ActorModelNet.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Tests.Utilities;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.System.Tests
{
    public class PersistenceTests
    {
        [Fact]
        public async Task CheckPersistenceCalledInInitializing()
        {
            var loggerMock = new Mock<ILogger<ActorSystem>>();
            var persistence = new Mock<IPersistence>();
            var identity = new GuidActorIdentity(Guid.NewGuid());
            var configuration = new ActorSystemConfiguration() { PersistenceTimeoutInSecond = 2 };
            using var actorSystem = new ActorSystem(loggerMock.Object, persistence.Object, configuration);
            actorSystem.Register<ActorTest>();

            var actor1 = actorSystem.Spawn<ActorTest, ActorState>(identity);
            var state1 = await actor1.UnsafeGetState();
            persistence.Verify(s => s.Save(It.IsAny<IActorIdentity>(), It.IsAny<ActorState>()), Times.Once);
        }

        [Fact]
        public async Task CheckPersistenceCalledAfterChange()
        {
            var loggerMock = new Mock<ILogger<ActorSystem>>();
            var persistence = new Mock<IPersistence>();
            var tcs = new TaskCompletionSource();
            var identity = new GuidActorIdentity(Guid.NewGuid());
            persistence.Setup(m => m.Save<ActorState>(identity, It.Is<ActorState>(n => n.IntValue != 5)))
                .Returns(() => {
                    tcs.SetResult();
                    return true;
                });

            var configuration = new ActorSystemConfiguration() { PersistenceTimeoutInSecond = 2 };
            using var actorSystem = new ActorSystem(loggerMock.Object, persistence.Object, configuration);
            actorSystem.Register<ActorTest>();

            var actor1 = actorSystem.Spawn<ActorTest, ActorState>(identity);
            actor1.Send(new AddValue(7));
            var watch = Stopwatch.StartNew();
            actor1.Send(new AddValue(7));


            await tcs.Task;
            watch.Stop();
            var state1 = await actor1.UnsafeGetState();
            Assert.Equal(19, state1.IntValue);
            Assert.Equal(2, Math.Round(watch.Elapsed.TotalSeconds));
            persistence.Verify(s => s.Save(It.IsAny<IActorIdentity>(), It.IsAny<ActorState>()), Times.Exactly(2));
        }

        public class ActorTest : IActor<ActorState>
        {
            public ActorState InitialState() => new ActorState(5);
            public IActorBehavior<ActorState> Behaviour() => new ActorBehaviour(); //
        }
        public record ActorState(int IntValue);
        public class ActorBehaviour : IActorBehavior<ActorState>
        {
            public ActorState Handle(MessageEnvelop envelop, ActorState state)
            {
                switch (envelop.Message)
                {
                    case AddValue message:
                        return Handle(state, message);
                }
                return state;
            }

            private ActorState Handle(ActorState state, AddValue message)
            {
                return new ActorState(state.IntValue + message.IntValue);
            }

        }
        public record AddValue(int IntValue);
    }
}
