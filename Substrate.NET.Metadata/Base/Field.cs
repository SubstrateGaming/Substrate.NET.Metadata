﻿using System;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Base
{
    public class Field : BaseType, IMetadataType
    {
        public Field() { }

        public Field(BaseOpt<Str> name, TType fieldTy, BaseOpt<Str> fieldTypeName, BaseVec<Str> docs)
        {
            Name = name;
            FieldTy = fieldTy;
            FieldTypeName = fieldTypeName;
            Docs = docs;
        }

        public override string TypeName() => "Field<T: Form = MetaForm>";

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(FieldTy.Encode());
            result.AddRange(FieldTypeName.Encode());
            result.AddRange(Docs.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new BaseOpt<Str>();
            Name.Decode(byteArray, ref p);

            FieldTy = new TType();
            FieldTy.Decode(byteArray, ref p);

            FieldTypeName = new BaseOpt<Str>();
            FieldTypeName.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public BaseOpt<Str> Name { get; private set; }
        public TType FieldTy { get; private set; }
        public BaseOpt<Str> FieldTypeName { get; private set; }
        public BaseVec<Str> Docs { get; private set; }

        public TType ElemType => FieldTy;
    }
}