using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.Base.Portable
{
    public class PortableRegistry : BaseVec<PortableType>
    {
        public override string TypeName() => "PortableRegistry";
    }
}