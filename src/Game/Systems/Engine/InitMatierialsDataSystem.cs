using System.Reflection;

namespace Game;

class InitMatierialsSystem : IEcsInitSystem
{
    public void Init(EcsSystems systems)
    {
        var sharedData = systems.GetShared<SharedData>();
        var materialsData = new MaterialsData() { materialUniformFields = new() };
        var materialUnifromFields = materialsData.materialUniformFields;

    var materialTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsAssignableTo(typeof(Material)) && t != typeof(Material)).ToList();

        foreach (var type in materialTypes)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<MaterialFieldData> materialInfo = new();

            for (int i = 0; i < fields.Length; i++)
            {
                if ((fields[i].GetCustomAttribute(typeof(UniformAttribute)) is var attrbute) && attrbute == null)
                    continue;

                materialInfo.Add(new MaterialFieldData(fields[i], (UniformAttribute)attrbute));
            }

            materialsData.materialUniformFields[type] = materialInfo;
        }

        sharedData.eventBus.NewEventSingleton<MaterialsData>() = materialsData;

        sharedData.renderData.toTextureRenderer.Init(materialsData.materialUniformFields);
    }
}
