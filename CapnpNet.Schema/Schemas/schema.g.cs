namespace CapnpNet.Schema
{
  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Node : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 5;
    public const int KNOWN_POINTER_WORDS = 6;
    private global::CapnpNet.Struct _s;
    public Node(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Node(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Node(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Node(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public enum Union : ushort
    {
      file = 0,
      @struct = 1,
      @enum = 2,
      @interface = 3,
      @const = 4,
      annotation = 5,
    }

    public bool Is(out structGroup @struct)
    {
      var ret = this.which == Union.@struct;
      @struct = new structGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out enumGroup @enum)
    {
      var ret = this.which == Union.@enum;
      @enum = new enumGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out interfaceGroup @interface)
    {
      var ret = this.which == Union.@interface;
      @interface = new interfaceGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out constGroup @const)
    {
      var ret = this.which == Union.@const;
      @const = new constGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out annotationGroup annotation)
    {
      var ret = this.which == Union.annotation;
      annotation = new annotationGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public Union which
    {
      get { return (Union)_s.ReadUInt16(6); }
      set { _s.WriteUInt16(6, (ushort)value); }
    }

    public ulong id
    {
      get { return _s.ReadUInt64(0); }
      set { _s.WriteUInt64(0, value); }
    }

    public global::CapnpNet.Text displayName
    {
      get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
      set { _s.WritePointer(0, value); }
    }

    public uint displayNamePrefixLength
    {
      get { return _s.ReadUInt32(2); }
      set { _s.WriteUInt32(2, value); }
    }

    public ulong scopeId
    {
      get { return _s.ReadUInt64(2); }
      set { _s.WriteUInt64(2, value); }
    }

    public global::CapnpNet.FlatArray<Parameter> parameters
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Parameter>>(5); }
      set { _s.WritePointer(5, value); }
    }

    public bool isGeneric
    {
      get { return _s.ReadBool(288); }
      set { _s.WriteBool(288, value); }
    }

    public global::CapnpNet.FlatArray<NestedNode> nestedNodes
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<NestedNode>>(1); }
      set { _s.WritePointer(1, value); }
    }

    public global::CapnpNet.FlatArray<Annotation> annotations
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Annotation>>(2); }
      set { _s.WritePointer(2, value); }
    }

    public structGroup @struct => new structGroup(_s);
    public struct structGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public structGroup(global::CapnpNet.Struct s) { _s = s; }
      public ushort dataWordCount
      {
        get { return _s.ReadUInt16(7); }
        set { _s.WriteUInt16(7, value); }
      }

      public ushort pointerCount
      {
        get { return _s.ReadUInt16(12); }
        set { _s.WriteUInt16(12, value); }
      }

      public ElementSize preferredListEncoding
      {
        get { return (ElementSize)_s.ReadUInt16(13); }
        set { _s.WriteUInt16(13, (ushort)value); }
      }

      public bool isGroup
      {
        get { return _s.ReadBool(224); }
        set { _s.WriteBool(224, value); }
      }

      public ushort discriminantCount
      {
        get { return _s.ReadUInt16(15); }
        set { _s.WriteUInt16(15, value); }
      }

      public uint discriminantOffset
      {
        get { return _s.ReadUInt32(8); }
        set { _s.WriteUInt32(8, value); }
      }

      public global::CapnpNet.FlatArray<Field> fields
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Field>>(3); }
        set { _s.WritePointer(3, value); }
      }
    }

    public enumGroup @enum => new enumGroup(_s);
    public struct enumGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public enumGroup(global::CapnpNet.Struct s) { _s = s; }
      public global::CapnpNet.FlatArray<Enumerant> enumerants
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Enumerant>>(3); }
        set { _s.WritePointer(3, value); }
      }
    }

    public interfaceGroup @interface => new interfaceGroup(_s);
    public struct interfaceGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public interfaceGroup(global::CapnpNet.Struct s) { _s = s; }
      public global::CapnpNet.FlatArray<Method> methods
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Method>>(3); }
        set { _s.WritePointer(3, value); }
      }

      public global::CapnpNet.FlatArray<Superclass> superclasses
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Superclass>>(4); }
        set { _s.WritePointer(4, value); }
      }
    }

    public constGroup @const => new constGroup(_s);
    public struct constGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public constGroup(global::CapnpNet.Struct s) { _s = s; }
      public Type type
      {
        get { return _s.DereferencePointer<Type>(3); }
        set { _s.WritePointer(3, value); }
      }

      public Value value
      {
        get { return _s.DereferencePointer<Value>(4); }
        set { _s.WritePointer(4, value); }
      }
    }

    public annotationGroup annotation => new annotationGroup(_s);
    public struct annotationGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public annotationGroup(global::CapnpNet.Struct s) { _s = s; }
      public Type type
      {
        get { return _s.DereferencePointer<Type>(3); }
        set { _s.WritePointer(3, value); }
      }

      public bool targetsFile
      {
        get { return _s.ReadBool(112); }
        set { _s.WriteBool(112, value); }
      }

      public bool targetsConst
      {
        get { return _s.ReadBool(113); }
        set { _s.WriteBool(113, value); }
      }

      public bool targetsEnum
      {
        get { return _s.ReadBool(114); }
        set { _s.WriteBool(114, value); }
      }

      public bool targetsEnumerant
      {
        get { return _s.ReadBool(115); }
        set { _s.WriteBool(115, value); }
      }

      public bool targetsStruct
      {
        get { return _s.ReadBool(116); }
        set { _s.WriteBool(116, value); }
      }

      public bool targetsField
      {
        get { return _s.ReadBool(117); }
        set { _s.WriteBool(117, value); }
      }

      public bool targetsUnion
      {
        get { return _s.ReadBool(118); }
        set { _s.WriteBool(118, value); }
      }

      public bool targetsGroup
      {
        get { return _s.ReadBool(119); }
        set { _s.WriteBool(119, value); }
      }

      public bool targetsInterface
      {
        get { return _s.ReadBool(120); }
        set { _s.WriteBool(120, value); }
      }

      public bool targetsMethod
      {
        get { return _s.ReadBool(121); }
        set { _s.WriteBool(121, value); }
      }

      public bool targetsParam
      {
        get { return _s.ReadBool(122); }
        set { _s.WriteBool(122, value); }
      }

      public bool targetsAnnotation
      {
        get { return _s.ReadBool(123); }
        set { _s.WriteBool(123, value); }
      }
    }

    [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
    public struct Parameter : global::CapnpNet.IStruct
    {
      public const int KNOWN_DATA_WORDS = 0;
      public const int KNOWN_POINTER_WORDS = 1;
      private global::CapnpNet.Struct _s;
      public Parameter(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
      {
      }

      public Parameter(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
      {
      }

      public Parameter(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
      {
      }

      public Parameter(global::CapnpNet.Struct s) { _s = s; }
      global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
      {
        get { return _s; }
      }

      global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
      {
        get { return _s.Pointer; }
      }

      public global::CapnpNet.Text name
      {
        get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
    public struct NestedNode : global::CapnpNet.IStruct
    {
      public const int KNOWN_DATA_WORDS = 1;
      public const int KNOWN_POINTER_WORDS = 1;
      private global::CapnpNet.Struct _s;
      public NestedNode(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
      {
      }

      public NestedNode(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
      {
      }

      public NestedNode(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
      {
      }

      public NestedNode(global::CapnpNet.Struct s) { _s = s; }
      global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
      {
        get { return _s; }
      }

      global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
      {
        get { return _s.Pointer; }
      }

      public global::CapnpNet.Text name
      {
        get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
        set { _s.WritePointer(0, value); }
      }

      public ulong id
      {
        get { return _s.ReadUInt64(0); }
        set { _s.WriteUInt64(0, value); }
      }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Field : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 3;
    public const int KNOWN_POINTER_WORDS = 4;
    private global::CapnpNet.Struct _s;
    public Field(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Field(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Field(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Field(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public enum Union : ushort
    {
      slot = 0,
      group = 1,
    }

    public bool Is(out slotGroup slot)
    {
      var ret = this.which == Union.slot;
      slot = new slotGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out groupGroup group)
    {
      var ret = this.which == Union.group;
      group = new groupGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public Union which
    {
      get { return (Union)_s.ReadUInt16(4); }
      set { _s.WriteUInt16(4, (ushort)value); }
    }

    public global::CapnpNet.Text name
    {
      get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
      set { _s.WritePointer(0, value); }
    }

    public ushort codeOrder
    {
      get { return _s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, value); }
    }

    public global::CapnpNet.FlatArray<Annotation> annotations
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Annotation>>(1); }
      set { _s.WritePointer(1, value); }
    }

    public ushort discriminantValue
    {
      get { return _s.ReadUInt16(1, 65535); }
      set { _s.WriteUInt16(1, value, 65535); }
    }

    public slotGroup slot => new slotGroup(_s);
    public struct slotGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public slotGroup(global::CapnpNet.Struct s) { _s = s; }
      public uint offset
      {
        get { return _s.ReadUInt32(1); }
        set { _s.WriteUInt32(1, value); }
      }

      public Type type
      {
        get { return _s.DereferencePointer<Type>(2); }
        set { _s.WritePointer(2, value); }
      }

      public Value defaultValue
      {
        get { return _s.DereferencePointer<Value>(3); }
        set { _s.WritePointer(3, value); }
      }

      public bool hadExplicitDefault
      {
        get { return _s.ReadBool(128); }
        set { _s.WriteBool(128, value); }
      }
    }

    public groupGroup group => new groupGroup(_s);
    public struct groupGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public groupGroup(global::CapnpNet.Struct s) { _s = s; }
      public ulong typeId
      {
        get { return _s.ReadUInt64(2); }
        set { _s.WriteUInt64(2, value); }
      }
    }

    public ordinalGroup ordinal => new ordinalGroup(_s);
    public struct ordinalGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public ordinalGroup(global::CapnpNet.Struct s) { _s = s; }
      public enum Union : ushort
      {
        @implicit = 0,
        @explicit = 1,
      }

      public Union which
      {
        get { return (Union)_s.ReadUInt16(5); }
        set { _s.WriteUInt16(5, (ushort)value); }
      }

      public ushort @explicit
      {
        get { return _s.ReadUInt16(6); }
        set { _s.WriteUInt16(6, value); }
      }
    }

    public const ushort noDiscriminant = 65535;
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Enumerant : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 1;
    public const int KNOWN_POINTER_WORDS = 2;
    private global::CapnpNet.Struct _s;
    public Enumerant(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Enumerant(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Enumerant(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Enumerant(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public global::CapnpNet.Text name
    {
      get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
      set { _s.WritePointer(0, value); }
    }

    public ushort codeOrder
    {
      get { return _s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, value); }
    }

    public global::CapnpNet.FlatArray<Annotation> annotations
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Annotation>>(1); }
      set { _s.WritePointer(1, value); }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Superclass : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 1;
    public const int KNOWN_POINTER_WORDS = 1;
    private global::CapnpNet.Struct _s;
    public Superclass(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Superclass(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Superclass(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Superclass(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public ulong id
    {
      get { return _s.ReadUInt64(0); }
      set { _s.WriteUInt64(0, value); }
    }

    public Brand brand
    {
      get { return _s.DereferencePointer<Brand>(0); }
      set { _s.WritePointer(0, value); }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Method : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 3;
    public const int KNOWN_POINTER_WORDS = 5;
    private global::CapnpNet.Struct _s;
    public Method(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Method(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Method(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Method(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public global::CapnpNet.Text name
    {
      get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
      set { _s.WritePointer(0, value); }
    }

    public ushort codeOrder
    {
      get { return _s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, value); }
    }

    public global::CapnpNet.FlatArray<Node.Parameter> implicitParameters
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Node.Parameter>>(4); }
      set { _s.WritePointer(4, value); }
    }

    public ulong paramStructType
    {
      get { return _s.ReadUInt64(1); }
      set { _s.WriteUInt64(1, value); }
    }

    public Brand paramBrand
    {
      get { return _s.DereferencePointer<Brand>(2); }
      set { _s.WritePointer(2, value); }
    }

    public ulong resultStructType
    {
      get { return _s.ReadUInt64(2); }
      set { _s.WriteUInt64(2, value); }
    }

    public Brand resultBrand
    {
      get { return _s.DereferencePointer<Brand>(3); }
      set { _s.WritePointer(3, value); }
    }

    public global::CapnpNet.FlatArray<Annotation> annotations
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Annotation>>(1); }
      set { _s.WritePointer(1, value); }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Type : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 3;
    public const int KNOWN_POINTER_WORDS = 1;
    private global::CapnpNet.Struct _s;
    public Type(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Type(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Type(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Type(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public enum Union : ushort
    {
      @void = 0,
      @bool = 1,
      int8 = 2,
      int16 = 3,
      int32 = 4,
      int64 = 5,
      uint8 = 6,
      uint16 = 7,
      uint32 = 8,
      uint64 = 9,
      float32 = 10,
      float64 = 11,
      text = 12,
      data = 13,
      list = 14,
      @enum = 15,
      @struct = 16,
      @interface = 17,
      anyPointer = 18,
    }

    public bool Is(out listGroup list)
    {
      var ret = this.which == Union.list;
      list = new listGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out enumGroup @enum)
    {
      var ret = this.which == Union.@enum;
      @enum = new enumGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out structGroup @struct)
    {
      var ret = this.which == Union.@struct;
      @struct = new structGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out interfaceGroup @interface)
    {
      var ret = this.which == Union.@interface;
      @interface = new interfaceGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public bool Is(out anyPointerGroup anyPointer)
    {
      var ret = this.which == Union.anyPointer;
      anyPointer = new anyPointerGroup(ret ? _s : default (global::CapnpNet.Struct));
      return ret;
    }

    public Union which
    {
      get { return (Union)_s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, (ushort)value); }
    }

    public listGroup list => new listGroup(_s);
    public struct listGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public listGroup(global::CapnpNet.Struct s) { _s = s; }
      public Type elementType
      {
        get { return _s.DereferencePointer<Type>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    public enumGroup @enum => new enumGroup(_s);
    public struct enumGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public enumGroup(global::CapnpNet.Struct s) { _s = s; }
      public ulong typeId
      {
        get { return _s.ReadUInt64(1); }
        set { _s.WriteUInt64(1, value); }
      }

      public Brand brand
      {
        get { return _s.DereferencePointer<Brand>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    public structGroup @struct => new structGroup(_s);
    public struct structGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public structGroup(global::CapnpNet.Struct s) { _s = s; }
      public ulong typeId
      {
        get { return _s.ReadUInt64(1); }
        set { _s.WriteUInt64(1, value); }
      }

      public Brand brand
      {
        get { return _s.DereferencePointer<Brand>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    public interfaceGroup @interface => new interfaceGroup(_s);
    public struct interfaceGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public interfaceGroup(global::CapnpNet.Struct s) { _s = s; }
      public ulong typeId
      {
        get { return _s.ReadUInt64(1); }
        set { _s.WriteUInt64(1, value); }
      }

      public Brand brand
      {
        get { return _s.DereferencePointer<Brand>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    public anyPointerGroup anyPointer => new anyPointerGroup(_s);
    public struct anyPointerGroup
    {
      private readonly global::CapnpNet.Struct _s;
      public anyPointerGroup(global::CapnpNet.Struct s) { _s = s; }
      public enum Union : ushort
      {
        unconstrained = 0,
        parameter = 1,
        implicitMethodParameter = 2,
      }

      public bool Is(out anyPointerGroup.unconstrainedGroup unconstrained)
      {
        var ret = this.which == Union.unconstrained;
        unconstrained = new anyPointerGroup.unconstrainedGroup(ret ? _s : default (global::CapnpNet.Struct));
        return ret;
      }

      public bool Is(out anyPointerGroup.parameterGroup parameter)
      {
        var ret = this.which == Union.parameter;
        parameter = new anyPointerGroup.parameterGroup(ret ? _s : default (global::CapnpNet.Struct));
        return ret;
      }

      public bool Is(out anyPointerGroup.implicitMethodParameterGroup implicitMethodParameter)
      {
        var ret = this.which == Union.implicitMethodParameter;
        implicitMethodParameter = new anyPointerGroup.implicitMethodParameterGroup(ret ? _s : default (global::CapnpNet.Struct));
        return ret;
      }

      public Union which
      {
        get { return (Union)_s.ReadUInt16(4); }
        set { _s.WriteUInt16(4, (ushort)value); }
      }

      public unconstrainedGroup unconstrained => new unconstrainedGroup(_s);
      public struct unconstrainedGroup
      {
        private readonly global::CapnpNet.Struct _s;
        public unconstrainedGroup(global::CapnpNet.Struct s) { _s = s; }
        public enum Union : ushort
        {
          anyKind = 0,
          @struct = 1,
          list = 2,
          capability = 3,
        }

        public Union which
        {
          get { return (Union)_s.ReadUInt16(5); }
          set { _s.WriteUInt16(5, (ushort)value); }
        }
      }

      public parameterGroup parameter => new parameterGroup(_s);
      public struct parameterGroup
      {
        private readonly global::CapnpNet.Struct _s;
        public parameterGroup(global::CapnpNet.Struct s) { _s = s; }
        public ulong scopeId
        {
          get { return _s.ReadUInt64(2); }
          set { _s.WriteUInt64(2, value); }
        }

        public ushort parameterIndex
        {
          get { return _s.ReadUInt16(5); }
          set { _s.WriteUInt16(5, value); }
        }
      }

      public implicitMethodParameterGroup implicitMethodParameter => new implicitMethodParameterGroup(_s);
      public struct implicitMethodParameterGroup
      {
        private readonly global::CapnpNet.Struct _s;
        public implicitMethodParameterGroup(global::CapnpNet.Struct s) { _s = s; }
        public ushort parameterIndex
        {
          get { return _s.ReadUInt16(5); }
          set { _s.WriteUInt16(5, value); }
        }
      }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Brand : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 0;
    public const int KNOWN_POINTER_WORDS = 1;
    private global::CapnpNet.Struct _s;
    public Brand(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Brand(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Brand(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Brand(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public global::CapnpNet.FlatArray<Scope> scopes
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Scope>>(0); }
      set { _s.WritePointer(0, value); }
    }

    [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
    public struct Scope : global::CapnpNet.IStruct
    {
      public const int KNOWN_DATA_WORDS = 2;
      public const int KNOWN_POINTER_WORDS = 1;
      private global::CapnpNet.Struct _s;
      public Scope(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
      {
      }

      public Scope(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
      {
      }

      public Scope(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
      {
      }

      public Scope(global::CapnpNet.Struct s) { _s = s; }
      global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
      {
        get { return _s; }
      }

      global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
      {
        get { return _s.Pointer; }
      }

      public enum Union : ushort
      {
        bind = 0,
        inherit = 1,
      }

      public Union which
      {
        get { return (Union)_s.ReadUInt16(4); }
        set { _s.WriteUInt16(4, (ushort)value); }
      }

      public ulong scopeId
      {
        get { return _s.ReadUInt64(0); }
        set { _s.WriteUInt64(0, value); }
      }

      public global::CapnpNet.FlatArray<Binding> bind
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Binding>>(0); }
        set { _s.WritePointer(0, value); }
      }
    }

    [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
    public struct Binding : global::CapnpNet.IStruct
    {
      public const int KNOWN_DATA_WORDS = 1;
      public const int KNOWN_POINTER_WORDS = 1;
      private global::CapnpNet.Struct _s;
      public Binding(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
      {
      }

      public Binding(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
      {
      }

      public Binding(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
      {
      }

      public Binding(global::CapnpNet.Struct s) { _s = s; }
      global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
      {
        get { return _s; }
      }

      global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
      {
        get { return _s.Pointer; }
      }

      public enum Union : ushort
      {
        unbound = 0,
        type = 1,
      }

      public bool Is(out Type type)
      {
        var ret = this.which == Union.type;
        type = ret ? this.type : default (Type);
        return ret;
      }

      public Union which
      {
        get { return (Union)_s.ReadUInt16(0); }
        set { _s.WriteUInt16(0, (ushort)value); }
      }

      public Type type
      {
        get { return _s.DereferencePointer<Type>(0); }
        set { _s.WritePointer(0, value); }
      }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Value : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 2;
    public const int KNOWN_POINTER_WORDS = 1;
    private global::CapnpNet.Struct _s;
    public Value(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Value(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Value(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Value(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public enum Union : ushort
    {
      @void = 0,
      @bool = 1,
      int8 = 2,
      int16 = 3,
      int32 = 4,
      int64 = 5,
      uint8 = 6,
      uint16 = 7,
      uint32 = 8,
      uint64 = 9,
      float32 = 10,
      float64 = 11,
      text = 12,
      data = 13,
      list = 14,
      @enum = 15,
      @struct = 16,
      @interface = 17,
      anyPointer = 18,
    }

    public Union which
    {
      get { return (Union)_s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, (ushort)value); }
    }

    public bool @bool
    {
      get { return _s.ReadBool(16); }
      set { _s.WriteBool(16, value); }
    }

    public sbyte int8
    {
      get { return _s.ReadInt8(2); }
      set { _s.WriteInt8(2, value); }
    }

    public short int16
    {
      get { return _s.ReadInt16(1); }
      set { _s.WriteInt16(1, value); }
    }

    public int int32
    {
      get { return _s.ReadInt32(1); }
      set { _s.WriteInt32(1, value); }
    }

    public long int64
    {
      get { return _s.ReadInt64(1); }
      set { _s.WriteInt64(1, value); }
    }

    public byte uint8
    {
      get { return _s.ReadUInt8(2); }
      set { _s.WriteUInt8(2, value); }
    }

    public ushort uint16
    {
      get { return _s.ReadUInt16(1); }
      set { _s.WriteUInt16(1, value); }
    }

    public uint uint32
    {
      get { return _s.ReadUInt32(1); }
      set { _s.WriteUInt32(1, value); }
    }

    public ulong uint64
    {
      get { return _s.ReadUInt64(1); }
      set { _s.WriteUInt64(1, value); }
    }

    public float float32
    {
      get { return _s.ReadFloat32(1); }
      set { _s.WriteFloat32(1, value); }
    }

    public double float64
    {
      get { return _s.ReadFloat64(1); }
      set { _s.WriteFloat64(1, value); }
    }

    public global::CapnpNet.Text text
    {
      get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
      set { _s.WritePointer(0, value); }
    }

    public global::CapnpNet.Data data
    {
      get { return _s.DereferencePointer<global::CapnpNet.Data>(0); }
      set { _s.WritePointer(0, value); }
    }

    public global::CapnpNet.AbsPointer list
    {
      get { return _s.DereferencePointer<global::CapnpNet.AbsPointer>(0); }
      set { _s.WritePointer(0, value); }
    }

    public ushort @enum
    {
      get { return _s.ReadUInt16(1); }
      set { _s.WriteUInt16(1, value); }
    }

    public global::CapnpNet.AbsPointer @struct
    {
      get { return _s.DereferencePointer<global::CapnpNet.AbsPointer>(0); }
      set { _s.WritePointer(0, value); }
    }

    public global::CapnpNet.AbsPointer anyPointer
    {
      get { return _s.DereferencePointer<global::CapnpNet.AbsPointer>(0); }
      set { _s.WritePointer(0, value); }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct Annotation : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 1;
    public const int KNOWN_POINTER_WORDS = 2;
    private global::CapnpNet.Struct _s;
    public Annotation(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public Annotation(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public Annotation(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public Annotation(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public ulong id
    {
      get { return _s.ReadUInt64(0); }
      set { _s.WriteUInt64(0, value); }
    }

    public Brand brand
    {
      get { return _s.DereferencePointer<Brand>(1); }
      set { _s.WritePointer(1, value); }
    }

    public Value value
    {
      get { return _s.DereferencePointer<Value>(0); }
      set { _s.WritePointer(0, value); }
    }
  }

  public enum ElementSize : ushort
  {
    empty = 0,
    bit = 1,
    @byte = 2,
    twoBytes = 3,
    fourBytes = 4,
    eightBytes = 5,
    pointer = 6,
    inlineComposite = 7
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct CapnpVersion : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 1;
    public const int KNOWN_POINTER_WORDS = 0;
    private global::CapnpNet.Struct _s;
    public CapnpVersion(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public CapnpVersion(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public CapnpVersion(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public CapnpVersion(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public ushort major
    {
      get { return _s.ReadUInt16(0); }
      set { _s.WriteUInt16(0, value); }
    }

    public byte minor
    {
      get { return _s.ReadUInt8(2); }
      set { _s.WriteUInt8(2, value); }
    }

    public byte micro
    {
      get { return _s.ReadUInt8(3); }
      set { _s.WriteUInt8(3, value); }
    }
  }

  [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
  public struct CodeGeneratorRequest : global::CapnpNet.IStruct
  {
    public const int KNOWN_DATA_WORDS = 0;
    public const int KNOWN_POINTER_WORDS = 3;
    private global::CapnpNet.Struct _s;
    public CodeGeneratorRequest(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
    {
    }

    public CodeGeneratorRequest(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
    {
    }

    public CodeGeneratorRequest(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
    {
    }

    public CodeGeneratorRequest(global::CapnpNet.Struct s) { _s = s; }
    global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
    {
      get { return _s; }
    }

    global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
    {
      get { return _s.Pointer; }
    }

    public CapnpVersion capnpVersion
    {
      get { return _s.DereferencePointer<CapnpVersion>(2); }
      set { _s.WritePointer(2, value); }
    }

    public global::CapnpNet.FlatArray<Node> nodes
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Node>>(0); }
      set { _s.WritePointer(0, value); }
    }

    public global::CapnpNet.FlatArray<RequestedFile> requestedFiles
    {
      get { return _s.DereferencePointer<global::CapnpNet.FlatArray<RequestedFile>>(1); }
      set { _s.WritePointer(1, value); }
    }

    [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
    public struct RequestedFile : global::CapnpNet.IStruct
    {
      public const int KNOWN_DATA_WORDS = 1;
      public const int KNOWN_POINTER_WORDS = 2;
      private global::CapnpNet.Struct _s;
      public RequestedFile(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
      {
      }

      public RequestedFile(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
      {
      }

      public RequestedFile(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
      {
      }

      public RequestedFile(global::CapnpNet.Struct s) { _s = s; }
      global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
      {
        get { return _s; }
      }

      global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
      {
        get { return _s.Pointer; }
      }

      public ulong id
      {
        get { return _s.ReadUInt64(0); }
        set { _s.WriteUInt64(0, value); }
      }

      public global::CapnpNet.Text filename
      {
        get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
        set { _s.WritePointer(0, value); }
      }

      public global::CapnpNet.FlatArray<Import> imports
      {
        get { return _s.DereferencePointer<global::CapnpNet.FlatArray<Import>>(1); }
        set { _s.WritePointer(1, value); }
      }

      [global::CapnpNet.PreferredListEncoding(global::CapnpNet.ElementSize.Composite)]
      public struct Import : global::CapnpNet.IStruct
      {
        public const int KNOWN_DATA_WORDS = 1;
        public const int KNOWN_POINTER_WORDS = 1;
        private global::CapnpNet.Struct _s;
        public Import(ref global::CapnpNet.AllocationContext allocContext) : this (allocContext.Allocate(KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS))
        {
        }

        public Import(global::CapnpNet.Message m) : this (m, KNOWN_DATA_WORDS, KNOWN_POINTER_WORDS)
        {
        }

        public Import(global::CapnpNet.Message m, ushort dataWords, ushort pointers) : this (m.Allocate(dataWords, pointers))
        {
        }

        public Import(global::CapnpNet.Struct s) { _s = s; }
        global::CapnpNet.Struct global::CapnpNet.IStruct.Struct
        {
          get { return _s; }
        }

        global::CapnpNet.AbsPointer global::CapnpNet.IAbsPointer.Pointer
        {
          get { return _s.Pointer; }
        }

        public ulong id
        {
          get { return _s.ReadUInt64(0); }
          set { _s.WriteUInt64(0, value); }
        }

        public global::CapnpNet.Text name
        {
          get { return _s.DereferencePointer<global::CapnpNet.Text>(0); }
          set { _s.WritePointer(0, value); }
        }
      }
    }
  }
}