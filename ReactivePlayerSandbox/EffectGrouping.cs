using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

namespace ReactivePlayerSandbox
{
    public record EffectModifier(EffectLogic Logic, double Result);

    public class EffectGrouping : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _cleanUp;
        private EffectModifier _mod;
        public EffectModifier Mod
        {
            get => _mod;
            set => SetAndRaise(ref _mod, value);
        }
        public EffectGrouping(IGroup<IReactiveEffect, string, EffectLogic> effectGroup)
        {
            var key = effectGroup.Key;
            _cleanUp = effectGroup.Cache.Connect()
                .QueryWhenChanged(query =>
                {
                    var result = query.Items.Sum(x => x.Value);
                    return new EffectModifier(key, result);
                })
                .Subscribe(mods => Mod = mods);
        }
        public void Dispose() => _cleanUp?.Dispose();
    }
}

