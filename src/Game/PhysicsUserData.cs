using Leopotam.EcsLite.Di;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class PhysicsUserData
{
    public EcsPackedEntity entity;

    public PhysicsBodyType bodyType;

    public PhysicsUserData(PhysicsBodyType userData, EcsPackedEntity? entity = null)
    {
        if (entity != null)
            this.entity = entity.Value;

        bodyType = userData;
    }
}
