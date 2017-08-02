using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DalSoft.RestClient.Extensions
{
    /// <summary>
    /// Helper class for renaming json-objects
    /// </summary>
    class RenamingJTokenWriter : JTokenWriter
    {
        readonly Func<string, string> nameMap;

        public RenamingJTokenWriter(Func<string, string> nameMap)
            : base()
        {
            if (nameMap == null)
                throw new ArgumentNullException();
            this.nameMap = nameMap;
        }

        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(nameMap(name));
        }

        // No need to override WritePropertyName(string name, bool escape) since it calls WritePropertyName(string name)
    }
}
