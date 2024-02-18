using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Core;
using Avalonia.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace UnitTest.Base.Utils
{
    /*
     * Export AvaloniaObject to xaml-like text
     *
     * !!CAUTION!!
     * 
     * I'm not sure that this class outputed xaml is valid for Avalonia.
     * I don't implement the logic to export any properties which is converted by any converter.
     */
    public class BrokenXamlWriter
    {
        #region assembly management

        /// <summary>
        /// xmlns-previx vs XmlNamespace
        /// </summary>
        private readonly HashSet<Assembly> _registeredAssemblies = new();
        private readonly Dictionary<string, XmlNamespace> _xmlNamespaces = new();

        private readonly List<AvaloniaProperty> _attachedProperties = new();


        public void RegisterAssembly(Assembly asm)
        {
            if (_registeredAssemblies.Contains(asm))
                return;

            /*
             * check XlmnsDefinition
             */
            foreach (IGrouping<string, XmlnsDefinitionAttribute> group
                in asm.GetCustomAttributes<XmlnsDefinitionAttribute>().GroupBy(attr => attr.XmlNamespace))
            {
                string xmlurl = group.Key;
                ClassNamespace[] clrspc = group.Select(def => new ClassNamespace(asm, def.ClrNamespace))
                                  .ToArray();

                if (_xmlNamespaces.Count == 0)
                {
                    _xmlNamespaces[string.Empty]
                        = new XmlNamespace(string.Empty, xmlurl, clrspc);

                    continue;
                }

                XmlNamespace alreadyRegistered = _xmlNamespaces.Values.Where(xpc => xpc.Namespace == xmlurl).FirstOrDefault();

                if (alreadyRegistered is null)
                {
                    /* register as new*/

                    string prefix = GeneratePrefixFor(asm);

                    _xmlNamespaces[prefix]
                        = new XmlNamespace(prefix, xmlurl, clrspc);
                }
                else
                {
                    /* update */

                    alreadyRegistered.ClassSpaces.AddRange(clrspc);
                }
            }

            /*
             * collect AttachedProperty
             * 
             * It's a dirty hack.
             * 
             * Because AvaloniaObject dosen't expose ValueStore,
             * I can't recognize any attached properties.
             * This hack collect AttachedProperty in Assembly,
             * And check wheither property is set or not when exporting.
             */
            IEnumerable<AvaloniaProperty> attachecProperties = asm.GetTypes()
                    // only AvaloniaObject
                    .Where(tp => typeof(AvaloniaObject).IsAssignableFrom(tp))
                    .SelectMany(aoTp => aoTp.GetFields(BindingFlags.Public | BindingFlags.Static))
                    // only fields for AttachedProperty
                    .Where(fld => typeof(AvaloniaProperty).IsAssignableFrom(fld.FieldType))
                    .Where(fld => fld.FieldType.IsGenericType)
                    .Where(fld => fld.FieldType.GetGenericTypeDefinition() == typeof(AttachedProperty<>))
                    .Select(fld => (AvaloniaProperty)fld.GetValue(null));

            _attachedProperties.AddRange(attachecProperties);



            _registeredAssemblies.Add(asm);
        }

        private string GeneratePrefixFor(Assembly asm)
        {
            // 'Markdown.Avalonia' -> "ma"
            string full = String.Join("", asm.GetName().Name.Split('.')
                                             .Select(nmchip => nmchip[0].ToString().ToLower()));

            // When full is 'yoghurt', Try 'y', 'yo', 'yog', ...
            string prefix = Enumerable.Range(1, full.Length)
                                      .Select(len => full.Substring(0, len))
                                      .Where(chip => !_xmlNamespaces.ContainsKey(chip))
                                      .FirstOrDefault();

            // 'yogurt2', 'yogurt3', 'yogurt4', ...
            for (var idx = 2; prefix is null; ++idx)
            {
                var chip = full + idx;
                if (!_xmlNamespaces.ContainsKey(chip))
                {
                    prefix = chip;
                    break;
                }
            }

            return prefix;
        }

        public string GetPrefixFor(Type type)
        {
            Assembly asm = type.Assembly;

            if (!_registeredAssemblies.Contains(asm))
            {
                RegisterAssembly(asm);
            }

            string nmspc = type.Namespace;

            // already registered?
            KeyValuePair<string, XmlNamespace>[] keyAndValues = _xmlNamespaces
                .Where(entry => entry.Value.ClassSpaces
                                     .Any(clsnmspc => clsnmspc.Assembly == asm && clsnmspc.Namespace == nmspc))
                .ToArray();

            if (keyAndValues.Length > 0)
            {
                return keyAndValues[0].Key;
            }

            // make xmlspace
            string prefix = GeneratePrefixFor(asm);

            string xmlurl = $"clr-namespace:{nmspc};assembly={asm.GetName().Name}";

            var xmlspc = new XmlNamespace(prefix, xmlurl, new ClassNamespace(asm, nmspc));

            _xmlNamespaces[prefix] = xmlspc;

            return xmlspc.Prefix;
        }

        #endregion

        #region collect properties

        public ObjectNode Collect(AvaloniaObject obj)
        {
            var node = new ObjectNode();

            Type objType = obj.GetType();


            /*
             * Check Content property
             */
            PropertyInfo contentProp = objType.GetProperties()
                                              .Where(pinf => pinf.GetCustomAttribute(typeof(ContentAttribute)) != null)
                                              .FirstOrDefault();

            if (contentProp != null)
            {
                var objValue = contentProp.GetValue(obj);
                if (objValue != null)
                {
                    node.Content = new ObjectProperty()
                    {
                        Owner = obj,
                        AttributeName = contentProp.Name,
                        PropertyInfo = contentProp,
                        Value = objValue
                    };
                }
            }

            /*
             * Check Avalonia property
             */
            var attrAvaProps = objType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                  .Where(fld => typeof(AvaloniaProperty).IsAssignableFrom(fld.FieldType))
                                  .Select(fld => ((AvaloniaProperty)fld.GetValue(null)))
                                  // ignore content property
                                  .Where(ap => ap.Name != contentProp?.Name);

            node.Attributes = new List<ObjectProperty>();
            node.Attributes.AddRange(
                CollectChangedValue(obj, attrAvaProps)
                    .Select(tpl => new ObjectProperty()
                    {
                        Owner = obj,
                        AvaloniaProperty = tpl.Item1,
                        AttributeName = tpl.Item1.Name,
                        PropertyInfo = objType.GetProperty(tpl.Item1.Name),
                        Value = tpl.Item2
                    })
            );

            /*
             * Check Object property
             */
            var plainProps = objType.GetProperties()
                               // ignore avalonia property
                               .Where(pinf => !attrAvaProps.Any(nd => nd.Name == pinf.Name))
                               // ignore content property
                               .Where(pinf => pinf != contentProp)
                               // has getter and setter
                               .Where(pinf => pinf.CanWrite && pinf.CanRead)
                               .Where(pinf => pinf.GetSetMethod() != null)
                               .Where(pinf => pinf.GetGetMethod() != null && pinf.GetGetMethod().GetParameters().Length == 0);

            foreach (var pinf in plainProps)
            {
                object value = pinf.GetValue(obj);

                if (obj is StyledElement elm)
                {
                    switch (pinf.Name)
                    {
                        case nameof(StyledElement.Resources):
                            if (elm.Resources.Count == 0)
                                continue;
                            break;
                    }
                }

                if (pinf.PropertyType.IsValueType)
                {
                    if (value == Activator.CreateInstance(pinf.PropertyType))
                        continue;
                }
                else if (value is null)
                {
                    continue;
                }

                node.Attributes.Add(new ObjectProperty()
                {
                    Owner = obj,
                    AttributeName = pinf.Name,
                    PropertyInfo = pinf,
                    Value = pinf.GetValue(obj)
                });
            }


            var addableProps = objType.GetProperties()
                   // ignore avalonia property
                   .Where(pinf => !attrAvaProps.Any(nd => nd.Name == pinf.Name))
                   // ignore content property
                   .Where(pinf => pinf != contentProp)
                   .Where(pinf => pinf.CanRead)
                   .Where(pinf => pinf.GetSetMethod() == null)
                   .Where(pinf => pinf.GetGetMethod() != null && pinf.GetGetMethod().GetParameters().Length == 0);

            foreach (var pinf in addableProps)
            {
                object value = pinf.GetValue(obj);

                if (obj is StyledElement elm)
                {
                    switch (pinf.Name)
                    {
                        case nameof(Control.Classes):
                            if (elm.Classes.Count == 0 || (elm.Classes.Count == 1 && String.IsNullOrEmpty(elm.Classes[0])))
                                continue;
                            break;
                    }
                }

                if (value is null)
                    continue;

                if (value is not IList)
                    continue;

                var list = (IList)value;
                if (list.Count == 0)
                    continue;

                node.Attributes.Add(new ObjectProperty()
                {
                    Owner = obj,
                    AttributeName = pinf.Name,
                    PropertyInfo = pinf,
                    Value = pinf.GetValue(obj)
                });
            }


            var attachAvaProps = _attachedProperties.Where(prop => !node.Attributes.Any(attr => attr.AvaloniaProperty == prop));
            node.Attributes.AddRange(
                CollectChangedValue(obj, attachAvaProps)
                    .Select(tpl => new ObjectProperty()
                    {
                        Owner = obj,
                        AvaloniaProperty = tpl.Item1,
                        AttributePrefix = GetPrefixFor(tpl.Item1.OwnerType),
                        AttributeName = $"{tpl.Item1.OwnerType.Name}.{tpl.Item1.Name}",
                        PropertyInfo = objType.GetProperty(tpl.Item1.Name),
                        Value = tpl.Item2
                    })
            );


            return node;
        }

        private IEnumerable<(AvaloniaProperty, object)> CollectChangedValue(AvaloniaObject obj, IEnumerable<AvaloniaProperty> aprops)
        {
            foreach (var aprop in aprops)
            {
                if (aprop.Name == "Parent") continue;
                if (aprop.IsReadOnly) continue;

                if (obj.IsSet(aprop))
                {
                    var objValue = obj.GetValue(aprop);
                    yield return (aprop, objValue);
                }
                else if (aprop.Name == "Text" && !String.IsNullOrEmpty(obj.GetValue(aprop).ToString()))
                {
                    var objValue = obj.GetValue(aprop);
                    yield return (aprop, objValue);
                }
            }
        }

        #endregion

        public XmlDocument Document { private set; get; }

        public BrokenXamlWriter()
        {
            Document = new XmlDocument();
            RegisterAssembly(typeof(Control).Assembly);
        }

        private XmlElement CreateElement(string name)
        {
            var spidx = name.IndexOf(':');

            return spidx == -1 ?
                        CreateElement(null, name) :
                        CreateElement(name.Substring(0, spidx), name.Substring(spidx + 1));
        }
        private XmlElement CreateElement(string prefix, string name)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                if (name.Contains(":")) throw new ArgumentException();

                return Document.CreateElement(name);
            }
            else
            {
                return Document.CreateElement(prefix, name, _xmlNamespaces[prefix].Namespace);
            }
        }

        private XmlAttribute CreateAttribute(string name)
        {
            var spidx = name.IndexOf(':');

            return spidx == -1 ?
                        CreateAttribute(null, name) :
                        CreateAttribute(name.Substring(0, spidx), name.Substring(spidx + 1));
        }

        private XmlAttribute CreateAttribute(string prefix, string name)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                if (name.Contains(":")) throw new ArgumentException();

                return Document.CreateAttribute(name);
            }
            else
            {
                return Document.CreateAttribute(prefix, name, _xmlNamespaces[prefix].Namespace);
            }
        }

        public XmlDocument Transform(object value)
        {
            var valueType = value.GetType();

            var root = Document.CreateElement("", valueType.Name, _xmlNamespaces[""].Namespace);

            Document.AppendChild(root);

            ApplyTo(root, (AvaloniaObject)value);

            foreach (var xmlSpc in _xmlNamespaces.Values)
            {
                if (string.IsNullOrEmpty(xmlSpc.Prefix)) continue;

                Document.DocumentElement.SetAttribute("xmlns:" + xmlSpc.Prefix, xmlSpc.Namespace);
            }
            Document.DocumentElement.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");

            return Document;
        }

        private void ApplyTo(XmlNode valueNode, AvaloniaObject value)
        {
            ObjectNode objNode = Collect(value);

            if (objNode.Content != null)
            {
                Append(valueNode, objNode.Content, true);
            }

            foreach (var a in objNode.Attributes)
            {
                Append(valueNode, a, false);
            }
        }

        private void Append(XmlNode valueNode, ObjectProperty prop, bool isContent)
        {
            if (prop.Value is String
                    || prop.Value is bool
                    || prop.Value is short || prop.Value is int || prop.Value is long
                    || prop.Value is float || prop.Value is double)
            {

                XmlAttribute attr = CreateAttribute(prop.AttributePrefix, prop.AttributeName);
                attr.Value = prop.Value.ToString();

                ((XmlElement)valueNode).SetAttributeNode(attr);
            }

            else if (prop.Value is null)
            {
                // FIXIT 

                XmlAttribute attr = CreateAttribute(prop.AttributePrefix, prop.AttributeName);
                attr.Value = "{x:Null}";

                ((XmlElement)valueNode).SetAttributeNode(attr);
            }

            else if (prop.Value is Classes clsLst)
            {
                XmlAttribute attr = CreateAttribute(prop.AttributeName);
                attr.Value = String.Join(",", clsLst);

                ((XmlElement)valueNode).SetAttributeNode(attr);
            }

            else if (prop.Value is IEnumerable list)
            {
                XmlNode propHolder;
                if (isContent)
                {
                    propHolder = valueNode;
                }
                else
                {
                    propHolder = CreateElement(valueNode.Name + "." + prop.AttributeName);
                    valueNode.AppendChild(propHolder);
                }

                foreach (var elm in list)
                {
                    Type elmType = elm.GetType();
                    XmlElement element = CreateElement(GetPrefixFor(elmType), elmType.Name);

                    propHolder.AppendChild(element);

                    if (elm is AvaloniaObject elmAo)
                    {
                        ApplyTo(element, elmAo);
                    }
                }
            }

            else if (prop.Value is AvaloniaObject aobj)
            {
                Type aoType = aobj.GetType();

                XmlElement propHolder = CreateElement(valueNode.Name + "." + prop.AttributeName);
                XmlElement element = CreateElement(GetPrefixFor(aoType), aoType.Name);
                propHolder.AppendChild(element);

                valueNode.AppendChild(propHolder);

                ApplyTo(element, aobj);
            }
        }
    }

    class XmlNamespace
    {
        public string Prefix { get; }
        public string Namespace { get; }
        public List<ClassNamespace> ClassSpaces { get; }

        public XmlNamespace(string prefix, string nmspc, params ClassNamespace[] clsspc)
            : this(prefix, nmspc, (IEnumerable<ClassNamespace>)clsspc)
        {
        }

        public XmlNamespace(string prefix, string nmspc, IEnumerable<ClassNamespace> clsspc)
        {
            this.Prefix = prefix;
            this.Namespace = nmspc;
            this.ClassSpaces = new List<ClassNamespace>(clsspc);
        }
    }
    class ClassNamespace
    {
        public Assembly Assembly { get; }
        public string Namespace { get; }

        public ClassNamespace(Assembly asm, string nmspc)
        {
            this.Assembly = asm;
            this.Namespace = nmspc;
        }
    }

    public class ObjectNode
    {
        public object Owner { get; set; }
        public ObjectProperty Content { get; set; }
        public List<ObjectProperty> Attributes { get; set; }
    }

    public class ObjectProperty
    {
        public object Owner { get; set; }
        public string AttributePrefix { get; set; }
        public string AttributeName { get; set; }

        public AvaloniaProperty AvaloniaProperty { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object Value { get; set; }
    }
}
