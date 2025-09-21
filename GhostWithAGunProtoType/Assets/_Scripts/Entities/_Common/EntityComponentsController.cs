using System.Collections.Generic;
using UnityEngine;

public abstract class EntityComponentsController : MonoBehaviour, IEntityController
{
    protected List<IEntityController> _controllers = new List<IEntityController>();

    private void Start()
    {
        Setup(null);
    }

    public virtual void Setup(IEntityController controller)
    {
        _controllers.ForEach(c => c.Setup(this));
    }

    public virtual void Dispose()
    {
        _controllers.ForEach(c => c.Dispose());
    }

    public virtual void Activate()
    {
        _controllers.ForEach(c => c.Activate());
    }

    public virtual void Deactivate()
    {
        _controllers.ForEach(c => c.Deactivate());
    }


    protected void AddComponentToList<T>(T controller) where T : IEntityController
    {
        if (controller == null)
        {
            Debug.LogError("Controller is null");
            return;
        }

        _controllers.Add(controller);
    }

    public T GetUnitComponent<T>() where T : IEntityController
    {
        foreach (IEntityController controller in _controllers)
        {
            if (controller is T t)
            {
                return t;
            }
        }

        Debug.Log($"No {typeof(T).Name} in components of {gameObject.name}");
        return default;
    }

    public bool TryGetUnitComponent<T>(out T component) where T : class, IEntityController
    {
        foreach (IEntityController controller in _controllers)
        {
            if (controller is T matched)
            {
                component = matched;
                return true;
            }
        }

        component = default;
        return false;
    }
}
