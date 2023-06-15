using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactivePlayerSandbox.Tests
{
    public class Initial
    {
        [Fact]
        public void TestOfGroupings()
        {
            var sourceL = new SourceList<string>();
            var effect2 = new ReactiveEffect("Effect2", EffectLogic.Additive, 0.3);
            effect2.ConnectToTargetObservable(sourceL.AsObservableList());

            var effects = new List<ReactiveEffect>
            {
                new ReactiveEffect("Effect1", EffectLogic.Additive, 0.2),
                effect2,
                new ReactiveEffect("Effect3", EffectLogic.Multiplicative, 2.0)
            };
            using (var cache = new SourceCache<IReactiveEffect, string>(e => e.Id))
            {
                cache.AddOrUpdate(effects);
                var shared = cache.Connect().Publish();

                //shared.MergeMany(x => x.ValueObs).Scan(1.0, (x,i) => x += i).Subscribe(x => x); // Nice!

                IObservableList<EffectGrouping> _mods;
                var another = shared
                    .AutoRefresh(x => x.Value)
                    .Group(x => x.Logic)
                    .Transform(grp => new EffectGrouping(grp))
                    .BindToObservableList(out _mods)
                    .DisposeMany()
                    .Subscribe();

                shared.Connect();

                Assert.Equal(0.5, _mods.Items.SingleOrDefault(x => x.Mod.Logic == EffectLogic.Additive).Mod.Result, 0.1);
                Assert.Equal(2, _mods.Count);

                effects[0].Value = 0.6;
                cache.RemoveKey("Effect3");

                Assert.Equal(0.9, _mods.Items.SingleOrDefault(x => x.Mod.Logic == EffectLogic.Additive).Mod.Result, 0.1);
                Assert.Equal(1, _mods.Count);

                sourceL.Add("SomeEffect");
                Assert.Equal(0.9, _mods.Items.SingleOrDefault(x => x.Mod.Logic == EffectLogic.Additive).Mod.Result, 0.1);

                sourceL.Add("Frozen");
                Assert.Equal(1.1, _mods.Items.SingleOrDefault(x => x.Mod.Logic == EffectLogic.Additive).Mod.Result, 0.1);

                sourceL.Clear();
                Assert.Equal(0.9, _mods.Items.SingleOrDefault(x => x.Mod.Logic == EffectLogic.Additive).Mod.Result, 0.1);
            }
        }
    }
}