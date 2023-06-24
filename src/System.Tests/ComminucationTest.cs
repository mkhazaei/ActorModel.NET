using ActorModelNet.Contracts;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Tests.Utilities;
using static ActorModelNet.System.Tests.ActorSystemTests;

namespace ActorModelNet.System.Tests;

public class ComminucationTest
{

    [Fact]
    public async Task RequestResponseTest()
    {
        var loggerMock = new Mock<ILogger<ActorSystem>>();
        var serviceProvider = new Mock<IServiceProvider>();
        using var actorSystem = new ActorSystem(loggerMock.Object);
        actorSystem.Register<ActorTest>();

        var actor1 = actorSystem.Spawn<ActorTest, ActorState>(Utils.Out(new GuidActorIdentity(Guid.NewGuid()), out var id1));
        var actor2 = actorSystem.Spawn<ActorTest, ActorState>(Utils.Out(new GuidActorIdentity(Guid.NewGuid()), out var id2), new ActorState(13));

        actor1.Send(new GetValueFrom(id2, Utils.Out(new TaskCompletionSource(), out var tcs)));
        await tcs.Task;

        var state1 = await actor1.UnsafeGetState();
        var state2 = await actor2.UnsafeGetState();
        Assert.Equal(13, state1.IntValue);
        Assert.Equal(13, state2.IntValue);
    }


    public class ActorTest : IActor<ActorState>
    {
        public ActorState InitialState() => new ActorState(0);
        public IActorBehavior<ActorState> Behaviour() => new ActorBehaviour(); //
    }
    public record ActorState(int IntValue, TaskCompletionSource? TCS = null);
    public class ActorBehaviour : IActorBehavior<ActorState>
    {
        public ActorState Handle(MessageEnvelope envelope, ActorState state)
        {
            switch (envelope.Message)
            {

                case GetValueFrom message:
                    envelope.Send<ActorTest>(message.Target, new SendYourValue());
                    return new ActorState(state.IntValue, message.TCS);

                case SendYourValue message:
                    envelope.Respond(new RequestedValue(state.IntValue));
                    return state;

                case RequestedValue message:
                    state.TCS?.SetResult();
                    return new ActorState(message.IntValue);

            }
            return state;
        }
    }

    public record GetValueFrom(IActorIdentity Target, TaskCompletionSource TCS);
    public record SendYourValue();
    public record RequestedValue(int IntValue);
}