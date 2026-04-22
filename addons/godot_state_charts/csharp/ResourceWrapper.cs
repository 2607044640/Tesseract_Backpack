// ReSharper disable once CheckNamespace
namespace GodotStateCharts
{
    using Godot;
    
    /// Base class for all wrapper classes for Godot Resource types. Provides common functionality. Not to be used directly.
    public abstract class ResourceWrapper
    {
        /// The wrapped resource. Useful for you need to access the underlying resource directly,
        /// e.g. for serialization.
        public readonly Resource Wrapped;

        protected ResourceWrapper(Resource wrapped)
        {
            Wrapped = wrapped;
        }

        /// Allows to call methods on the wrapped resource deferred.
        public Variant CallDeferred(string method, params Variant[] args)
        {
            return Wrapped.CallDeferred(method, args);
        }

        /// Allows to call methods on the wrapped resource.
        public Variant Call(string method, params Variant[] args)
        {
            return Wrapped.Call(method, args);
        }
    }
}

