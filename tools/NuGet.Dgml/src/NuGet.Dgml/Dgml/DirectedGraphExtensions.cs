using System;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NuGet.Dgml
{
    /// <summary>
    /// Provides static extension methods for <see cref="DirectedGraph"/>.
    /// </summary>
    public static class DirectedGraphExtensions
    {
        /// <summary>
        /// Returns the input typed as <see cref="XDocument"/>.
        /// </summary>
        /// <param name="graph">The sequence to type as <see cref="XDocument"/>.</param>
        /// <returns>The input sequence typed as <see cref="XDocument"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <c>null</c>.</exception>
        /// <see cref="XmlSerializer"/>
        public static XDocument AsXDocument(this DirectedGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var document = new XDocument();
            using (var writer = document.CreateWriter())
            {
                var serializer = new XmlSerializer(typeof(DirectedGraph));
                serializer.Serialize(writer, graph);
            }
            return document;
        }
    }
}
