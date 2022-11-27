using System;

namespace Game;

struct MaterialsData : IEventSingleton
{
    public Dictionary<Type, List<MaterialFieldData>> materialUniformFields;
}
