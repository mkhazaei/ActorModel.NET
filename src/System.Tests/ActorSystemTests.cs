using ActorModelNet.Contracts;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Tests.Utilities;

namespace ActorModelNet.System.Tests;

public class ActorSystemTests
{
    [Fact]
    public async Task CheckDefaultValueVork()
    {
        var loggerMock = new Mock<ILogger<ActorSystem>>();
        var serviceProvider = new Mock<IServiceProvider>();
        using var actorSystem = new ActorSystem(loggerMock.Object, serviceProvider.Object);
        actorSystem.Register<ActorTest>();

        var actor1 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Utils.Out(Guid.NewGuid(), out var id1)));

        var state1 = await actor1.UnsafeGetState();
        Assert.Equal(0, state1.IntValue);
    }

    [Fact]
    public async Task CheckInitialize()
    {
        var loggerMock = new Mock<ILogger<ActorSystem>>();
        var serviceProvider = new Mock<IServiceProvider>();
        using var actorSystem = new ActorSystem(loggerMock.Object, serviceProvider.Object);
        actorSystem.Register<ActorTest>();

        var actor1 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Utils.Out(Guid.NewGuid(), out var id1)), new ActorState(7));

        var state1 = await actor1.UnsafeGetState();
        Assert.Equal(7, state1.IntValue);
    }


    [Fact]
    public async Task ChangeStateTest()
    {
        var loggerMock = new Mock<ILogger<ActorSystem>>();
        var serviceProvider = new Mock<IServiceProvider>();
        using var actorSystem = new ActorSystem(loggerMock.Object, serviceProvider.Object);
        actorSystem.Register<ActorTest>();

        var actor1 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Utils.Out(Guid.NewGuid(), out var id1)));
        var actor2 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Utils.Out(Guid.NewGuid(), out var id2)), new ActorState(10));

        actor1.Send(new AddValue(13));
        actor2.Send(new MinusValue(17));

        var state1 = await actor1.UnsafeGetState();
        var state2 = await actor2.UnsafeGetState();
        Assert.Equal(13, state1.IntValue);
        Assert.Equal(-7, state2.IntValue);
    }



    public class ActorTest : IActor<ActorState>
    {
        public ActorState InitialState() => new ActorState(0);
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

                case MinusValue message:
                    return Handle(state, message);

            }
            return state;
        }

        private ActorState Handle(ActorState state, AddValue message)
        {
            return new ActorState(state.IntValue + message.IntValue);
        }

        private ActorState Handle(ActorState state, MinusValue message)
        {
            return new ActorState(state.IntValue - message.IntValue);
        }

    }
    public record AddValue(int IntValue);
    public record MinusValue(int IntValue);

}