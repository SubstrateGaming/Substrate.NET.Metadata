using Newtonsoft.Json.Linq;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V15
{
    public class OuterEnums15 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(CallType.Encode());
            result.AddRange(EventType.Encode());
            result.AddRange(ErrorType.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            CallType = new TType();
            CallType.Decode(byteArray, ref p);

            EventType = new TType();
            EventType.Decode(byteArray, ref p);

            ErrorType = new TType();
            ErrorType.Decode(byteArray, ref p);
        }

        public override string ToString()
        {
            return $"CallType: {CallType}, EventType: {EventType}, ErrorType: {ErrorType}";
        }

        public TType CallType { get; private set; } = default!;
        public TType EventType { get; private set; } = default!;
        public TType ErrorType { get; private set; } = default!;
    }
}
