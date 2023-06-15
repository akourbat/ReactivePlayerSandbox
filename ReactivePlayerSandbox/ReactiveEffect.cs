using DynamicData;
using DynamicData.Binding;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactivePlayerSandbox
{
    public enum EffectLogic { Additive, Multiplicative }

    public interface IReactiveEffect: INotifyPropertyChanged
    {
        string Id { get; }
        EffectLogic Logic { get; }
        double Value { get; set; }
        IObservable<double> ValueObs { get; }
        void ConnectToTargetObservable(IObservableList<string> list);
        void Dispose();
    }

    public class ReactiveEffect : AbstractNotifyPropertyChanged, IDisposable, IReactiveEffect
    {
        private CompositeDisposable _cleanUp;
        private ISourceList<string> _targetEffects;
        private double _initialValue;

        public ReactiveEffect(string id, EffectLogic logic, double value)
        {
            Id = id;
            Logic = logic;
            Value = value;
            _initialValue = value;
            _cleanUp= new CompositeDisposable();
            _targetEffects = new SourceList<string>();
            _targetEffects.Add("Frozen");
        }
        public void ConnectToTargetObservable(IObservableList<string> list)
        {
            var combined = new SourceList<IObservableList<string>>();
            combined.Add(_targetEffects.AsObservableList());
            combined.Add(list);
            _cleanUp.Add(combined
                .And()
                .ToCollection()
                .Subscribe(c => this.Value = c.Count > 0 ? _initialValue + 0.2 : _initialValue));

            //_cleanUp.Add(list.Connect()
            //    .Filter(x => x == "Frozen")
            //    .ToCollection()
            //    .Subscribe(c => this.Value = c.Count > 0 ? _initialValue + 0.2 : _initialValue));
        }
        public string Id { get; }

        public EffectLogic Logic { get; }

        private BehaviorSubject<double> _valueSubj = new BehaviorSubject<double>(0.0);

        public double Value
        {
            get => _valueSubj.Value;
            set
            {
                _valueSubj.OnNext(value);
                OnPropertyChanged();
            }
        }
        public IObservable<Func<string,string>> observable { get; } // just checking something
        public IObservable<double> ValueObs => _valueSubj.AsObservable();

        //private double _value;
        //public double Value
        //{
        //    get => _value;
        //    set => SetAndRaise(ref _value, value);
        //}
        public void Dispose() => _cleanUp?.Dispose();
    }
}
