﻿using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class PalletErrorMetadataV9 : BaseType, IMetadataName
    {
        public Str Name { get; set; }
        public BaseVec<Str> Docs { get; set; }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - num;
        }
    }
}
