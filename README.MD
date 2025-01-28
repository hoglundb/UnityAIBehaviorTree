# Deer AI Behavior System

This guide explains the Deer AI system, which utilizes a behavior tree and 
blackboard to create dynamic, responsive behavior. The system is modular, extensible, and supports 
behaviors such as wandering, fleeing, and reacting to stimuli. This code is designed to be used within the Unity Game Engine,
but the main idea presented here can be applied to any AI system.

Note: This system is designed to be configured with Unity's `Animation Controller` and `NavMesh` components. 
However, the data structure presented here can easily be adapted to work with the specific 
configuration of your project.

### Demonstration Video

Here is a demo video of the behavior tree system in action inside a Unity3D project: [YouTube video](https://youtu.be/49WsIcEQUdc)

---

## Table of Contents
1. [AI Functionality](#ai-functionality)
2. [Behavior Tree](#behavior-tree)
    - [Nodes](#nodes)
    - [Behavior Tree Class](#behavior-tree-class)
3. [Strategies](#strategies)
    - [IStrategy Interface](#istrategy-interface)
    - [Action Strategy](#action-strategy)
    - [Condition Strategy](#condition-strategy)
4. [Blackboard System](#blackboard-system)
    - [Blackboard Structure](#blackboard-structure)
    - [Managing The Blackboard](#arbiter-and-blackboard-experts)
5. [Integrating AI Behavior](#integrating-ai-behavior)
6. [Conclusion](#conclusion)

---


## AI Functionality

The Deer AI system supports the following behaviors:
- **Wandering**: Randomly selects a destination to wander to.
- **Eating**: Stops to eat occasionally.
- **Idling**: Pauses without performing any action.
- **Startled**: Reacts to sudden noises or visual threats.
- **Fleeing**: Runs away from danger until a safe distance is reached.

---


## Behavior Tree

The behavior tree is the core of the AI, enabling decision-making by evaluating conditions and executing actions in a structured hierarchy.

### Nodes

**Nodes** are the smallest unit of the behavior tree. They can be:
1. **Condition Nodes**: Evaluate specific conditions.
2. **Action Nodes**: Execute predefined actions.
3. **Composite Nodes**: Combine multiple child nodes into sequences or selectors.

#### Node Definition
```csharp
public class Node
{
    public readonly string name;
    public readonly int priority;
    public readonly List<Node> children;
    protected int currentChildIndex;

    public Node(string name = "NewNode", int priority = 0)
    {
        this.name = name;
        this.priority = priority;
        children = new();
    }

    public virtual void AddChild(Node childNode) 
        => children.Add(childNode);

    public virtual NodeStatus Tick() => children[currentChildIndex].Tick();

    public virtual void Reset()
    {
        currentChildIndex = 0;
        foreach (var child in children) child.Reset();
    }
}
```

### Behavior Tree Class

The `BehaviorTree` class serves as the core of the AI system, managing decision-making and prioritizing behaviors. It inherits from the `Node` base class and handles evaluating child nodes in sequence, looping through them if necessary.

---

### Key Features

#### 1. **Looping Behavior**
The `BehaviorTree` can optionally loop, allowing it to continuously re-evaluate its child nodes:
```csharp
private readonly bool loop;
public BehaviorTree(string name, bool shouldLoop = false) : base(name) => loop = shouldLoop;

```

## Strategies
### IStrategy Interface

The `IStrategy` interface defines reusable behavior logic for AI nodes. By encapsulating logic into individual strategies, we create a flexible and modular way to define AI actions and conditions.

---


```csharp
public interface IStrategy
{
    NodeStatus Tick();
    void ResetToDefault();
}
```

### Action Strategy
This is a strategy that we can re-use quite a bit. Note: we optimize the flexability of this component by passing it a delegate/action to be executed, rather than 
performing the action logic directly.   

```csharp
 public class ActionStrategy : IStrategy
    {
        private Action action;
        private bool hasExecutedAction = false;

        public ActionStrategy(Action action) => this.action = action;

        public NodeStatus Tick()
        {
            if (!hasExecutedAction)
            {
                action(); // Start the action
                hasExecutedAction = true;
                return NodeStatus.RUNNING;
            }

            // Action is now considered complete after one frame
            return NodeStatus.SUCCESS;
        }

        public void ResetToDefault() => hasExecutedAction = false;
    }
```

### Condition Strategy
Another important strategy we need is a condition. This is critical for setting up the behavior tree logic. 

```csharp
public class Condition : IStrategy
{
    private readonly Func<bool> predicate;

    public Condition(Func<bool> predicate) => this.predicate = predicate;

    public NodeStatus Tick() => predicate() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;

    public void ResetToDefault() {  }
}
```
###

## Blackboard System
We need to create a blackboard system so that our AI agents can dynamically share and access critical information about the game world and their current state. 
This allows for more flexible and reactive behaviors as the game progresses. Important to note that we use dictionaries here since the blackboard can be queried every frame. 
This script will be attached to the animal game object.

### Blackboard Structure

```csharp
 public class Blackboard : MonoBehaviour 
{
    [Space, Header("Dictionaries For Data Types")]
    [SerializeField] private SerializableDictionary<string, bool> boolDictionary = new();
    [SerializeField] private SerializableDictionary<string, int> intDictionary = new();
    [SerializeField] private SerializableDictionary<string,float> floatDictionary = new();
    [SerializeField] private SerializableDictionary<string, Vector3> vector3Dictionary = new();

    public List<Action> PassedActions { get; } = new();

    private readonly Arbiter arbiter = new();

    public void AddAction(Action action)
    {
        if (action != null)
            PassedActions.Add(action);
    }

    public void ClearActions() => PassedActions.Clear();

    // ... Other methods and utilities  
        
```

We also provide methods for adding keys and getting values. We do this for all the relavent data types we might need.

```csharp

  public bool TryGetBool(string key, out bool value) 
            => TryGetValueFromDictionary(boolDictionary, key, out value);

  public void SetBool(string key, bool value) 
            => SetValueInDictionary(boolDictionary, key, value);  

  // ... Other methods and utilities    

```

### Arbiter and Blackboard Experts
To manage the blackboard, we will be using a arbiter/expert structure. Experts will be components for desision making, while the arbiter determines which experts request has the highest priority. 

First lets define an IExpert interface.
```csharp
public interface IExpert
{
    int GetImportance(Blackboard blackboard);

    void Execute(Blackboard blackboard);
}
```

With each Expert determining it's own importance, the Arbiter's job is to execute the most important one. 

```csharp
public class Arbiter : MonoBehaviour
{
    readonly List<IExpert> experts = new();

    public void RegisterExpert(IExpert expert) => experts.Add(expert);

    public List<Action> EvaluateBlackboard(Blackboard blackboard)
    {
        IExpert bestExpert = null;
        int highestAssistance = 0;

        foreach(var expert in experts)
        {
            var insistance = expert.GetImportance(blackboard);
            if(insistance > highestAssistance)
            {
                highestAssistance = insistance;
                bestExpert = expert;
            }
        }

        bestExpert?.Execute(blackboard);

        var actions = blackboard.PassedActions;
        blackboard.ClearActions();

        return actions;
    }
}
```
<br>
Now we can make a few expert components for the animal's vision and hearing perception. 

```csharp
public class HearingExpert : MonoBehaviour, IExpert
{
   // ... Methods and utilities omitted to save space
}


public class PerceptionExpert : MonoBehaviour, IExpert
{
    // ... Methods and utilities omitted to save space
}
```

## Integrating AI Behavior
Finally, we are ready to set up the AI behavior controller. We attach the `AnimalBehaviorController.cs` script to the Animal game object.

```csharp
public class AnimalBehaviorController : MonoBehaviour
{
    // ... Serialized fields

    private void Awake()
    {
        // Initialize the blackboard
        references.Blackboard.SetInt(key_ThreatHeardAmount, 0);
        references.Blackboard.SetBool(key_IsReadyToDie, false);
        references.Blackboard.SetBool(key_isThreatSpotted, false);
        references.Blackboard.SetVector3(key_ThreatPosition, Vector3.zero);

        // Setup the behavior tree
        tree = new BehaviorTree(name + "DeerBehavior", true);
        var actions = new PrioritySelector("DeerLogic");
        actions.AddChild(CreateDeathSequence());
        actions.AddChild(CreateFleeSequence());
        actions.AddChild(CreateHearingSequence());
        actions.AddChild(CreateRandomActionSelector());
        tree.AddChild(actions);
    }
    
     // ... Other methods and utilities    
```
<br>

Then, each frame in Update the tree is evaluated (top to bottom & left to right)
``` csharp
 private void Update() => tree.Tick();
 ```
<br>

## Conclusion

This behavior tree implementation is designed to be modular, scalable, and easy to extend, making it a powerful tool for creating complex AI behaviors in Unity. Whether you're working on AI for zombies, wildlife, or other NPCs, this system provides a robust foundation to handle various scenarios with flexibility.

Feel free to explore and modify the code to suit your project needs. If you have any questions, feedback, or ideas for improvement, don’t hesitate to reach out or open an issue in this repository.

Thank you for checking out this project, and I hope it proves valuable in your game development journey!
