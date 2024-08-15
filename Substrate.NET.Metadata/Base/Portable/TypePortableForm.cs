using Substrate.NET.Metadata.Conversion;
using Substrate.NetApi;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Base.Portable
{
    public class TypePortableForm : BaseType
    {
        public TypePortableForm() { }

        internal TypePortableForm(Path path,
                                  BaseVec<TypeParameter> typeParams,
                                  BaseEnumExt<TypeDefEnum, TypeDefComposite, TypeDefVariant, TypeDefSequence, TypeDefArray, TypeDefTuple, BaseEnum<TypeDefPrimitive>, TypeDefCompact, TypeDefBitSequence, BaseVoid> typeDef,
                                  BaseVec<Str> docs)
        {
            Path = path;
            TypeParams = typeParams;
            TypeDef = typeDef;
            Docs = docs;
        }

        public override string TypeName() => "Type<T: Form = MetaForm>";

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Path = new Path();
            Path.Decode(byteArray, ref p);

            TypeParams = new BaseVec<TypeParameter>();
            TypeParams.Decode(byteArray, ref p);

            TypeDef = new BaseEnumExt<TypeDefEnum, TypeDefComposite, TypeDefVariant, TypeDefSequence, TypeDefArray, TypeDefTuple, BaseEnum<TypeDefPrimitive>, TypeDefCompact, TypeDefBitSequence, BaseVoid>();
            TypeDef.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Path Path { get; internal set; }
        public BaseVec<TypeParameter> TypeParams { get; internal set; }
        public BaseEnumExt<TypeDefEnum, TypeDefComposite, TypeDefVariant, TypeDefSequence, TypeDefArray, TypeDefTuple, BaseEnum<TypeDefPrimitive>, TypeDefCompact, TypeDefBitSequence, BaseVoid> TypeDef { get; internal set; }
        public BaseVec<Str> Docs { get; internal set; }

        public static TypePortableForm CreatePrimitive(NodeTypePrimitive nodeTypePrimitive)
        {
            var ty = CreateCommon(nodeTypePrimitive);

            ty.TypeDef = new BaseEnumExt<TypeDefEnum, TypeDefComposite, TypeDefVariant, TypeDefSequence, TypeDefArray, TypeDefTuple, BaseEnum<TypeDefPrimitive>, TypeDefCompact, TypeDefBitSequence, BaseVoid>();

            var mapPrimitive = Enum.Parse(typeof(TypeDefPrimitive), nodeTypePrimitive.Primitive.ToString());
            if(mapPrimitive == null)
            {
                throw new MetadataConversionException($"Unable to find {nodeTypePrimitive.Primitive} into enum {typeof(TypeDefPrimitive)}");
            }

            ty.TypeDef.Create(TypeDefEnum.Primitive, new BaseEnum<TypeDefPrimitive>((TypeDefPrimitive)nodeTypePrimitive.Primitive));

            return ty;
        }

        private static TypePortableForm CreateCommon(NodeType nodeTypePrimitive)
        {
            var ty = new TypePortableForm();

            ty.Path = new Path();
            ty.Path.Create(nodeTypePrimitive.Path.Select(x => new Str(x)).ToArray());

            ty.TypeParams = new BaseVec<TypeParameter>(
                nodeTypePrimitive.TypeParams.Select(x => new TypeParameter(new Str(x.Name), x.TypeId != null ? new BaseOpt<TType>(TType.From(x.TypeId.Value)) : new BaseOpt<TType>())).ToArray());

            ty.Docs = new BaseVec<Str>();
            ty.Docs.Create(nodeTypePrimitive.Docs.Select(x => new Str(x)).ToArray());

            //ty.TypeDef

            return ty;
        }
    }

    public class Path : BaseVec<Str>
    {
        public override string TypeName() => "Path<T: Form = MetaForm>";
    }

    public class TypeParameter : BaseType, IMetadataName
    {
        public TypeParameter() { }

        public TypeParameter(Str name, BaseOpt<TType> typeParameterType)
        {
            Name = name;
            TypeParameterType = typeParameterType;
        }

        public override string TypeName() => "TypeParameter<T: Form = MetaForm>";

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            TypeParameterType = new BaseOpt<TType>();
            TypeParameterType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; internal set; } = default!;
        public BaseOpt<TType> TypeParameterType { get; internal set; } = default!;
    }
}