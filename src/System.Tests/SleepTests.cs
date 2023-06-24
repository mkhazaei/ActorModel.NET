using ActorModelNet.Contracts;
using ActorModelNet.Contracts.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.System.Tests
{
    public class SleepTests
    {
        [Fact]
        public async Task ChackActorSleeped()
        {
            var identity = new GuidActorIdentity(Guid.NewGuid());
            var configuration = new ActorSystemConfiguration() { PersistenceTimeoutInSecond = 1, SleepTimeoutInSecond = 3, SleepTimeoutSlidingWindowsInSecond = 2 };
            var persistence = new Mock<IPersistence>();
            persistence.Setup(m => m.Save<ActorState>(identity, It.IsAny<ActorState>())).Returns(() => true);
            var loggerMock = new Mock<ILogger<ActorSystem>>();
            var tcs = new TaskCompletionSource();
            _ = loggerMock.Setup(m => m.Log(It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Sleep : Actor/{identity} slept"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Callback(() => { tcs.SetResult(); });
            using var actorSystem = new ActorSystem(loggerMock.Object, persistence.Object, configuration);
            actorSystem.Register<ActorTest>();

            var actor1 = actorSystem.Spawn<ActorTest, ActorState>(identity);
            var watch = Stopwatch.StartNew();
            actor1.Send(new AddValue(7));

            await tcs.Task;
            watch.Stop();
            Assert.Equal(3, Math.Round(watch.Elapsed.TotalSeconds));
            loggerMock.VerifyLog(m => m.LogDebug($"Actor/{identity}: TrySleep()"), Times.Once());
            loggerMock.VerifyLog(m => m.LogDebug($"Actor/{identity}: Sleeping..."), Times.Once());
            loggerMock.VerifyLog(m => m.LogDebug($"Sleep : Actor/{identity} slept"), Times.Once());
        }
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
                    return new ActorState(state.IntValue + message.IntValue);
                case ActorRestoredMessage message:
                    return state;
            }
            return state;
        }


    }
    public record AddValue(int IntValue);
}
