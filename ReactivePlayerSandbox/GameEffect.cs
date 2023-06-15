using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactivePlayerSandbox
{
    public enum EffectType { DD, DoT, Term}

    public abstract class Effect
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string TargetAttribute { get; set; }
        public int Value { get; set; }
        public string ValueType { get; set; } //e.g. "Frost" or "Holy"

        public Effect(CoreAttributes state, Func<CoreAttributes, int> formula)
        {
            this.Value = formula.Invoke(state);
        }
    }

    public class DDEffect: Effect
    {
        public DDEffect(CoreAttributes state, Func<CoreAttributes, int> formula): base(state, formula)
        {
        }
    }
    public class DoTEffect: Effect
    {
        public int NumberOfTicks { get; set; }
        public TimeSpan TickDuration { get; set; }

        public DoTEffect(CoreAttributes state, Func<CoreAttributes, int> formula) : base(state, formula)
        {
        }
    }

    public class TermEffect: Effect
    {
        public TimeSpan Duration { get; private set; }
        public TermEffect(CoreAttributes state, Func<CoreAttributes, int> formula, TimeSpan duration) : base(state, formula)
        {
            Duration = duration;
        }
    }

    public class GameEffectProxy
    {
        public IObservable<Tick> Ticks { get; }
        public Effect Effect { get; }
        public GameEffectProxy(Effect effect, IList<Effect> list)
        {
            var tt = new SourceCache<Effect, string>(x => x.Name); // sample
            
            Ticks = effect switch
            {
                DDEffect dd => Observable.Return(new Tick { }).Finally(() => list.Remove(Effect)), // sample
                DoTEffect dot => Observable.Interval(dot.TickDuration).Take(dot.NumberOfTicks).Select(x => new Tick { }),
                TermEffect term => Observable.Timer(term.Duration).Select(x => new Tick { }).StartWith(new Tick { }),
                _ => throw new ArgumentNullException("Effect cannot be null")
            };
            Effect = effect;
        }
        
    }
}
