# anvil-csharp
CSharp Core Library for Anvil


I’ve split the framework into two assemblies for now. The root core of it being pure CSharp so it can be used in console applications, server side stuff, whatever. Then a Unity core that relies on the CSharp core. There will eventually be other Unity ones similar to how we had the UI and 2D Toolkit modules at Karman. Each assembly has an asmdef file so it’s actually compiled as an assembly which is super cool and let’s internal actually be enforced. 

The UpdateHandle is similar to the old way we did things except it’s not Unity specific anymore.
You create one nice and easy via:  m_UpdateHandle = UpdateHandle.Create<UnityUpdateSource>();

You can pass in (and extend your own) AbstractUpdateSource
So in pure CSharp you could have a TimerUpdateSource or ThreadUpdateSource or whatever you want.
For Unity I have a UnityUpdateSource, a UnityLateUpdateSourceand a UnityFixedUpdateSource
We used to have one monobehaviour that implemented all three of those Unity functions and dispatched events where in reality we rarely ever used anything but the Update one.
So now, when you call UpdateHandle.Create<UnityUpdateSource>();it will lazy instantiate ONLY ONE monobehaviour that in this case only implements just the Updatefunction.
Any myUpdateHandle.OnUpdate += HandleOnUpdate is piped from that MonoBehaviour’s Update because of the source. Similarly if we also wanted FixedUpdate, you’d have a separate UpdateHandle.
m_FixedUpdateHandle = UpdateHandle.Create<UnityFixedUpdateSource>(); and any OnUpdate  event from it would be piped from the MonoBehaviour’s FixedUpdate function.

You can create any number of UpdateHandles with the same source and there will only ever be the one “Source” object and all the UpdateHandles subscribe to it.
I did look into the override functionality of the += and -= to be able to have an UpdateHandle only listen to the “Source” if someone else is listening to that Handle. Otherwise it’s dormant.

Next the call later functionality. Similar concept in that I’m allowing the user to specify their own behavior if they want to.
m_UpdateHandle.CallLater(UnityFramesCallLaterHandle.Create(10, HandleOnDelayFrames));
Before we had CallLaterFrames or CallLaterSeconds as methods on the UpdateHandle, and then I think if I recall we realized we also needed a flag to support unscaled time or regular unity time. But maybe you need Milliseconds or maybe you need to get your time from the system clock or maybe from pinging a server.
So instead you have your own CallLaterHandle that extends off of an AbstractCallLaterHandle
So you can then have:
UnityFramesCallLaterHandle for delaying by a number of Unity Frames.
UnityDeltaTimeCallLaterHandle for delaying by seconds using Time.deltaTime
UnityUnscaledDeltaTimeCallLaterHandle  for delaying by seconds using Time.unscaledDeltaTime

Since you’re using a CallLaterHandle on an existing UpdateHandle, the UpdateSource is important.
Using something like UnityFixedDeltaTimeCallLaterHandle  on an UpdateHandle configured with a UnityLateUpdateSource would lead to bad results. Or results you don’t expect anyway, so there is a validation phase that lets you specify in your custom CallLaterHandle which UpdateSources are valid.

