# ActorModel.NET
A Simple & Lightweight Actor Model for .NET

Properties:
- Isolated State
- Support Persistance Layer
- Support Sleep & Restore


## How to use:

Define Actor State and Behavior:
```C#
// Actor Definition
public class MyActor : IActor<ActorState>
{
	// Initial value
    public ActorState InitialState() => new ActorState(4);
	
	// Behaviour Factory
    public IActorBehavior<ActorState> Behaviour() => new ActorBehaviour();
}

// Actor State
public record ActorState(int IntValue);

// Actor Behaviour
public class ActorBehaviour : IActorBehavior<ActorState>
{
    public ActorState Handle(MessageEnvelop envelop, ActorState state)
    {
        switch (envelop.Message)
        {
            case AddValue message:
                return new ActorState(state.IntValue + message.IntValue);
			case Ping message:
				envelop.Responde(new Pong(state.IntValue));
                return state;
        }
        return state;
    }
}

```


Create ``ActorSystem``:
```C#
// Create Instance of ActorSystem
using var actorSystem = new ActorSystem(logger, persistence, configuration);

// Register Actors
actorSystem.Register<MyActort1>();
actorSystem.Register(new MyActor2(...));
```

Create (Spawn) Actor / Send message / Get sate:
```C#
// Spawn Actor
var actor1 = actorSystem.Spawn<ActorTest, ActorState>(new GuidActorIdentity(Guid.NewGuid()));

// Send message
actor1.Send(new AddValue(13));

// Get Sate of Actor
var state1 = await actor1.GetState(m => m.IntValue);

// Get / Restore Actor
var actor1 = actorSystem.Get<ActorTest, ActorState>(identity)
```