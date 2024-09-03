using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    public abstract class NodeBuilderType
    {
        public int? Index { get; set; } = null;

        public abstract TypeDefEnum TypeDef {  get; }

        /// <summary>
        /// The mapped class to V14
        /// </summary>
        public string Adapted { get; set; }

        /// <summary>
        /// The initial raw class
        /// </summary>
        public string Raw { get; set; }

        /// <summary>
        /// Pallet context to help identify the pallet.
        /// Useful for pallets with the same type name (for example "AuthorityId" which is used in multiple pallets and return different lookup index)
        /// </summary>
        public string PalletContext { get; set; } = string.Empty;

        /// <summary>
        /// When the code is not enough to identify the mapping (or maybe i am too lazy...), hard map the index
        /// </summary>
        public uint? IndexHardBinding => SearchV14.HardIndexBinding(Adapted);

        protected NodeBuilderType(string content, string raw, string palletContext)
        {
            Adapted = content;
            Raw = raw;
            PalletContext = palletContext;
        }

        public List<NodeBuilderType> Children { get; set; } = new List<NodeBuilderType>();

        private string ToString(int level)
        {
            var indent = new string(' ', level * 2);
            var result = indent + TypeDef.ToString();

            foreach (var child in Children)
            {
                result += "\n" + child.ToString(level + 1);
            }

            return result;
        }

    }

    public class NodeBuilderTypeUndefined : NodeBuilderType
    {
        public override TypeDefEnum TypeDef => throw new NotImplementedException();
        
        public NodeBuilderTypeUndefined(string content, string palletContext) : base(content, content, palletContext)
        {
        }

        public NodeBuilderTypeUndefined(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
        }
    }

    /// <summary>
    /// A variant of <see cref="NodeBuilderTypeVariant"/> to handle Option<T> more easily
    /// </summary>
    public class NodeBuilderTypeOption : NodeBuilderType
    {
        public override TypeDefEnum TypeDef => TypeDefEnum.Variant;

        public NodeBuilderTypeOption(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
        }
    }

    public class NodeBuilderTypePrimitive : NodeBuilderType
    {
        public TypeDefPrimitive Primitive { get; set; }

        public NodeBuilderTypePrimitive(string content, string raw, TypeDefPrimitive primitive, string palletContext): base(content, raw, palletContext)
        {
            Primitive = primitive;
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Primitive;
    }

    public class NodeBuilderTypeComposite : NodeBuilderType
    {
        public NodeBuilderTypeComposite(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Composite;
    }

    public class NodeBuilderTypeSequence : NodeBuilderType
    {
        public NodeBuilderTypeSequence(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Sequence;
    }

    public class NodeBuilderTypeTuple : NodeBuilderType
    {
        public NodeBuilderTypeTuple(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Tuple;
    }

    public class NodeBuilderTypeArray : NodeBuilderType
    {
        public NodeBuilderTypeArray(string content, string raw, int length, string palletContext): base(content, raw, palletContext)
        {
            Length = length;
        }

        public int Length { get; set; }

        public override TypeDefEnum TypeDef => TypeDefEnum.Array;
    }

    public class NodeBuilderTypeCompact : NodeBuilderType
    {
        public NodeBuilderTypeCompact(string content, string palletContext) : base(content, content, palletContext)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Compact;
    }

    public class NodeBuilderTypeVariant : NodeBuilderType
    {
        public NodeBuilderTypeVariant(string content, string raw, string palletContext) : base(content, raw, palletContext)
        {
            PalletContext = palletContext;
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Variant;
    }
}
