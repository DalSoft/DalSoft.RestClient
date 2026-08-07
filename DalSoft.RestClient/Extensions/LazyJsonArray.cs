using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using DalSoft.RestClient.Serialization;

namespace DalSoft.RestClient.Extensions
{
    //Wraps a json array without materializing it, elements are only wrapped when accessed
    //DynamicObject rather than a plain class because the DLR can't bind to members of an internal type from the caller's assembly
    internal class LazyJsonArray : DynamicObject, IReadOnlyList<object>
    {
        private readonly IJsonNode _arrayNode;

        public LazyJsonArray(IJsonNode arrayNode)
        {
            _arrayNode = arrayNode;
        }

        public int Count => _arrayNode.GetArrayCount();

        public object this[int index] => _arrayNode.GetIndex(index).Wrap();

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var node in _arrayNode.EnumerateArray())
            {
                yield return node.Wrap();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this[(int)indexes[0]];
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == nameof(Count))
            {
                result = Count;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type.IsInstanceOfType(this)) //IEnumerable etc. so foreach and LINQ work
            {
                result = this;
                return true;
            }

            if (binder.Type.IsAssignableFrom(typeof(List<object>)))
            {
                result = new List<object>(this); //Only materializes if the caller asks for a List
                return true;
            }

            result = null;
            return false;
        }
    }
}
