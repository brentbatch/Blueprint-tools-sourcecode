using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Advanced_Blueprint_Tools
{
    class BlueprintOptimizer
    {
        public static dynamic optimizeblueprint(dynamic blueprint)
        {
            dynamic optimizedblueprint = new JObject();







            return optimizedblueprint;
        }

        public static dynamic CreateBlueprintFromPoints(List<Point3D> pointlist)
        {
            dynamic blueprint = new JObject();
            blueprint.version = 1;
            blueprint.bodies = new JArray();
            blueprint.bodies.Add(new JObject());
            blueprint.bodies[0].childs = new JArray();

            




            return blueprint;
        }
    }
}
