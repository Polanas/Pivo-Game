using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Contacts;
using Box2DSharp.Collision.Collider;

namespace Game;

class MyContactListener : IContactListener
{

    public virtual void BeginContact(Contact contact) { }

    public virtual void EndContact(Contact contact) { }

    public virtual void PreSolve(Contact contact, in Manifold oldManifold) { }

    public virtual void PostSolve(Contact contact, in ContactImpulse impulse) { }
}
