using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using TypeDefExt = Substrate.NetApi.Model.Types.Base.BaseEnumExt<
    Substrate.NET.Metadata.Base.TypeDefEnum,
    Substrate.NET.Metadata.Base.TypeDefComposite,
    Substrate.NET.Metadata.Base.TypeDefVariant,
    Substrate.NET.Metadata.Base.TypeDefSequence,
    Substrate.NET.Metadata.Base.TypeDefArray,
    Substrate.NET.Metadata.Base.TypeDefTuple,
    Substrate.NetApi.Model.Types.Base.BaseEnum<Substrate.NET.Metadata.Base.TypeDefPrimitive>,
    Substrate.NET.Metadata.Base.TypeDefCompact,
    Substrate.NET.Metadata.Base.TypeDefBitSequence,
    Substrate.NetApi.Model.Types.Base.BaseVoid>;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    internal class CustomCompositeBuilder : ICustomNodeBuilder
    {
        private List<string> Paths { get; set; } = new List<string>();
        private List<Field> Fields { get; set; } = new List<Field>();
        private uint? Start { get; set; }
        private uint? End { get; set; }

        public bool IsVersionValid(uint version)
        {
            return (Start, End) switch
            {
                (null, null) => true,
                (>= 0, >= 0) => version >= Start && version <= End,
                (>= 0, _) => version >= Start,
                (_, >= 0) => version <= End
            };
        }

        public PortableType Build(ConversionBuilder conversionBuilder)
        {
            var path = new Base.Portable.Path();
            path.Create(Paths.Select(p => new Str(p)).ToArray());

            var docs = new BaseVec<Str>([]);

            var typeParams = new BaseVec<TypeParameter>([]);

            // For each fields, try to get ids
            foreach(var field in Fields)
            {
                field.FieldTy = TType.From(conversionBuilder.BuildPortableTypes(field.FieldTypeName.Value.Value).Value);
            }

            var composite = new Base.TypeDefComposite();
            composite.Fields = new BaseVec<Field>(Fields.ToArray());

            var typeDef = new TypeDefExt();
            typeDef.Create(Base.TypeDefEnum.Composite, composite);

            var index = conversionBuilder.GetNewIndex();
            var pt = new PortableType(index, new TypePortableForm(
                path,
                typeParams,
                typeDef,
                docs
            ));

            conversionBuilder.PortableTypes.Add(pt);

            conversionBuilder.OverrideTypeMapping.Add(conversionBuilder.FindIndexByClass(Paths[^1]).index, (int)index.Value);
            return pt;
        }

        public CustomCompositeBuilder FromVersion(uint version)
        {
            if (End is not null && version > End)
                throw new InvalidOperationException("Start version > End version");
            
            Start = version;
            return this;
        }

        public CustomCompositeBuilder ToVersion(uint version)
        {
            if (Start is not null && version < Start)
                throw new InvalidOperationException("Start version > End version");

            End = version;
            return this;
        }

        public CustomCompositeBuilder WithPath(params string[] paths)
        {
            this.Paths = paths.ToList();
            return this;
        }

        public CustomCompositeBuilder AddField(string fieldName, string fieldType)
        {
            var field = new Field();
            field.Name = new BaseOpt<Str>(new Str(fieldName));
            field.FieldTypeName = new BaseOpt<Str>(new Str(fieldType));
            field.Docs = new BaseVec<Str>([]);

            Fields.Add(field);
            return this;
        }

        public CustomCompositeBuilder WithTypeParams()
        {
           return this;
        }
    }
}
