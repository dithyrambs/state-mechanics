# State Mechanics

## Overview

StateMechanics is a state machine that was designed specifically to fill the gap between easy-to-setup but unscalable method-based state machines, and the laborious-but-more-correct class based state machines. If only there was a way to quickly prototype adding new states without making new files or even needing to manually add the newly added states.StateMechanics solves all that by using inner classes and then auto-caching them into the state machine. And if your inner classes ever get too unwieldy, you can alway pull them into separate classes later.

## Setup

StateMechanics uses generics to auto-inject a reference of the outer-class into the inner-class. When defining your state machine, pass in the type of the outer class. Then pass in a reference to the class instance.

	StateMachine<StateMachineContainingClass> stateMachine;
	//then later
	stateMachine = new StateMachine<StateMachineContainingClass>(this);

Also make sure to tick the update loop if you need. In a game engine like Unity, you would do:

    private void Update()
    {
        stateMachine.Update();
    }

## Auto-Populating States From Inner Classes

Simply call this method, and any inner State child-class inside the type that’s passed in will be added to the state machine automatically.

    stateMachine.SetStatesFromInnerClasses<StateMachineContainingClass>();

That’s all you have to do! When you instruct the state machine to switch to a state, it should just work.


    stateMachine.SwitchTo<SomeState>();


## Manually Adding External Class States

If you need to, you could of course just manually add states, by type:

    stateMachine.AddState<SomeState>();

## Making States

Make sure that the type passed in to your state is the “mechanic” operating your state, of the same type that you injected when instantiating the state machine.

    public class SomeState : State<StateMachineContainingClass>
    {
    }

Once that’s setup, all the state machine basics are here; just override Enter, Update, Or Exit virtual methods.

    public override Enter()
    {
        //code that should only execute when entering a state
    }

Additionally, each state has an Init, and a CleanUp public virtual method for you to override. Init is called at the moment the state is added to the state machine; CleanUp only executes if you call CleanUp on the state machine itself. In Unity you might do something like:

    private void OnDestroy()
    {
        stateMachine.CleanUp();
    }

## The Mechanic

At the risk of getting too cute with nomenclature, this state machine employs the concept of a “mechanic”— the object that controls the state machines. Often this will be the outer-class that contains your inner-classes.

Why do all states have a ‘mechanic’ member? The answer is: it’s much easier to communicate across your states this way. Perhaps there’s a property or method they all need to access. And it’s always nice to have an alternative to having to solve this problem with inheritance. But the real reason why is that it makes it easier to prototype; I could have let users inject this managing-class reference on a case by case basis—but in my experience making state machines, I almost always needed it. In the interest of making a state machine that requires little effort and no boiler-plate to setup, this was a huge boon.

From within a state, you can access any property or method of a mechanic:

    mechanic.SomeMethod();

## Intra and Inter State Communication

The ‘mechanic’ paradigm is meant as a quick tool for prototyping, but isn’t in itself a complete solution. Consider using a blackboard system of some kind to pass information not just between your states, but between separate state machines. Additionally, if your states need to change based on external variables, perhaps consider some kind of messaging system that disseminates messages down to the currently active state. 

Good luck, and hope you find this state machine as useful as I have.
