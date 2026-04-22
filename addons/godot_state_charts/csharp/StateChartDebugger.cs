// ReSharper disable once CheckNamespace
namespace GodotStateCharts
{
    using Godot;
    using System;

    /// Wrapper around the state chart debugger node.
    public class StateChartDebugger : NodeWrapper
    {
        private StateChartDebugger(Node wrapped) : base(wrapped) {}

        ///  Creates a wrapper object around the given node and verifies that the node
        /// is actually a state chart debugger. The wrapper object can then be used to interact
        /// with the state chart debugger from C#.
        /// <param name="stateChartDebugger">the node that is the state chart debugger</param>
        /// <returns>a StateChartDebugger wrapper.</returns>
        /// <throws>ArgumentException if the node is not a state chart debugger.</throws>
        public static StateChartDebugger Of(Node stateChartDebugger)
        {
            if (stateChartDebugger.GetScript().As<Script>() is not GDScript gdScript
                || !gdScript.ResourcePath.EndsWith("state_chart_debugger.gd"))
            {
                throw new ArgumentException("Given node is not a state chart debugger.");
            }

            return new StateChartDebugger(stateChartDebugger);
        }

        /// Sets the node that the state chart debugger should debug.
        /// <param name="node">the the node that should be debugged. Can be a state chart or any
        /// node above a state chart. The debugger will automatically pick the first state chart
        /// node below the given one.</param>
        public void DebugNode(Node node)
        {
            Call(MethodName.DebugNode, node);
        }
        
        /// Adds a history entry to the history output.
        /// <param name="text">the text to add</param>
        public void AddHistoryEntry(string text)
        {
            Call(MethodName.AddHistoryEntry, text);
        }
        
        public class MethodName : Node.MethodName
        {
            /// Sets the node that the state chart debugger should debug.
            public static readonly string DebugNode = "debug_node";
            ///  Adds a history entry to the history output.
            public static readonly string AddHistoryEntry = "add_history_entry";
        }
    }
}
