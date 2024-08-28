using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace NuGet
{
    internal sealed class StubNuGetResourceProvider : ResourceProvider
    {
        private readonly INuGetResource _resource;

        internal StubNuGetResourceProvider(INuGetResource resource) : base(resource.GetType().BaseType) => _resource = resource;

        public override Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
            => Task.FromResult(Tuple.Create(true, _resource));
    }
}
