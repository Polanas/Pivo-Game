namespace Game;

class ContactListenerEventHandler
{
    public readonly ContactEvent onBeginEvent;

    public readonly ContactEvent onEndEvent;

    public readonly PhysicsBodyType userData1;

    public readonly PhysicsBodyType userData2;

    public ContactListenerEventHandler(ContactEvent onBeginEvent, ContactEvent onEndEvent, PhysicsBodyType userData1, PhysicsBodyType userData2)
    {
        this.onBeginEvent = onBeginEvent;
        this.onEndEvent = onEndEvent;
        this.userData1 = userData1;
        this.userData2 = userData2;
    }
}
