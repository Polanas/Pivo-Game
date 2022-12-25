using Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class Coroutine
{
    public Func<IEnumerator> func;

    public CoroutineHandle handle;

    public Coroutine(Func<IEnumerator> func, CoroutineHandle handle)
    {
        this.func = func;
        this.handle = handle;
    }
}

/// <summary>
/// Based on Celeste's state machine (thank you very much :>)
/// </summary>
/// 
class StateMachine<T>
{
    private T _state;

    private bool _updateTwiceUponStateChange;

    private Dictionary<T, Action> _begins;

    private Dictionary<T, Action> _ends;

    private Dictionary<T, Coroutine> _coroutines;

    private Dictionary<T, Func<T>> _updates;

    private CoroutineRunner _coroutineRunner;

    public T PreviousState { get; private set; }

    public T State => _state;

    public CoroutineRunner CoroutineRunner => _coroutineRunner;

    public StateMachine(bool updateTwiceUponStateChange = false)
    {
        _updateTwiceUponStateChange = updateTwiceUponStateChange;

        _begins = new();
        _ends = new();
        _updates = new();
        _coroutines = new();

        _coroutineRunner = new();
    }

    public void SetCallBacks(T state, Func<T> onUpdate, Action begin = null, Action end = null, Func<IEnumerator> coroutine = null)
    {
        _begins[state] = begin;
        _ends[state] = end;
        _updates[state] = onUpdate;
        _coroutines[state] = new Coroutine(coroutine, default(CoroutineHandle));
    }

    public void ForceState(T newState)
    {
        if (EqualityComparer<T>.Default.Equals(_state, newState))
            return;

        _state = newState;

        _ends[PreviousState]?.Invoke();
        _begins[_state]?.Invoke();

        if (_coroutines[PreviousState].func != null)
            _coroutineRunner.Stop(_coroutines[PreviousState].handle);

        if (_coroutines[newState].func != null)
            _coroutines[newState].handle = _coroutineRunner.Run(_coroutines[newState].func.Invoke());
    }

    public void RunCoroutine(Func<IEnumerator> coroutne)
    {
        _coroutineRunner.Run(coroutne.Invoke());
    }

    public void Update(float deltaTime)
    {
        if (_coroutines[_state] != null)
            _coroutineRunner.Update(deltaTime);

        if (_updates[_state] != null)
            ForceState(_updates[_state].Invoke());

        if (!_updateTwiceUponStateChange)
            return;

        if (!EqualityComparer<T>.Default.Equals(PreviousState, _state))
        {
            _coroutineRunner.Update(deltaTime);
            ForceState(_updates[_state].Invoke());
        }

        PreviousState = _state;
    }
}