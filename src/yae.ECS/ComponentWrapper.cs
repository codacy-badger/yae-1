namespace yae.ECS
{
    /// <summary>
    /// Avoid lookup to gets the entity that owns the component
    /// </summary>
    class ComponentWrapper<TEntity, TComponent>
    {
        public TComponent Component { get; set; }
        public TEntity Entity { get; set; }

        public ComponentWrapper(TComponent component, TEntity entity)
        {
            Component = component;
            Entity = entity;
        }
    }
}