using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactivePlayerSandbox
{
    public class Tick
    {
        public string Source { get; set; }
        public string TargetAttribute { get; set; }
        public int Value { get; set; }
        public string ValueType { get; set; } //e.g. "Frost" or "Holy"
        public IList<string> Bypasses { get; set; } = new List<string>();

        public void Apply(CoreAttributes target)
        {
            target.Attributes[TargetAttribute] += Value; //simplistic
        }
    }

    public class CoreAttributes
    {
        public string PlayerId { get; private set; }
        public IDictionary<string, int> Attributes { get; private set; }

        public CoreAttributes(string id, IDictionary<string, int> attributes)
        {
            this.PlayerId = id;
            this.Attributes= attributes;
        }
        public void Apply(Tick tick) 
        {
            Attributes[tick.TargetAttribute] += tick.Value; // simplistic
        }
    }
}
