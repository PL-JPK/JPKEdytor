using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;

namespace Puch.JPK
{
    public struct DictionaryElement
    {
        public string Kod { get; set; }
        public string Nazwa { get; set; }
    }

    public class XsdCodeReader
    {
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        public static IEnumerable<DictionaryElement> ReadDictionary(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var schema = XmlSchema.Read(fs, ValidationEventHandler);
                var set = new XmlSchemaSet();
                set.Add(schema);
                set.Compile();
                var content = (schema.Items[1] as XmlSchemaSimpleType)?.Content as XmlSchemaSimpleTypeRestriction;
                var facets = content?.Facets;
                if (facets != null)
                {
                    return facets.Cast<XmlSchemaEnumerationFacet>().Select(u => new DictionaryElement() { Kod = u.Value, Nazwa = (u.Annotation.Items[0] as XmlSchemaDocumentation).Markup[0].Value });
                }
                return null;
            }
        }
    }
}
