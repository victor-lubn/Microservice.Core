using System;
using Xunit;

namespace NuGet.Dgml
{
    public class DirectedGraphExtensionsFacts
    {
        public class AsXDocument
        {
            /* Only basic tests were implemented. We assume that XmlSerializer
             * and the xsd tool are working properly.
             */

            [Fact]
            public void ThrowsOnNull()
            {
                DirectedGraph graph = null;
                Assert.Throws<ArgumentNullException>("graph", () => graph.AsXDocument());
            }

            [Fact]
            public void GeneratesXDocument()
            {
                var graph = new DirectedGraph
                {
                    Nodes = new[] { new DirectedGraphNode(), }
                };
                var document = graph.AsXDocument();
                Assert.Single(document.Root.Elements());
            }
        }
    }
}
