using TCustomAttributes;
using UnityEngine;

public class EnemyComponentsController : EntityComponentsController
{
    [field: SerializeField, OnSelf] public EnemyMovement Movement { get; private set; }

    public override void Setup(IEntityController controller)
    {
        base.Setup(controller);

    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
