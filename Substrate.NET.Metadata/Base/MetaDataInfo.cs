﻿using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Substrate.NET.Metadata.Base
{
    public class MetaDataInfo : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Magic.Encode());
            result.AddRange(Version.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Magic = new U32();
            Magic.Decode(byteArray, ref p);

            Version = new U8();
            Version.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public U32 Magic { get; private set; }
        public U8 Version { get; private set; }
    }
}
