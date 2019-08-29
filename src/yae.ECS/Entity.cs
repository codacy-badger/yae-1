using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace yae.ECS
{
    public abstract class Entity<TEntity>
        where TEntity : Entity<TEntity>
    {
        public Guid Id { get; }
        //private Context _ctx;

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        public Release Insert<T>(out T value) where T : class, new()
        {
            value = ComponentStorage<TEntity, T>.Add((TEntity) this);
            return new Release();
        }

        public Release Update<T>(out T component) where T : class, new()
        {
            ComponentStorage<TEntity, T>.Update((TEntity) this, out component);
            return new Release();
        }

        public void Remove<T>() where T : class, new()
        {
            ComponentStorage<TEntity, T>.Remove((TEntity)this);
        }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public ref struct Release
    {
        public void Dispose()
        {
        }
    }

    public class Operation
    {
        //store context there
        private static Operation CreateOperation<T>(OperationKind kind, T component, Destination dst)
        {
            return new Operation<T>(kind, component, dst);
        }

        public static Operation Add<T>(T component, Destination dst = Destination.Source)
        {
            return CreateOperation(OperationKind.Insert, component, dst);
        }

        public static Operation Remove<T>(Destination dst = Destination.Source)
        {
            return CreateOperation<T>(OperationKind.Delete, default, dst);
        }
    }

    public class Operation<T> : Operation
    {
        public Operation(OperationKind kind, T component, Destination dst)
        {
            //may we use something to alert right systems
            //here we can act on operation and throws the flow..exactly same pattern for our IEnumerable on handlers!
        }
    }

    public interface ISystem<in TSource, in TDestination>
    {
        IEnumerable<Operation> Execute(TSource source, TDestination destination);
    }
}