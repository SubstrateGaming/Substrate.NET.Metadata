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
        public abstract TypeDefEnum TypeDef {  get; }
        public string Adapted { get; set; }
        public string Raw { get; set; }

        protected NodeBuilderType(string content, string raw)
        {
            Adapted = content;
            Raw = raw;
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
        
        public NodeBuilderTypeUndefined(string content) : base(content, content)
        {
        }
    }

    public class NodeBuilderTypePrimitive : NodeBuilderType
    {
        public TypeDefPrimitive Primitive { get; set; }

        public NodeBuilderTypePrimitive(string content, TypeDefPrimitive primitive): base(content, content)
        {
            Primitive = primitive;
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Primitive;
    }

    public class NodeBuilderTypeComposite : NodeBuilderType
    {
        public NodeBuilderTypeComposite(string content, string raw) : base(content, raw)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Composite;
    }

    public class NodeBuilderTypeSequence : NodeBuilderType
    {
        public NodeBuilderTypeSequence(string content) : base(content, content)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Sequence;
    }

    public class NodeBuilderTypeTuple : NodeBuilderType
    {
        public NodeBuilderTypeTuple(string content) : base(content, content)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Tuple;
    }

    public class NodeBuilderTypeArray : NodeBuilderType
    {
        public NodeBuilderTypeArray(string content, int length): base(content, content)
        {
            Length = length;
        }

        public int Length { get; set; }

        public override TypeDefEnum TypeDef => TypeDefEnum.Tuple;
    }

    public class NodeBuilderTypeCompact : NodeBuilderType
    {
        public NodeBuilderTypeCompact(string content) : base(content, content)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Compact;
    }

    public class NodeBuilderTypeVariant : NodeBuilderType
    {
        public NodeBuilderTypeVariant(string content) : base(content, content)
        {
        }

        public override TypeDefEnum TypeDef => TypeDefEnum.Variant;
    }
}
