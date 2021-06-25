Game Events are a simple way to define your game's events in your asset folder. An event can be any game system action that you might want to use as a trigger for other systems.

In our example here we use events havily (more so than you might if you wherent showing off the feature) to manage all of our major UI events and this allows an event triggered in the main.scene to trigger an action in the title.scene (see the Close Lesser Dialogs Game Event)

This also allows us to centralize event handling either for all events or groups events ... such as how we centralized the handling of title scene events in a single behaviour.

We have also demonstrated the creation of a custom Game Event type and Game Event Listener ... see the ulong game event for more details.