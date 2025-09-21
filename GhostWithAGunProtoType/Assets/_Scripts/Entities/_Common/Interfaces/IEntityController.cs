using UnityEngine;

public interface IEntityController
{
    /// <summary>
    /// Called when the entity first comes to life. An entity controller has to be setup by another entity controller.
    /// </summary>
    void Setup(IEntityController controller);
    /// <summary>
    /// Called to activate a deactivated entity.
    /// </summary>
    void Activate();
    /// <summary>
    /// Deactivates the entity. Used to stop the entity temporarily with the intent of activating it again later.
    /// </summary>
    void Deactivate();
    /// <summary>
    /// Called when the entity is no longer needed.
    /// </summary>
    void Dispose();
}
