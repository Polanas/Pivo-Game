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

record ContactEventData(ContactListenerEvent ContactEvent, PhysicsBodyType PhysicsUserData1, PhysicsBodyType PhysicsUserData2);

class ContactListener : IContactListener
{
    private Dictionary<ContactEventData, bool> _onBeginEvents = new();

    private Dictionary<ContactEventData, bool> _onEndEvents = new();

    private List<ContactEventData> _onBeginEventsList = new();

    private List<ContactEventData> _onEndEventsList = new();

    //private List<(PhysicsBodyType, PhysicsBodyType)> _onBeginEventsData = new();

    //private List<(PhysicsBodyType, PhysicsBodyType)> _onEndEventsData = new();

    public void AddBeginAndEndEvent(ContactListenerEvent beginEvent, ContactListenerEvent endEvent, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2, bool deletable)
    {
        AddBeginEventInternal(beginEvent, physicsUserData1, physicsUserData2, deletable);
        AddEndEventInternal(endEvent, physicsUserData1, physicsUserData2, deletable);
    }

    public void AddBeginEvent(ContactListenerEvent beginEvent, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2, bool deletable)
    {
        AddBeginEventInternal(beginEvent, physicsUserData1, physicsUserData2, deletable);
    }

    public void AddEndEvent(ContactListenerEvent endEvent, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2, bool deletable)
    {
        AddEndEventInternal(endEvent, physicsUserData1, physicsUserData2, deletable);
    }

    public void ClearEvents()
    {
        for (int i = 0; i < _onBeginEventsList.Count; i++)
        {
            var beginEvent = _onBeginEventsList[i];
            var eventData = new ContactEventData(beginEvent.ContactEvent, beginEvent.PhysicsUserData1, beginEvent.PhysicsUserData1);

            bool deletable = _onBeginEvents[eventData];

            if (deletable)
            {
                _onBeginEvents.Remove(eventData);
                _onBeginEventsList.RemoveAt(i);
            }
        }

        for (int i = 0; i < _onEndEvents.Count; i++)
        {
            var endEvent = _onEndEventsList[i];
            var eventData = new ContactEventData(endEvent.ContactEvent, endEvent.PhysicsUserData1, endEvent.PhysicsUserData1);

            bool deletable = _onEndEvents[eventData];

            if (deletable)
            {
                _onEndEvents.Remove(eventData);
                _onEndEventsList.RemoveAt(i);
            }
        }
    }

    public void BeginContact(Contact contact)
    {
        var fixture1 = contact.FixtureA;
        var fixture2 = contact.FixtureB;

        for (int i = 0; i < _onBeginEvents.Count; i++)
        {
            var matches = UserDataMatches(((PhysicsUserData)fixture1.UserData).bodyType, ((PhysicsUserData)fixture2.UserData).bodyType,
                                         _onBeginEventsList[i].PhysicsUserData1, _onBeginEventsList[i].PhysicsUserData2);

            if (!matches)
                continue;

            bool areInRightOrder = ((PhysicsUserData)fixture1.UserData).bodyType == _onBeginEventsList[i].PhysicsUserData1;
            _onBeginEventsList[i].ContactEvent.Invoke(areInRightOrder ? fixture1.Body : fixture2.Body, areInRightOrder ? fixture2.Body : fixture1.Body);
        }
    }

    public void EndContact(Contact contact)
    {
        var fixture1 = contact.FixtureA;
        var fixture2 = contact.FixtureB;

        for (int i = 0; i < _onEndEvents.Count; i++)
        {
            var matches = UserDataMatches(((PhysicsUserData)fixture1.UserData).bodyType, ((PhysicsUserData)fixture2.UserData).bodyType,
                                         _onEndEventsList[i].PhysicsUserData1, _onEndEventsList[i].PhysicsUserData2);

            if (!matches)
                continue;

            bool areInRightOrder = ((PhysicsUserData)fixture1.UserData).bodyType == _onEndEventsList[i].PhysicsUserData1;
            _onEndEventsList[i].ContactEvent.Invoke(areInRightOrder ? fixture1.Body : fixture2.Body, areInRightOrder ? fixture2.Body : fixture1.Body);
        }
    }

    private void AddBeginEventInternal(ContactListenerEvent contactEvent, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2, bool deletable)
    {
        var contactEventData = new ContactEventData(contactEvent, physicsUserData1, physicsUserData2);

        if (!_onBeginEvents.ContainsKey(contactEventData))
        {
            _onBeginEvents.Add(contactEventData, deletable);
            _onBeginEventsList.Add(contactEventData);
        }
    }

    private void AddEndEventInternal(ContactListenerEvent contactEvent, PhysicsBodyType physicsUserData1, PhysicsBodyType physicsUserData2, bool deletable)
    {
        var contactEventData = new ContactEventData(contactEvent, physicsUserData1, physicsUserData2);

        if (!_onEndEvents.ContainsKey(contactEventData))
        {
            _onEndEvents.Add(contactEventData, deletable);
            _onEndEventsList.Add(contactEventData);
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
