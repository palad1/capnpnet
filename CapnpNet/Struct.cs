using CapnpNet.Rpc;
using System;
using System.Runtime.CompilerServices;

namespace CapnpNet
{
  public interface IStruct
  {
    Struct Struct { get; set; }
  }

  public static class StructExtensions
  {
    public static T As<T>(this Struct s) where T : struct, IStruct => new T { Struct = s };

    public static Struct GetStruct<T>(this T structObj) where T : struct, IStruct => structObj.Struct;

    public static bool Compact<T>(this T structObj, bool dataOnly = true) where T : struct, IStruct
    {
      // FIXME: check for upgraded list struct
      var s = structObj.Struct;
      var originalEnd = s.StructWordOffset + s.DataWords + s.PointerWords;
      int savedWords = 0;

      var dw = s.DataWords;
      while (dw > 0 && s.ReadInt64(dw - 1) == 0)
      {
        dw--;
        savedWords++;
      }

      var pw = s.PointerWords;
      if (dataOnly == false)
      {
        while (pw > 0 && s.ReadRawPointer(pw - 1).RawValue == 0)
        {
          pw--;
          savedWords++;
        }
      }

      if (savedWords == 0) return false;

      var newStruct = new Struct(s.Segment, s.StructWordOffset, dw, pw, 0);

      var pointerShift = s.DataWords - dw;
      if (pointerShift > 0)
      {
        // need to shift pointers over
        for (int i = 0; i < pw; i++)
        {
          var ptr = s.ReadRawPointer(i);
          if (ptr.Type == PointerType.Struct || ptr.Type == PointerType.List) ptr.WordOffset += pointerShift;

          newStruct.WriteRawPointer(i, ptr);
        }
      }

      var seg = s.Segment;
      for (int i = 0; i < savedWords; i++)
      {
        seg[originalEnd - i - 1 | Word.unit] = 0;
      }
      
      structObj.Struct = newStruct;

      seg.TryReclaim(originalEnd, savedWords);

      return true;
    }

    public static T CopyTo<T>(this T structObj, Message dest) where T : struct, IStruct
    {
      return new T() { Struct = structObj.Struct.CopyTo(dest) };
    }
  }

  public struct Struct
  {
    private readonly Segment _segment;
    private readonly int _structWordOffset; // in the encoding spec, technically this is a uint, but currently I bottom out at an API that uses int :(
    private readonly ushort _dataWords, _pointerWords;
    private readonly byte _upgradedListElementByteOffset; // FIXME seems like this will have trouble detecting first element in a word

    public static bool operator ==(Struct a, Struct b)
    {
      return a._segment == b._segment
        && a._structWordOffset == b._structWordOffset
        && a._dataWords == b._dataWords
        && a._pointerWords == b._pointerWords
        && a._upgradedListElementByteOffset == b._upgradedListElementByteOffset;
    }

    public static bool operator !=(Struct a, Struct b) => !(a == b);
    
    public override bool Equals(object obj) => obj is Struct s && this == s;
    
    public override int GetHashCode()
    {
      return _segment.GetHashCode() * 17 + _structWordOffset;
    }

    public Struct(Segment segment, int pointerOffset, StructPointer pointer)
      : this(
          segment,
          pointerOffset + pointer.WordOffset,
          pointer.DataWords,
          pointer.PointerWords,
          0)
    {
    }

    public Struct(Segment segment, int structWordOffset, ushort dataWords, ushort pointerWords, byte upgradedListElementSize)
    {
      _segment = segment;
      _structWordOffset = structWordOffset;
      _dataWords = dataWords;
      _pointerWords = pointerWords;
      _upgradedListElementByteOffset = upgradedListElementSize;
    }

    public Pointer[] PointersDebug
    {
      get
      {
        var ret = new Pointer[_pointerWords];
        ref var pointers = ref Unsafe.As<ulong, Pointer>(ref _segment[_structWordOffset + _dataWords | Word.unit]);
        for (int i = 0; i < _pointerWords; i++)
        {
          ret[i] = Unsafe.Add(ref pointers, i);
        }

        return ret;
      }
    }

#if SPAN
    // <summary>
    // Note, when _upgradedListElementOffset > 0, Span will be one word long, and may comprise of multiple structs.
    // </summary>
    public Span<ulong> Span => _segment.Span.Slice((int)_structWordOffset, _dataWords + _pointerWords);
#endif

    public int StructWordOffset => _structWordOffset;
    public Segment Segment => _segment;
    public ushort DataWords => _dataWords;
    public ushort PointerWords => _pointerWords;

    public override string ToString() => $"Struct(S={_segment.SegmentIndex}, O={_structWordOffset}, DW={_dataWords}, PW={_pointerWords})";
    
    public int CalculateSize()
    {
      // TODO: traversal depth limit
      var size = this.DataWords + this.PointerWords;

      for (int i = 0; i < this.PointerWords; i++)
      {
        var ptr = this.ReadRawPointer(i);
        var dataSeg = _segment;
        var srcMsg = _segment.Message;
        // TODO: only dereference far; don't count against transversal limit
        srcMsg.Traverse(ref ptr, ref dataSeg, out int baseOffset);
        
        if (ptr.Type == PointerType.Struct)
        {
          size += this.DereferenceRawStruct(i).CalculateSize();
        }
        else if (ptr.Is(out ListPointer list))
        {
          int elementsPerWord;
          if (list.ElementSize == ElementSize.OneBit) elementsPerWord = 64;
          else if (list.ElementSize == ElementSize.OneByte) elementsPerWord = 8;
          else if (list.ElementSize == ElementSize.TwoBytes) elementsPerWord = 4;
          else if (list.ElementSize == ElementSize.FourBytes) elementsPerWord = 2;
          else if (list.ElementSize >= ElementSize.EightBytesNonPointer) elementsPerWord = 1;
          else throw new NotSupportedException();

          var words = ((int)list.ElementCount + elementsPerWord - 1) / elementsPerWord;
          if (list.ElementSize == ElementSize.Composite) words++;

          size += words;
        }
      }

      return size;
    }

    public Struct CopyTo(Message dest)
    {
      //if (this == default(Struct)) 

      if (this.Segment.Message == dest) return this;

      var newS = dest.Allocate(this.DataWords, this.PointerWords);
      var destSeg = newS.Segment;
      var srcSeg = this.Segment;
      var srcMsg = srcSeg.Message;

      // copy data
      for (int i = 0; i < this.DataWords; i++)
      {
        destSeg[newS.StructWordOffset + i | Word.unit] = srcSeg[this.StructWordOffset + i | Word.unit];
      }

      for (int i = 0; i < this.PointerWords; i++)
      {
        CopyPointer(ref this, i, this.ReadRawPointer(i));
      }

      return newS;

      void CopyPointer(ref Struct @this, int i, Pointer ptr)
      {
        var dataSeg = srcSeg;
        var isFar = srcMsg.Traverse(ref ptr, ref dataSeg, out int baseOffset);
        if (!isFar) baseOffset = @this._structWordOffset + @this._dataWords + i + 1;

        if (ptr.Type == PointerType.Struct)
        {
          newS.WritePointer(i, @this.DereferenceRawStruct(i).CopyTo(dest));
        }
        else if (ptr.Is(out ListPointer list))
        {
          ref ulong src = ref dataSeg[baseOffset + ptr.WordOffset | Word.unit];

          int elementsPerWord;
          if (list.ElementSize == ElementSize.OneBit) elementsPerWord = 64;
          else if (list.ElementSize == ElementSize.OneByte) elementsPerWord = 8;
          else if (list.ElementSize == ElementSize.TwoBytes) elementsPerWord = 4;
          else if (list.ElementSize == ElementSize.FourBytes) elementsPerWord = 2;
          else if (list.ElementSize >= ElementSize.EightBytesNonPointer) elementsPerWord = 1;
          else
          {
            throw new NotSupportedException(); // zero
          }
          
          if (list.ElementSize == ElementSize.EightBytesPointer)
          {
            ref Pointer pointers = ref Unsafe.As<ulong, Pointer>(ref src);
            for (int j = 0; j < ptr.ElementCount; j++)
            {
            }
          }

          var words = ((int)list.ElementCount + elementsPerWord - 1) / elementsPerWord;
          if (list.ElementSize == ElementSize.Composite) words++;

          dest.Allocate(words, out int offset, out Segment newSeg);
          
          ref ulong dst = ref newSeg[offset | Word.unit];

          // TODO: follow pointers in payload according to tag
          if (list.ElementSize == ElementSize.Composite)
          {
            var tag = Unsafe.As<ulong, StructPointer>(ref src);
            var wordsPerStruct = tag.DataWords + tag.PointerWords;
            dst = src; // copy tag
            var elemOffset = 0;
            while (elemOffset < words - 1)
            {
              if (elemOffset % wordsPerStruct < tag.DataWords)
              {
                Unsafe.Add(ref dst, j + 1) = Unsafe.Add(ref src, j + 1);
              }
              else
              {

              }
            }
            
          }
          else
          {
            // simply copy data
            for (int j = 0; j < words; j++)
            {
              Unsafe.Add(ref dst, j) = Unsafe.Add(ref src, j);
            }
          }

          newS.WritePointerCore(i, newSeg, offset, list);
        }
        else if (ptr.Is(out OtherPointer other))
        {
          if (other.OtherPointerType == OtherPointerType.Capability)
          {
            newS.WritePointer(i, srcMsg.LocalCaps[(int)other.CapabilityId]);
          }
          else
          {
            // TODO: throw? some kind of warning?
            throw new NotSupportedException("Unexpected OtherPointerType");
            //newS.WriteRawPointer(i, ptr);
          }
        }
        else
        {
          throw new InvalidOperationException(); // should be unreachable
        }
      }
    }
    
    internal void ReplaceCaps(Func<uint, ICapability, uint> capReplacer)
    {
      //if (this == default(Struct)) 
      
      var srcSeg = this.Segment;
      var srcMsg = srcSeg.Message;

      for (int i = 0; i < this.PointerWords; i++)
      {
        var ptr = this.ReadRawPointer(i);
        var dataSeg = srcSeg;
        var isFar = srcMsg.Traverse(ref ptr, ref dataSeg, out int baseOffset);
        if (!isFar) baseOffset = _structWordOffset + _dataWords + i + 1;

        if (ptr.Type == PointerType.Struct)
        {
          this.DereferenceRawStruct(i).ReplaceCaps(capReplacer);
        }
        else if (ptr.Is(out ListPointer list))
        {
          // TODO
          throw new NotSupportedException();
        }
        else if (ptr.Is(out OtherPointer other))
        {
          if (isFar) throw new NotSupportedException();

          if (other.OtherPointerType == OtherPointerType.Capability)
          {
            var cap = srcMsg.LocalCaps[(int)other.CapabilityId];
            var newId = capReplacer(other.CapabilityId, cap);
            this.Pointer(i).CapabilityId = newId;
          }
          else
          {
            throw new NotSupportedException();
          }
        }
        else
        {
          throw new InvalidOperationException(); // should be unreachable
        }
      }
    }

    private ref Pointer Pointer(int index)
    {
      Check.Range(index, _pointerWords);
      return ref Unsafe.As<ulong, Pointer>(ref _segment[_structWordOffset + _dataWords + index | Word.unit]);
    }

#region Read methods
    public Pointer ReadRawPointer(int pointerIndex)
    {
      Check.Positive(pointerIndex);

      // TODO: how to handle default?
      if (_upgradedListElementByteOffset > 0 || pointerIndex >= this.PointerWords) return new Pointer();

      return this.Pointer(pointerIndex);
    }

    public Struct DereferenceRawStruct(int pointerIndex)
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        return new Struct(targetSegment, baseOffset, (StructPointer)pointer);
      }

      // TODO: how to handle default?
      return new Struct();
    }

    public T DereferenceStruct<T>(int pointerIndex) where T : struct, IStruct
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        return new T
        {
          Struct = new Struct(targetSegment, baseOffset, (StructPointer)pointer)
        };
      }

      // TODO: how to handle default?
      return default(T);
    }

    public Text DereferenceText(int pointerIndex)
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        var listPointer = (ListPointer)pointer;
        TypeHelpers.AssertSize<byte>(listPointer.ElementSize);

        return new Text(targetSegment, baseOffset, listPointer);
      }

      // TODO: how to handle default?
      return new Text();
    }

    public PrimitiveList<T> DereferenceList<T>(int pointerIndex)
      where T : struct
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        var listPointer = (ListPointer)pointer;

        if (listPointer.ElementSize == ElementSize.Zero) return new PrimitiveList<T>(); // ummm...

        TypeHelpers.AssertSize<T>(listPointer.ElementSize);

        return new PrimitiveList<T>(targetSegment, baseOffset, listPointer);
      }

      // TODO: how to handle default?
      return new PrimitiveList<T>();
    }

    public BoolList DereferenceBoolList(int pointerIndex)
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        var listPointer = (ListPointer)pointer;

        if (listPointer.ElementSize != ElementSize.OneBit) throw new ArgumentException($"Expected list with element size of one bit, but found {pointer.ElementSize}");
      
        return new BoolList(
          targetSegment,
          baseOffset,
          listPointer);
      }

      // TODO: how to handle default?
      return new BoolList();
    }

    public CompositeList<T> DereferenceCompositeList<T>(int pointerIndex) where T : struct, IStruct
    {
      if (this.DereferenceCore(pointerIndex, out var pointer, out var baseOffset, out var targetSegment))
      {
        var listPointer = (ListPointer)pointer;

        if (listPointer.ElementSize != ElementSize.Composite) throw new ArgumentException($"Expected composite list, but found {listPointer.ElementSize} list");
      
        return new CompositeList<T>(
          targetSegment,
          baseOffset,
          listPointer);
      }

      // TODO: how to handle default?
      return new CompositeList<T>();
    }

    // not sure about this factoring; does this reduce the amount of generated code for the generic methods?
    private bool DereferenceCore(int pointerIndex, out Pointer pointer, out int baseOffset, out Segment targetSegment)
    {
      Check.Positive(pointerIndex);

      if (_upgradedListElementByteOffset > 0 || pointerIndex >= this.PointerWords)
      {
        pointer = default(Pointer);
        baseOffset = 0;
        targetSegment = null;
        return false;
      }
      
      pointer = this.Pointer(pointerIndex);
      
      if (pointer == default(Pointer))
      {
        baseOffset = 0;
        targetSegment = null;
        return false;
      }

      targetSegment = _segment;
      if (!_segment.Message.Traverse(ref pointer, ref targetSegment, out baseOffset))
      {
        baseOffset = _structWordOffset + _dataWords + pointerIndex + 1;
      }
      
      return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T GetOrDefault<T>(int index)
      where T : struct
    {
      Check.Positive(index);

      if (_upgradedListElementByteOffset > 0)
      {
        // this is a list element upgraded to a struct; only the first field is present
        if (index > 0) return default(T);
        
        return Unsafe.As<byte, T>(ref _segment[_structWordOffset * sizeof(ulong) + _upgradedListElementByteOffset / Unsafe.SizeOf<T>() | Byte.unit]);
      }
      else
      {
        if (index * Unsafe.SizeOf<T>() >= _dataWords * sizeof(ulong)) return default(T);

        return Unsafe.As<byte, T>(ref _segment[_structWordOffset * sizeof(ulong) + index * Unsafe.SizeOf<T>() | Byte.unit]);
      }
    }

    // TODO: option which throws instead of return default for out of range?
    // TODO: aggressiveinlining attribute? Also, if inlined, does defaultValue = 0 get optimized away?
    // TODO: take a second look at these xor operations for the smaller integers
    public sbyte ReadInt8(int index, sbyte defaultValue = 0) => (sbyte)(this.GetOrDefault<sbyte>(index) ^ defaultValue);
    public byte ReadUInt8(int index, byte defaultValue = 0) => (byte)(this.GetOrDefault<byte>(index) ^ defaultValue);
    public short ReadInt16(int index, short defaultValue = 0) => (short)(this.GetOrDefault<short>(index) ^ defaultValue);
    public ushort ReadUInt16(int index, ushort defaultValue = 0) => (ushort)(this.GetOrDefault<ushort>(index) ^ defaultValue);
    public int ReadInt32(int index, int defaultValue = 0) => this.GetOrDefault<int>(index) ^ defaultValue;
    public uint ReadUInt32(int index, uint defaultValue = 0) => this.GetOrDefault<uint>(index) ^ defaultValue;
    public long ReadInt64(int index, long defaultValue = 0) => this.GetOrDefault<long>(index) ^ defaultValue;
    public ulong ReadUInt64(int index, ulong defaultValue = 0) => this.GetOrDefault<ulong>(index) ^ defaultValue;
    public float ReadFloat32(int index, float defaultValue = 0) => TypeHelpers.Xor(this.GetOrDefault<float>(index), defaultValue);
    public double ReadFloat64(int index, double defaultValue = 0) => TypeHelpers.Xor(this.GetOrDefault<double>(index), defaultValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool(int index, bool defaultValue = false)
    {
      // bool list elements can't be upgraded to struct
      if (_upgradedListElementByteOffset > 0) return defaultValue;

      var wordIndex = index >> 6;
      if (wordIndex >= this.DataWords) return defaultValue;
      
      // TODO: byte-sized mask?
      var mask = 1UL << (index & 63);
      return (_segment[_structWordOffset + wordIndex | Word.unit] & mask) > 0 != defaultValue;
    }

#endregion Read methods

#region Write methods
    public void WriteRawPointer(int pointerIndex, Pointer pointer)
    {
      // TODO: disallow writing arbitrary capability pointers
      if (pointerIndex < 0 || pointerIndex >= this.PointerWords)
      {
        throw new ArgumentOutOfRangeException("pointerIndex", "Pointer index out of range");
      }

      this.Pointer(pointerIndex) = pointer;
    }

    public void WritePointer<T>(int pointerIndex, PrimitiveList<T> dest)
      where T : struct
    {
      this.WritePointerCore(
        pointerIndex,
        dest.Segment,
        dest.ListWordOffset,
        new ListPointer
        {
          Type = PointerType.List,
          ElementSize = TypeHelpers.ToElementSize<T>(),
          ElementCount = (uint)dest.Count,
        });
    }

    public void WritePointer(int pointerIndex, Text dest)
    {
      this.WritePointerCore(
        pointerIndex,
        dest.Segment,
        dest.ListWordOffset,
        new ListPointer
        {
          Type = PointerType.List,
          ElementSize = ElementSize.OneByte,
          ElementCount = (uint)dest.Count,
        });
    }

    public void WritePointer(int pointerIndex, BoolList dest)
    {
      this.WritePointerCore(
        pointerIndex,
        dest.Segment,
        dest.ListWordOffset,
        new ListPointer
        {
          Type = PointerType.List,
          ElementSize = ElementSize.OneBit,
          ElementCount = (uint)dest.Count,
        });
    }

    public void WritePointer<T>(int pointerIndex, CompositeList<T> dest) where T : struct, IStruct
    {
      this.WritePointerCore(
        pointerIndex,
        dest.Segment,
        dest.TagWordOffset,
        new ListPointer
        {
          Type = PointerType.List,
          ElementSize = ElementSize.Composite,
          ElementCount = (uint)dest.Count,
        });
    }
    
    public void WritePointer(int pointerIndex, Struct s)
    {
      this.WritePointerCore(
        pointerIndex,
        s.Segment,
        s.StructWordOffset,
        new StructPointer
        {
          Type = PointerType.Struct,
          DataWords = s.DataWords,
          PointerWords = s.PointerWords,
        });
    }
    
    public void WritePointer<T>(int pointerIndex, T dest) where T : struct, IStruct
    {
      var s = dest.Struct;
      this.WritePointerCore(
        pointerIndex,
        s.Segment,
        s.StructWordOffset,
        new StructPointer
        {
          Type = PointerType.Struct,
          DataWords = s.DataWords,
          PointerWords = s.PointerWords,
        });
    }

    public void WritePointer(int pointerIndex, ICapability cap)
    {
      Check.NotNull(cap, nameof(cap));
      
      var localCaps = this.Segment.Message.LocalCaps;
      var id = localCaps.IndexOf(cap);
      if (id == -1)
      {
        id = localCaps.Count;
        localCaps.Add(cap);
      }

      var p = new Pointer()
      {
        Type = PointerType.Other,
        OtherPointerType = OtherPointerType.Capability,
        CapabilityId = (uint)id
      };

      _segment[_structWordOffset + this.DataWords + pointerIndex | Word.unit] = p.RawValue;
    }

    private void WritePointerCore(int pointerIndex, Segment destSegment, int absOffset, Pointer tag)
    {
      if (pointerIndex < 0 || pointerIndex >= this.PointerWords)
      {
        throw new ArgumentOutOfRangeException("pointerIndex", "Pointer index out of range");
      }

      var msg = _segment.Message;
      if (destSegment.Message != msg)
      {
        throw new ArgumentException("Can't point to struct in different message");
      }

      Pointer resultPointer;
      if (this.Segment == destSegment)
      {
        int pointerWordOffset = this.StructWordOffset + this.DataWords + pointerIndex + 1;
        int relWordOffset = absOffset - pointerWordOffset;
        tag.WordOffset = relWordOffset;
        resultPointer = tag;
      }
      else
      {
        if (destSegment.TryAllocate(1, out int padOffset))
        {
          tag.WordOffset = absOffset - (padOffset + 1);
          destSegment[padOffset | Word.unit] = tag.RawValue;

          resultPointer = new FarPointer
          {
            Type = PointerType.Far,
            LandingPadOffset = (uint)padOffset,
            TargetSegmentId = (uint)destSegment.SegmentIndex,
          };
        }
        else
        {
          // double-far pointer
          //int segmentIndex = -1;
          //foreach (var segment in _segment.Message.Segments)
          //{
          //  if (segment.TryAllocate(2, out padOffset))
          //  {
          //    segment[padOffset | Word.unit] = new FarPointer
          //    {
          //      Type = PointerType.Far,
          //      LandingPadOffset = (uint)absOffset,
          //      TargetSegmentId = (uint)destSegment.SegmentIndex,
          //    }.RawValue;
          //    tag.WordOffset = 0;
          //    segment[padOffset + 1 | Word.unit] = tag.RawValue;

          //    segmentIndex = segment.SegmentIndex;
          //    break;
          //  }
          //}
          
          //if (segmentIndex == -1) throw new InvalidOperationException("Cannot allocate far pointer");
          
          _segment.Message.Allocate(2, out padOffset, out Segment segment);
          segment[padOffset | Word.unit] = new FarPointer
          {
            Type = PointerType.Far,
            LandingPadOffset = (uint)absOffset,
            TargetSegmentId = (uint)destSegment.SegmentIndex
          }.RawValue;
          tag.WordOffset = 0;
          segment[padOffset + 1 | Word.unit] = tag.RawValue;

          resultPointer = new FarPointer
          {
            Type = PointerType.Far,
            IsDoubleFar = true,
            LandingPadOffset = (uint)padOffset,
            TargetSegmentId = (uint)segment.SegmentIndex,
          };
        }
      }
      
      _segment[_structWordOffset + this.DataWords + pointerIndex | Word.unit] = resultPointer.RawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write<T>(int index, T value)
      where T : struct
    {
      if (_upgradedListElementByteOffset > 0 && index > 0)
      {
        throw new InvalidOperationException("Cannot write to struct from a non-composite list");
      }
      
      Unsafe.As<byte, T>(ref _segment[_structWordOffset * sizeof(ulong) + index * Unsafe.SizeOf<T>() | Byte.unit]) = value;
    }

    public void WriteInt8(int index, sbyte value, sbyte defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteUInt8(int index, byte value, byte defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteInt16(int index, short value, short defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteUInt16(int index, ushort value, ushort defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteInt32(int index, int value, int defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteUInt32(int index, uint value, uint defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteInt64(int index, long value, long defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteUInt64(int index, ulong value, ulong defaultValue = 0) => this.Write(index, value ^ defaultValue);
    public void WriteFloat32(int index, float value, float defaultValue = 0) => this.Write(index, TypeHelpers.Xor(value, defaultValue));
    public void WriteFloat64(int index, double value, double defaultValue = 0) => this.Write(index, TypeHelpers.Xor(value, defaultValue));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(int index, bool value, bool defaultValue = false)
    {
      if (_upgradedListElementByteOffset > 0 && index > 0)
      {
        throw new InvalidOperationException("Cannot write to struct from a non-composite list");
      }

      var wordIndex = index >> 6;
      if (wordIndex >= this.DataWords) throw new IndexOutOfRangeException();

      var mask = 1UL << (index & 63);
      if (value != defaultValue)
      {
        _segment[_structWordOffset + wordIndex | Word.unit] |= mask;
      }
      else
      {
        _segment[_structWordOffset + wordIndex | Word.unit] &= ~mask;
      }
    }

#endregion Write methods
  }
}