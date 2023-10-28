# ActorModel.NET
A lightweight Actor Model for .NET, which supports persistence, sleeping and on-demand restoring.

Properties:
- *Isolated State:* State kept outside of Actor definition
- *Support Persistence Layer:* The state would be persisted after any change in state by a defined delay
- *Support Sleep & Restore:* Actors will be automatically persisted and disposed after a defined period of inactivity to release the RAM. Then, they will be automatically restored from the persistence layer on demand.


## How to use

Define Actor Definition (State and Behavior) by implementing the interface ``IActor<TState>``.<br />
In actor definition, you need to declare:
- State Type ``TState``
- State initial value (by implementing ``InitialState()``)
- Actor Behaviour (by implementing the interface ``IActorBehavior<TState>`` and its factory in ``Behaviour()``)
```C#
// Actor Definition
public class MyActor : IActor<ActorState>
{
    // State Initial value
    public ActorState InitialState() => new ActorState(4);
    
    // Behaviour Factory
    public IActorBehavior<ActorState> Behaviour() => new ActorBehaviour();
}

// Actor State
public record ActorState(int IntValue);

// Actor Behaviour
public class ActorBehaviour : IActorBehavior<ActorState>
{
    public ActorState Handle(MessageEnvelope envelope, ActorState state)
    {
        switch (envelope.Message)
        {
            case AddValue message:
                return new ActorState(state.IntValue + message.IntValue);
            case Ping message:
                envelope.Responde(new Pong(state.IntValue));
                return state;
        }
        return state;
    }
}

```

Then, you need to create an ``ActorSystem`` instance and keep it as a singleton instance. You need to implement the ``IPersistence`` interface for the persistence layer. Also, you can customise the configuration by providing an ``ActorSystemConfiguration`` configuration class.
```C#
// Create Instance of ActorSystem
using var actorSystem = new ActorSystem(logger, persistence, configuration);
```

After creating ``ActorSystem`` instance and actor definitions, it's time to register Actor definitions in the system:
```C#
// Register Actors
actorSystem.Register<MyActort1>();
actorSystem.Register(new MyActor2(...));
```

Now, you can Create (Spawn) Actor / Send message / Get sate:
```C#
// Spawn (Create) and Actor
var actor1 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Guid.NewGuid()));

// Send a message to the actor
actor1.Send(new AddValue(13));

// Get Sate of the Actor
var state1 = await actor1.GetState(m => m.IntValue);

// Get / Restore the Actor
var actor1 = actorSystem.Get<ActorTest, ActorState>(identity);
```