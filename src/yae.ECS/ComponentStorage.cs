using System;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace yae.ECS
{
    internal static class ComponentStorage<TEntity, TComponent> where TComponent : class, new()
    {
        private static readonly ConcurrentDictionary<TEntity, ComponentWrapper<TEntity, TComponent>> Components 
            = new ConcurrentDictionary<TEntity, ComponentWrapper<TEntity, TComponent>>();

        private static readonly ObjectPool<TComponent> Pool 
            = new DefaultObjectPool<TComponent>(new DefaultPooledObjectPolicy<TComponent>());

        public static TComponent Add(TEntity entity)
        {
            var cmp = Pool.Get();
            if (Components.TryAdd(entity, new ComponentWrapper<TEntity, TComponent>(cmp, entity))) return cmp;
            Pool.Return(cmp);
            throw new InvalidOperationException("Something went wrong : Add");
        }

        public static bool Has(TEntity entity)
        {
            return Components.TryGetValue(entity, out var _);
        }

        public static Release Update(TEntity entity, out TComponent component)
        {
            //acquire the lock and more...
            if (!Components.TryGetValue(entity, out var cmp))
                throw new InvalidOperationException("Trying update a non existent component");
            component = cmp.Component;
            return new Release();

        }

        public static void Remove(TEntity entity)
        {
            if(!Components.TryRemove(entity, out var cmp))
                throw new InvalidOperationException("Something went wrong: Remove");

            Pool.Return(cmp.Component);
        }
    }
}