using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct MaterialFieldData
{
    public FieldInfo field;

    public UniformAttribute uniformAttribute;

    public MaterialFieldData(FieldInfo field, UniformAttribute uniformInfo)
    {
        this.field = field;
        this.uniformAttribute = uniformInfo;
    }
}