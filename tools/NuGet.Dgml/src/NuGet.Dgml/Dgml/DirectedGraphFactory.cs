namespace NuGet.Dgml
{
    /// <summary>
    /// Provides instance methods to create instances of <see cref="DirectedGraph"/>.
    /// </summary>
    public static class DirectedGraphFactory
    {
        /// <summary>
        /// Creates a <see cref="DirectedGraph"/> with the specified parameters.
        /// </summary>
        /// <param name="layout">The layout of the directed graph.</param>
        /// <param name="graphDirection">The direction of the graph.</param>
        /// <returns>The directed graph configured with the specified parameters.</returns>
        public static DirectedGraph Create(LayoutEnum layout, GraphDirectionEnum graphDirection)
            => new DirectedGraph
            {
                Layout = layout,
                LayoutSpecified = true,
                GraphDirection = graphDirection,
                GraphDirectionSpecified = true,
            };

        /// <summary>
        /// Creates a <see cref="DirectedGraph"/> to visualize dependencies of NuGet packages.
        /// </summary>
        /// <returns></returns>
        public static DirectedGraph CreateDependencyGraph() => Create(LayoutEnum.Sugiyama, GraphDirectionEnum.RightToLeft);
    }
}
