using Box2DSharp.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DSharp.Dynamics;
using Box2DSharp.Collision.Collider;

namespace Game;

delegate void ContactListenerEvent(Body body1, Body body2);

record ContactEvent(ContactListenerEvent EventContactListener, PhysicsBodyType PhysicsUserData1, PhysicsBodyType PhysicsUserData2);

class ContactListener : IContactListener
{
    private List<ContactEvent> _onBeginEvents = new();

    private List<ContactEvent> _onEndEvents = new();

    private List<(PhysicsBodyType, PhysicsBodyType)> _onBeginEventsData = new();

    private List<(PhysicsBodyType, PhysicsBodyType)> _onEndEventsData = new();

    public bool BeginEventExists(PhysicsBodyType userData1, PhysicsBodyType userData2) => _onBeginEventsData.Contains((userData1, userData2));

    public bool EndEventExists(PhysicsBodyType userData1, PhysicsBodyType userData2) => _onEndEventsData.Contains((userData1, userData2));

    public bool BeginAndEndEventExists(PhysicsBodyType userData1, PhysicsBodyType userData2) => _onBeginEventsData.Contains((userData1, userData2));

    public ContactListenerEventHandler AddBeginAndEndEvent(ContactListenerEvent beginContactListener, ContactListenerEvent endContactListener, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2)
    {
        var beginContactEvent = new ContactEvent(beginContactListener, physicsUserData1, physicsUserData2);
        var endContactEvent = new ContactEvent(endContactListener, physicsUserData1, physicsUserData2);

        if (!_onBeginEventsData.Contains((physicsUserData1, physicsUserData2)))
        {
            _onBeginEvents.Add(beginContactEvent);
            _onEndEvents.Add(endContactEvent);
        }

        var handler = new ContactListenerEventHandler(beginContactEvent, endContactEvent, physicsUserData1, physicsUserData2);
        return handler;
    }

    public ContactListenerEventHandler AddBeginEvent(ContactListenerEvent contactListener, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2)
    {
        var contactEvent = new ContactEvent(contactListener, physicsUserData1, physicsUserData2);

        if (!_onBeginEventsData.Contains((physicsUserData1, physicsUserData2)))
            _onBeginEvents.Add(contactEvent);

        var handler = new ContactListenerEventHandler(contactEvent, null, physicsUserData1, physicsUserData2);
        return handler;
    }

    public ContactListenerEventHandler AddEndEvent(ContactListenerEvent contactListener, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2)
    {
        var contactEvent = new ContactEvent(contactListener, physicsUserData1, physicsUserData2);

        if (!_onBeginEventsData.Contains((physicsUserData1, physicsUserData2)))
            _onEndEvents.Add(contactEvent);

        var handler = new ContactListenerEventHandler(null, contactEvent, physicsUserData1, physicsUserData2);

        return handler;
    }

    public void RemoveEndEvent(ContactListenerEventHandler contactListenerEventHandler)
    {
        _onEndEvents.Remove(contactListenerEventHandler.onEndEvent);
        _onEndEventsData.Remove((contactListenerEventHandler.userData1, contactListenerEventHandler.userData2));
    }

    public void RemoveBeginEvent(ContactListenerEventHandler contactListenerEventHandler)
    {
        _onBeginEvents.Remove(contactListenerEventHandler.onBeginEvent);
        _onBeginEventsData.Remove((contactListenerEventHandler.userData1, contactListenerEventHandler.userData2));
    }

    public void RemoveBeginAndEndEvent(ContactListenerEventHandler contactListenerEventHandler)
    {
        _onBeginEvents.Remove(contactListenerEventHandler.onBeginEvent);
        _onEndEvents.Remove(contactListenerEventHandler.onEndEvent);

        _onBeginEventsData.Remove((contactListenerEventHandler.userData1, contactListenerEventHandler.userData2));
        _onEndEventsData.Remove((contactListenerEventHandler.userData1, contactListenerEventHandler.userData2));

    }

    public void BeginContact(Contact contact)
    {
        var fixture1 = contact.FixtureA;
        var fixture2 = contact.FixtureB;

        for (int i = 0; i < _onBeginEvents.Count; i++)
        {
            var matches = UserDataMatches(((PhysicsUserData)fixture1.UserData).bodyType, ((PhysicsUserData)fixture2.UserData).bodyType,
                                         _onBeginEvents[i].PhysicsUserData1, _onBeginEvents[i].PhysicsUserData2);

            if (!matches)
                continue;

            _onBeginEvents[i].EventContactListener.Invoke(fixture2.Body, fixture1.Body);
        }
    }

    public void EndContact(Contact contact)
    {
        var fixture1 = contact.FixtureA;
        var fixture2 = contact.FixtureB;

        for (int i = 0; i < _onEndEvents.Count; i++)
        {
            var matches = UserDataMatches(((PhysicsUserData)fixture1.UserData).bodyType, ((PhysicsUserData)fixture2.UserData).bodyType,
                                         _onEndEvents[i].PhysicsUserData1, _onEndEvents[i].PhysicsUserData2);

            if (!matches)
                continue;

            _onEndEvents[i].EventContactListener.Invoke(fixture2.Body, fixture1.Body);
        }
    }

    private bool UserDataMatches(PhysicsBodyType fixture1Data, PhysicsBodyType fixture2Data,
                                 PhysicsBodyType savedData1, PhysicsBodyType savedData2)
    {
        return (savedData1 == fixture1Data && savedData2 == fixture2Data) ||
               (savedData2 == fixture1Data && savedData1 == fixture2Data);
    }

    public void PreSolve(Contact contact, in Manifold oldManifold) { }

    public void PostSolve(Contact contact, in ContactImpulse impulse) { }
}
