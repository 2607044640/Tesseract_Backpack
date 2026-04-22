// ReSharper disable once CheckNamespace
namespace GodotStateCharts
{
    using Godot;
    
    /// Base class for all wrapper classes. Provides some common functionality. Not to be used directly.
    public abstract class NodeWrapper
    {
        /// The wrapped node.
        protected internal readonly Node Wrapped;

        protected NodeWrapper(Node wrapped)
        {
            Wrapped = wrapped;
        }
          
        /// Allows to connect to signals on the wrapped node.
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <param name="flags"></param>
        public Error Connect(StringName signal, Callable method, uint flags = 0u)
        {
            return Wrapped.Connect(signal, method, flags);
        }
        
        
        /// Allows to call methods on the wrapped node deferred.
        public Variant CallDeferred(string method, params Variant[] args)
        {
            return Wrapped.CallDeferred(method, args);
        }
        
        /// Allows to call methods on the wrapped node.
        public Variant Call(string method, params Variant[] args)
        {
            return Wrapped.Call(method, args);
        }
    }
}
