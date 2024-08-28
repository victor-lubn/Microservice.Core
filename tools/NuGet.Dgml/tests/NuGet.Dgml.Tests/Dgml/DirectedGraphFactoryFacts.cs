using Xunit;

namespace NuGet.Dgml
{
    public class DirectedGraphFactoryFacts
    {
        public class Create
        {
            [Fact]
            public void AppliesParametersToDirectedGraph()
            {
                var layout = LayoutEnum.DependencyMatrix;
                var graphDirection = GraphDirectionEnum.BottomToTop;

                var directedGraph = DirectedGraphFactory.Create(layout, graphDirection);

                Assert.Equal(layout, directedGraph.Layout);
                Assert.True(directedGraph.LayoutSpecified);
                Assert.Equal(graphDirection, directedGraph.GraphDirection);
                Assert.True(directedGraph.GraphDirectionSpecified);
            }
        }

        public class CreateDependencyGraph
        {
            private readonly DirectedGraph _directedGraph;

            public CreateDependencyGraph() => _directedGraph = DirectedGraphFactory.CreateDependencyGraph();

            [Fact]
            public void LayoutIsSugiyama()
            {
                Assert.Equal(LayoutEnum.Sugiyama, _directedGraph.Layout);
                Assert.True(_directedGraph.LayoutSpecified);
            }

            [Fact]
            public void GraphDirectionIsRightToLeft()
            {
                Assert.Equal(GraphDirectionEnum.RightToLeft, _directedGraph.GraphDirection);
                Assert.True(_directedGraph.GraphDirectionSpecified);
            }
        }
    }
}
