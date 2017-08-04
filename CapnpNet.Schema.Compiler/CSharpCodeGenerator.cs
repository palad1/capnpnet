﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Options;

namespace CapnpNet.Schema.Compiler
{
  // TOOD: generate AST from FormattableString?
  public class CSharpCodeGenerator
  {
    // TODO: annotations for namespace, name overrides, and doc comments
    public const ulong NamespaceAnnotationId = ~0UL; // TODO

    // TODO: although it is auto-generated, I would like to remove excess qualification...
    public const string StructType = "global::CapnpNet.Struct";
    public const string MessageType = "global::CapnpNet.Message";
    public const string CnNamespace = "global::CapnpNet";

    private readonly CodeGeneratorRequest _request;
    private readonly SyntaxGenerator _generator;

    private Dictionary<ulong, TypeName> _typeNames = new Dictionary<ulong, TypeName>();

    private sealed class TypeName
    {
      public TypeName(string name, string identifier, TypeName parent)
      {
        this.Name = name;
        this.Identifier = identifier;
        this.Parent = parent;
      }

      public string Name { get; }
      public string Identifier { get; }
      public TypeName Parent { get; }
      public TypeName SideContainer { get; set; }

      public TypeName GetCommonParent(TypeName targetName)
      {
        if (targetName == null) return null;
        
        while (targetName != null)
        {
          if (targetName == this) return this;

          targetName = targetName.Parent;
        }

        return this.Parent?.GetCommonParent(targetName);
      }

      public override string ToString() => $"{this.Parent?.ToString()}.{this.Name}";
    }

    public CSharpCodeGenerator(CodeGeneratorRequest request)
      : this(request, "Schema")
    {
    }

    public CSharpCodeGenerator(CodeGeneratorRequest request, string defaultNamespace)
    {
      _request = request;
      this.DefaultNamespace = defaultNamespace;
      _generator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);
    }
    
    public string DefaultNamespace { get; set; }
    public OptionKey CSharpFormattingOption { get; private set; }

    private Node this[ulong id] => _request.nodes.First(n => n.id == id);

    public IReadOnlyDictionary<string, string> GenerateSources()
    {
      return _request.requestedFiles
        .Select(f => new
        {
          name = Path.GetFileNameWithoutExtension(f.filename.ToString()),
          content = this.GenerateFile(f),
        })
        .ToDictionary(
          x => x.name,
          x => x.content);
    }

    private void BuildNames(TypeName parent, Node node, string name)
    {
      TypeName typeName;

      if (node.Is(out Node.structGroup @struct))
      {
        typeName = new TypeName(name, _generator.IdentifierName(name).ToString(), parent);
        _typeNames.Add(node.id, typeName);
        foreach (var groupField in @struct.fields.Where(f => f.which == Field.Union.group))
        {
          this.BuildNames(typeName, this[groupField.group.typeId], $"{groupField.name.ToString()}Group");
        }
      }
      else if (node.Is(out Node.interfaceGroup @interface))
      {
        typeName = new TypeName("I" + name, _generator.IdentifierName("I" + name).ToString(), parent);
        _typeNames.Add(node.id, typeName);
        TypeName container = null;
        foreach (var method in @interface.methods)
        {
          var n = this[method.paramStructType];
          if (n.scopeId == 0 && (n.Is(out Node.structGroup s) == false || s.dataWordCount + s.pointerCount > 0))
          {
            if (container == null)
            {
              container = new TypeName(name, _generator.IdentifierName(name).ToString(), parent);
            }

            this.BuildNames(container, n, method.name + "Params");
          }

          n = this[method.resultStructType];
          if (n.scopeId == 0 && (n.Is(out s) == false || s.dataWordCount + s.pointerCount > 0))
          {
            if (container == null)
            {
              container = new TypeName(name, _generator.IdentifierName(name).ToString(), parent);
            }

            this.BuildNames(container, n, method.name + "Results");
          }
        }

        typeName.SideContainer = container;
      }
      else
      {
        typeName = new TypeName(name, _generator.IdentifierName(name).ToString(), parent);
        _typeNames.Add(node.id, typeName);
      }

      foreach (var nn in node.nestedNodes)
      {
        this.BuildNames(typeName, this[nn.id], nn.name.ToString());
      }
    }

    private string GenerateFile(CodeGeneratorRequest.RequestedFile file)
    {
      var node = this[file.id];

      var @namespace = this.GetNamespace(node);

      this.BuildNames(null, node, "global::" + @namespace);
      
      var src = $@"namespace {@namespace}
{{
  {string.Join("\n", node.nestedNodes.SelectMany(this.GenerateNode))}
}}
";
      var tree = SyntaxFactory.ParseSyntaxTree(src);
      var root = tree.GetRoot()
        .NormalizeWhitespace("  ", false);
      return new WhitespaceRewriter().Visit(root).ToString();
    }

    private IEnumerable<string> GenerateNode(Node.NestedNode nestedNode)
    {
      return this.GenerateNode(this[nestedNode.id]);
    }

    private IEnumerable<string> GenerateNode(Node node)
    {
      var typeName = _typeNames[node.id];
      return node.which == Node.Union.@struct ? this.GenerateStruct(typeName, node)
        : node.which == Node.Union.@enum ? this.GenerateEnum(typeName, node)
        : node.which == Node.Union.@interface ? this.GenerateInterface(typeName, node)
        : node.which == Node.Union.@const ? this.GenerateConst(typeName, node)
        : node.which == Node.Union.annotation ? this.GenerateAnnotation(typeName, node)
        : null;
    }

    private IEnumerable<string> GenerateStruct(TypeName name, Node node)
    {
      var s = node.@struct;

      yield return $@"
        {this.GetDocComment(node)}
        public struct {name.Identifier}{this.GetGenericArgs(node)} : {CnNamespace}.IStruct
        {{
          public const int KNOWN_DATA_WORDS = {s.dataWordCount.ToString()};
          public const int KNOWN_POINTER_WORDS = {s.pointerCount.ToString()};
          private {StructType} _s;
          public {name.Identifier}(ref {CnNamespace}.AllocationContext allocContext) : this(allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)) {{ }}
          public {name.Identifier}({MessageType} m) : this(m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS) {{ }}
          public {name.Identifier}({MessageType} m, ushort dataWords, ushort pointers) : this(m.Allocate(dataWords, pointers)) {{ }}
          public {name.Identifier}({StructType} s) {{ _s = s; }}

          {StructType} {CnNamespace}.IStruct.Struct {{ get {{ return _s; }} set {{ _s = value; }} }}";

      foreach (var code in this.GenerateMembers(name, s))
      {
        yield return code;
      }

      foreach (var code in node.nestedNodes.SelectMany(this.GenerateNode))
      {
        yield return code;
      }

      yield return "}";
    }

    private string GetGenericArgs(Node node)
    {
      return node.isGeneric && node.parameters.Count > 0
        ? $"<{string.Join(", ", node.parameters.Select(p => this.ToName(p.name)))}>"
        : string.Empty;
    }

    private IEnumerable<string> GenerateEnum(TypeName name, Node node)
    {
      var @enum = node.@enum;
      yield return $@"
        {GetDocComment(node)}
        public enum {name.Identifier} : ushort
        {{
          {string.Join(",\n", @enum.enumerants
            .Select((e, i) => new { e.codeOrder, ordinal = i, enumerant = e })
            .OrderBy(e => e.codeOrder)
            .Select(e => $@"
              {GetDocComment(e.enumerant.annotations)}
              {ToName(e.enumerant.name)} = {e.ordinal}"))}
        }}";
    }

    private IEnumerable<string> GenerateInterface(TypeName name, Node node)
    {
      var i = node.@interface;
      yield return $@"
        {this.GetDocComment(node)}
        public interface {name.Identifier}{this.GetGenericArgs(node)} {GetSupertypes(i.superclasses)}
        {{";
      
      foreach (var tuple in i.methods.Select((method, ordinal) => ((method, ordinal))).OrderBy(m => m.Item1.codeOrder))
      {
        var (method, ordinal) = tuple;
        var returnType = _typeNames.ContainsKey(method.resultStructType)
          ? $"{this.GetTypeName(name, this[method.resultStructType])}"
          : "void";
        
        var genericArgs = method.implicitParameters.Count > 0
          ? $"<{string.Join(", ", method.implicitParameters.Select(p => this.ToName(p.name)))}>"
          : string.Empty;

        var parameter = _typeNames.ContainsKey(method.paramStructType)
          ? $"{this.GetTypeName(name, this[method.paramStructType])} parameters"
          : string.Empty;
        
        yield return $@"
          {GetDocComment(method.annotations)}
          [{CnNamespace}.Ordinal({ordinal})]
          {returnType} {this.ToName(method.name)}{genericArgs}({parameter});";
        ordinal++;
      }

      yield return "}";

      if (name.SideContainer != null)
      {
        yield return $@"
          public static class {name.SideContainer.Identifier}
          {{";

        foreach (var code in _typeNames
          .Where(kvp => kvp.Value.Parent == name.SideContainer)
          .SelectMany(kvp => this.GenerateNode(this[kvp.Key])))
        {
          yield return code;
        }
        
        yield return "}";
      }

      string GetSupertypes(CompositeList<Superclass> extends)
      {
        var names = string.Join(", ", extends.Select(e => this.GetTypeName(name.Parent, e)));
        return string.IsNullOrEmpty(names) ? string.Empty : " : " + names;
      }
    }

    private IEnumerable<string> GenerateConst(TypeName name, Node node)
    {
      var c = node.@const;
      this.GetTypeInfo(name.Parent, c.type, out var type, out var accessor);
      yield return $@"
        {this.GetDocComment(node)}
        public const {type} {name.Identifier} = {this.GetDefaultLiteral(c.value, false)};";
    }

    private IEnumerable<string> GenerateAnnotation(TypeName name, Node node)
    {
      yield break;
    }
    
    private void GetTypeInfo(TypeName container, Type t, out string type, out string accessor)
    {
      switch (t.which)
      {
        case Type.Union.@void: type = $"{CnNamespace}.Void"; accessor = null; break;
        case Type.Union.@bool: type = "bool"; accessor = "Bool"; break;
        case Type.Union.int8: type = "sbyte"; accessor = "Int8"; break;
        case Type.Union.int16: type = "short"; accessor = "Int16"; break;
        case Type.Union.int32: type = "int"; accessor = "Int32"; break;
        case Type.Union.int64: type = "long"; accessor = "Int64"; break;
        case Type.Union.uint8: type = "byte"; accessor = "UInt8"; break;
        case Type.Union.uint16: type = "ushort"; accessor = "UInt16"; break;
        case Type.Union.uint32: type = "uint"; accessor = "UInt32"; break;
        case Type.Union.uint64: type = "ulong"; accessor = "UInt64"; break;
        case Type.Union.float32: type = "float"; accessor = "Float32"; break;
        case Type.Union.float64: type = "double"; accessor = "Float64"; break;
        case Type.Union.text: type = $"{CnNamespace}.Text"; accessor = "Text"; break;
        case Type.Union.data: type = $"{CnNamespace}.PrimitiveList<byte>"; accessor = "List<byte>"; break;
        case Type.Union.anyPointer: type = $"{CnNamespace}.AbsPointer"; accessor = "AbsPointer "; break;
        case Type.Union.list:
          var elemType = t.list.elementType;
          if (elemType.which == Type.Union.@bool)
          {
            type = "BoolList";
            accessor = "BoolList";
          }
          else if (elemType.which >= Type.Union.int8
           && elemType.which <= Type.Union.float64)
          {
            string elemTypeName, elemAccessor;
            this.GetTypeInfo(container, elemType, out elemTypeName, out elemAccessor);
            type = $"PrimitiveList<{elemTypeName}>";
            accessor = $"List<{elemTypeName}>";
          }
          else if (elemType.which == Type.Union.@enum)
          {
            type = this.GetTypeName(container, elemType.@enum.typeId);
            accessor = "UInt16";
          }
          else if (elemType.which >= Type.Union.text
                && elemType.which <= Type.Union.@interface)
          {
            string elemTypeName, elemAccessor;
            this.GetTypeInfo(container, elemType, out elemTypeName, out elemAccessor);
            type = $"{CnNamespace}.CompositeList<{elemTypeName}>";
            accessor = $"CompositeList<{elemTypeName}>";
          }
          else if (elemType.which == Type.Union.anyPointer)
          {
            type = $"{CnNamespace}.Pointer";
            accessor = "RawPointer";
          }
          else if (elemType.which == Type.Union.@void)
          {
            type = $"{CnNamespace}.FlatArray<{CnNamespace}.Void>";
            accessor = "FlatArray";
          }
          else throw new System.InvalidOperationException($"Unexpected element type {elemType.which}");
          break;
        case Type.Union.@enum:
          type = this.GetTypeName(container, t.@enum.typeId);
          accessor = "UInt16";
          break;
        case Type.Union.@struct:
          type = this.GetTypeName(container, t.@struct.typeId);
          accessor = $"Struct<{type}>";
          break;
        case Type.Union.@interface:
          type = this.GetTypeName(container, t.@interface.typeId);
          accessor = $"Interface<{type}>";
          break;
        default:
          throw new System.InvalidOperationException($"Unexpected type {t.which}");
      }
    }

    private string ToName(Text name) => _generator.IdentifierName(name.ToString()).ToString();

    private IEnumerable<string> GenerateMembers(TypeName container, Node.structGroup s)
    {
      if (s.discriminantCount > 0)
      {
        var unionFieldList = s.fields
          .Where(f => f.discriminantValue != Field.noDiscriminant)
          .OrderBy(f => f.codeOrder)
          .ToList();
        yield return $@"
          public enum Union : ushort
          {{
            {string.Join("\n", unionFieldList.Select(f => $"{this.ToName(f.name)} = {f.discriminantValue},"))}
          }}";
        foreach (var field in unionFieldList.Where(f => f.which == Field.Union.group))
        {
          var discriminantName = this.ToName(field.name);
          var type = this.GetTypeName(container, field.group.typeId);
          var returnValueName = discriminantName == "ret" ? "ret1" : "ret";
          yield return $@"
            public bool Is(out {type} {discriminantName})
            {{
              var {returnValueName} = this.which == Union.{discriminantName};
              {discriminantName} = new {type}({returnValueName} ? _s : default({StructType}));
              return {returnValueName};
            }}";
        }
        foreach (var field in unionFieldList
            .Where(f => f.Is(out Field.slotGroup slot) && slot.type.which == Type.Union.@struct))
        {
          var discriminantName = this.ToName(field.name);
          var type = this.GetTypeName(container, field.slot.type.@struct.typeId);
          var returnValueName = discriminantName == "ret" ? "ret1" : "ret";
          yield return $@"
            public bool Is(out {type} {discriminantName})
            {{
              var {returnValueName} = this.which == Union.{discriminantName};
              {discriminantName} = {returnValueName} ? this.{discriminantName} : default({type});
              return {returnValueName};
            }}";
        }
        yield return $@"
          public Union which
          {{
            get {{ return (Union)_s.ReadUInt16({s.discriminantOffset}); }}
            set {{ _s.WriteUInt16({s.discriminantOffset}, (ushort)value); }}
          }}";
      }

      foreach (var field in s.fields.OrderBy(f => f.codeOrder))
      {
        var name = ToName(field.name);
        if (field.which == Field.Union.slot)
        {
          var slot = field.slot;
          if (slot.type.which == Type.Union.@void)
          {
            continue;
          }

          string type, accessor;
          this.GetTypeInfo(container, slot.type, out type, out accessor);
          if (slot.type.which == Type.Union.@enum)
          {
            yield return $@"
              {GetDocComment(field.annotations)}
              public {type} {name}
              {{
                get {{ return ({type})_s.ReadUInt16({slot.offset}  {GetDefaultLiteral(slot.defaultValue)}); }}
                set {{ _s.WriteUInt16({slot.offset}, (ushort)value {GetDefaultLiteral(slot.defaultValue)}); }}
              }}";
          }
          else if (slot.type.which == Type.Union.@struct
                || slot.type.which == Type.Union.@interface
                || slot.type.which == Type.Union.list
                || slot.type.which == Type.Union.text
                || slot.type.which == Type.Union.data
                || slot.type.which == Type.Union.anyPointer)
          {
            yield return $@"
              {GetDocComment(field.annotations)}
              public {type} {name}
              {{
                get {{ return _s.Dereference{accessor}({slot.offset}); }}
                set {{ _s.WritePointer({slot.offset}, value); }}
              }}";
          }
          else
          {
            yield return $@"
              {GetDocComment(field.annotations)}
              public {type} {name}
              {{
                get {{ return _s.Read{accessor}({slot.offset}  {GetDefaultLiteral(slot.defaultValue)}); }}
                set {{ _s.Write{accessor}({slot.offset}, value {GetDefaultLiteral(slot.defaultValue)}); }}
              }}";
          }
        }
        else if (field.which == Field.Union.group)
        {
          string groupName = _typeNames[field.group.typeId].Identifier;

          yield return $@"
            {GetDocComment(field.annotations)}
            public {groupName} {name} => new {groupName}(_s);
            public struct {groupName}
            {{
              private readonly {StructType} _s;
              public {groupName}({StructType} s) {{ _s = s; }}
            ";
          foreach (var member in this.GenerateMembers(
            container,
            _request.nodes.First(n => n.id == field.group.typeId && n.which == Node.Union.@struct).@struct))
          {
            yield return member;
          }

          yield return "}";
        }
        else
        {
          throw new System.InvalidOperationException($"Unexpected field type {field.which}");
        }
      }
    }
    
    private string GetTypeName(TypeName container, Superclass super) => this.GetTypeName(container, super.id);

    private string GetTypeName(TypeName container, Node node) => this.GetTypeName(container, node.id);

    private string GetTypeName(TypeName container, ulong typeId)
    {
      var targetName = _typeNames[typeId];
      var name = targetName.Identifier;

      var importedNames = new List<TypeName>();
      var parent = container;
      while (parent != null)
      {
        importedNames.Add(parent);
        parent = parent.Parent;
      }

      parent = targetName.Parent;
      while (importedNames.Contains(parent) == false)
      {
        name = $"{parent.Identifier}.{name}";
        parent = parent.Parent;
      }

      return name;
    }

    private string GetDefaultLiteral(Value val, bool leadingComma = true)
    {
      string ret;
      if (val.which == Value.Union.@bool)        ret = val.@bool ? "true" : null;
      else if (val.which == Value.Union.int8)    ret = val.int8 != 0 ? SyntaxFactory.Literal(val.int8).ToString() : null;
      else if (val.which == Value.Union.int16)   ret = val.int16 != 0 ? SyntaxFactory.Literal(val.int16).ToString() : null;
      else if (val.which == Value.Union.int32)   ret = val.int32 != 0 ? SyntaxFactory.Literal(val.int32).ToString() : null;
      else if (val.which == Value.Union.int64)   ret = val.int64 != 0 ? SyntaxFactory.Literal(val.int64).ToString() : null;
      else if (val.which == Value.Union.uint8)   ret = val.uint8 != 0 ? SyntaxFactory.Literal(val.uint8).ToString() : null;
      else if (val.which == Value.Union.uint16)  ret = val.uint16 != 0 ? SyntaxFactory.Literal(val.uint16).ToString() : null;
      else if (val.which == Value.Union.uint32)  ret = val.uint32 != 0 ? SyntaxFactory.Literal(val.uint32).ToString() : null;
      else if (val.which == Value.Union.uint64)  ret = val.uint64 != 0 ? SyntaxFactory.Literal(val.uint64).ToString() : null;
      else if (val.which == Value.Union.float32) ret = val.float32 != 0 ? SyntaxFactory.Literal(val.float32).ToString() : null;
      else if (val.which == Value.Union.float64) ret = val.float64 != 0 ? SyntaxFactory.Literal(val.float64).ToString() : null;
      else if (val.which == Value.Union.@enum)   ret = val.@enum != 0 ? SyntaxFactory.Literal(val.@enum).ToString() : null;
      else if (val.which == Value.Union.@void)   ret = $"default({CnNamespace}.Void)";
      else if (val.which == Value.Union.text)    ret = SyntaxFactory.Literal(val.text.ToString()).ToString();
      else ret = $"default /*not yet supported*/";
      //else throw new System.ArgumentException($"Value not a primitive type: {val.which}");
      
      return ret == null ? string.Empty
        : leadingComma ? ", " + ret
        : ret;
    }

    private string GetDocComment(Node node)
    {
      return this.GetDocComment(node.annotations);
    }

    private string GetDocComment(CompositeList<Annotation> annotations)
    {
      // TODO
      return string.Empty;
    }

    private string GetNamespace(Node node)
    {
      var ns = node.annotations
        .Where(a => a.id == NamespaceAnnotationId && a.value.which == Value.Union.text)
        .Select(a => a.value.text.ToString())
        .FirstOrDefault();

      return string.IsNullOrEmpty(ns) ? this.DefaultNamespace : ns;
    }
  }
}