using System;
using System.Collections.Generic;
using System.Text;

namespace yae.Sandbox
{
    public enum OperationKind
    {
        Add,
        Update,
        Remove //may we add new kind like Upsert or whatever
    }

    public enum Where //find a better name tho lmao
    {
        Source,
        Target
    }
    public class Operation
    {
        public static Operation CreateOperation<T>(OperationKind kind, T component, Where where)
        {
            return new Operation<T>(kind, component, where);
        }
    }

    public class Operation<T> : Operation
    {
        public Operation(OperationKind kind, T component, Where whereAct)
        {
            //here we can act on operation and throws the flow..exactly same pattern for our IEnumerable on handlers!
        }
    }

    interface ISystem<TSrc, TDst>
    {
        IEnumerable<Operation> Execute(TSrc src, TDst dst);
    }

    public class HealthComponent { }
    public class VelocityComponent { }
    public class BoostComponent { }

    class MyStstem : ISystem<HealthComponent, (VelocityComponent, BoostComponent)>
    {
        public IEnumerable<Operation> Execute(HealthComponent src, (VelocityComponent, BoostComponent) dst)
        {
            yield return Operation.CreateOperation(OperationKind.Add, new BoostComponent(), Where.Target);
        }
    }

    class ECS
    {
    }
}
