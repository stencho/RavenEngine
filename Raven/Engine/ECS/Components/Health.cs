using System.Collections.Generic;
using System.Reflection;

namespace Raven.Engine.Components;

//PROPERTIES
[ComponentProperty("Health", typeof(float))]
public partial class HealthComponent : Component {
    public HealthComponent(float starting_health = 1.0f) {
        add_data("Health", starting_health);
    }
}