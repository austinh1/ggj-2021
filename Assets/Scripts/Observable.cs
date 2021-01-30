using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
[ExecuteAlways]
public class Observable<T>
{
    [Serializable] public class Event : UnityEvent<T, T> { }
    [SerializeField] protected T _value;
    
    private ChangedArgs<T> _args;

    public virtual T Value
    {
        set
        {
            if (value != null ? value.Equals(_value) : _value == null)
                return;

            var previous = _value;
            _value = value;

            _args.Old = previous;
            _args.New = value;

            HandleEventPropagation(_args);
        }
        get => _value;
    }
    
    public Event OnChange = new Event();

    protected virtual void HandleEventPropagation(ChangedArgs<T> args)
    {
        OnChange.Invoke(args.Old, args.New);
    }
}

public struct ChangedArgs<T>
{
    public T Old { get; internal set; }
    public T New { get; internal set; }
}
