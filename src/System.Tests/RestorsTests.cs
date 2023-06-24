using ActorModelNet.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.System.Tests
{
    public class RestorsTests
    {
        [Fact]
        public async Task CheckPersistenceCalledInInitializing()
        {
            var loggerMock = new Mock<ILogger<ActorSystem>>();
            var persistence = new Mock<IPersistence>();
            var identity = new GuidActorIdentity(Guid.NewGuid());
            persistence.Setup(m => m.Load<ActorState>(identity))
                .Returns(() => new ActorState(10));
            using var actorSystem = new ActorSystem(loggerMock.Object, persistence.Object);
            actorSystem.Register<ActorTest>();

            var actor1 = actorSystem.Get<ActorTest, ActorState>(identity);
            var state1 = await actor1.UnsafeGetState();
            Assert.Equal(10, state1.IntValue);
        }


        public class ActorTest : IActor<ActorState>
        {
            public ActorState InitialState() => new ActorState(5);
            public IActorBehavior<ActorState> Behaviour() => new ActorBehaviour(); //
        }
        public record ActorState(int IntValue);
        public class ActorBehaviour : IActorBehavior<ActorState>
        {
            public ActorState Handle(MessageEnvelope envelope, ActorState state)
            {
                return state;
            }
        }

    }
}
