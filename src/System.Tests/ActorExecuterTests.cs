﻿using ActorModelNet.Contracts;
using ActorModelNet.System;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Tests.Utilities;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.System.Tests
{
    
    public class ActorExecuterTests
    {
        [Fact]
        public async Task CheckDefaultValueWork() 
        {
            var loggerMock = new Mock<ILogger>();
            var actor = new ActorTest();
            var actorExecuter = new ActorExecuter<ActorState>(new GuidActorIdentity(Guid.NewGuid()), actor, loggerMock.Object);

            var state1 = await actorExecuter.UnsafeGetState();
            Assert.Equal(5, state1.IntValue);
        }

        [Fact]
        public async Task CheckInitialValueWork()
        {
            var loggerMock = new Mock<ILogger>();
            var actor = new ActorTest();
            var actorExecuter = new ActorExecuter<ActorState>(new GuidActorIdentity(Guid.NewGuid()), actor, loggerMock.Object, new ActorState(7));

            var state1 = await actorExecuter.UnsafeGetState();
            Assert.Equal(7, state1.IntValue);
        }

        [Fact]
        public async Task CheckMessageExecuted()
        {
            var loggerMock = new Mock<ILogger>();
            var actor = new ActorTest();
            var actorExecuter = new ActorExecuter<ActorState>(new GuidActorIdentity(Guid.NewGuid()), actor, loggerMock.Object, new ActorState(7));

            actorExecuter.Send(new AddValue(5));
            var state1 = await actorExecuter.UnsafeGetState();
            Assert.Equal(12, state1.IntValue);
        }

        [Fact]
        public async Task CheckMessageExecutedInOrder()
        {
            var loggerMock = new Mock<ILogger>();
            var actor = new ActorTest();
            var actorExecuter = new ActorExecuter<ActorState>(new GuidActorIdentity(Guid.NewGuid()), actor, loggerMock.Object, new ActorState(7));

            actorExecuter.Send(new AddValue(5));
            actorExecuter.Send(new MultiplyValue(-2));
            var state1 = await actorExecuter.UnsafeGetState();
            Assert.Equal(-24, state1.IntValue);
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

                    case MultiplyValue message:
                        return Handle(state, message);

                }
                return state;
            }

            private ActorState Handle(ActorState state, AddValue message)
            {
                return new ActorState(state.IntValue + message.IntValue);
            }

            private ActorState Handle(ActorState state, MultiplyValue message)
            {
                return new ActorState(state.IntValue * message.IntValue);
            }


        }
        public record AddValue(int IntValue);
        public record MultiplyValue(int IntValue);


    }
}
