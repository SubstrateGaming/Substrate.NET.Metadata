﻿using System;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Base.Portable
{
    public class PortableType : BaseType
    {
        public PortableType() { }

        internal PortableType(U32 id, TypePortableForm ty) 
        {
            Id = id;
            Ty = ty;
        }

        public override string TypeName() => "PortableType";

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            // #[codec(compact)]
            Id = new U32();
            Id.Create(CompactInteger.Decode(byteArray, ref p));

            Ty = new TypePortableForm();
            Ty.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public U32 Id { get; private set; }
        public TypePortableForm Ty { get; private set; }
    }
}