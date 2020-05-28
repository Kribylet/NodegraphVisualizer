using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nodegraph_Generator
{
    /*
    * Structure contains a collection of Component objects. It is meant to be used to contain an entire 3D model in the Nodegraph project.
    */
    public class Structure
    {

        private List<Component> _components;

        public Structure(){
            _components = new List<Component>();
            components = _components.AsReadOnly();
        }

        public int addComponent(Component comp){
            comp.index = _components.Count;
            _components.Add(comp);
            return _components.Count - 1;
        }

        public void removeComponent(int index){
            _components.RemoveAt(index);
        }
        
        public ReadOnlyCollection<Component> components{
            get; private set;
        }

    }
}
