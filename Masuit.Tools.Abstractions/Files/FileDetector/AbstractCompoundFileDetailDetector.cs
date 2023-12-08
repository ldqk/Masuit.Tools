using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector;

public abstract class AbstractCompoundFileDetailDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] CfSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1 } },
    };

    /// <inheritdoc />
    protected override SignatureInformation[] SignatureInformations => CfSignatureInfo;

    public abstract IEnumerable<string> Chunks { get; }

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected abstract bool IsValidChunk(string chunkName, byte[] chunkData);

    public override bool Detect(Stream stream)
    {
        if (!base.Detect(stream))
        {
            return false;
        }

        stream.Position = 0;
        try
        {
            using var cf = new CompoundFile(stream, CFSConfiguration.LeaveOpen | CFSConfiguration.Default);
            return !(from chunk in Chunks
                     let compoundFileStream = cf.RootStorage.GetStream(chunk)
                     where compoundFileStream == null || !IsValidChunk(chunk, compoundFileStream.GetData())
                     select chunk).Any();
        }
        catch
        {
            return false;
        }
    }

    #region Modified OpenMCDF

    // -------------------------------------------------------------
    // This is a porting from java code, under MIT license of       |
    // the beautiful Red-Black Tree implementation you can find at  |
    // http://en.literateprograms.org/Red-black_tree_(Java)#chunk   |
    // Many Thanks to original Implementors.                        |
    // -------------------------------------------------------------
    internal enum Color
    { RED = 0, BLACK = 1 }

    internal enum NodeOp
    {
        LAssigned, RAssigned, CAssigned, PAssigned, VAssigned
    }

    internal interface IRBNode : IComparable
    {
        IRBNode Left { get; set; }

        IRBNode Right { get; set; }

        Color Color { get; set; }

        IRBNode Parent { get; set; }

        IRBNode Grandparent();

        IRBNode Sibling();

        IRBNode Uncle();
    }

    internal sealed class RBTree
    {
        public IRBNode Root { get; set; }

        private static Color NodeColor(IRBNode n) => n == null ? Color.BLACK : n.Color;

        public RBTree()
        { }

        public RBTree(IRBNode root)
        { Root = root; }

        private IRBNode Lookup(IRBNode template)
        {
            IRBNode n = Root;
            while (n != null)
            {
                int compResult = template.CompareTo(n);
                if (compResult == 0) return n;
                n = compResult < 0 ? n.Left : n.Right;
            }
            return n;
        }

        public bool TryLookup(IRBNode template, out IRBNode val)
        {
            val = Lookup(template);
            return val != null;
        }

        private void Replace(IRBNode oldn, IRBNode newn)
        {
            if (oldn.Parent == null) Root = newn;
            else
            {
                if (oldn == oldn.Parent.Left)
                {
                    oldn.Parent.Left = newn;
                }
                else
                {
                    oldn.Parent.Right = newn;
                }
            }
            if (newn != null) newn.Parent = oldn.Parent;
        }

        private void RotateL(IRBNode n)
        {
            IRBNode r = n.Right;
            Replace(n, r);
            n.Right = r.Left;
            if (r.Left != null) r.Left.Parent = n;
            r.Left = n;
            n.Parent = r;
        }

        private void RotateR(IRBNode n)
        {
            IRBNode l = n.Left;
            Replace(n, l);
            n.Left = l.Right;
            if (l.Right != null) l.Right.Parent = n;
            l.Right = n;
            n.Parent = l;
        }

        public void Insert(IRBNode newNode)
        {
            newNode.Color = Color.RED;

            if (Root == null) Root = newNode;
            else
            {
                var n = Root;
                while (true)
                {
                    int compResult = newNode.CompareTo(n);
                    if (compResult == 0) throw new Exception($"RBNode {newNode} already present in tree");
                    if (compResult < 0)
                    {
                        if (n.Left == null)
                        {
                            n.Left = newNode;
                            break;
                        }

                        n = n.Left;
                    }
                    else
                    {
                        if (n.Right == null)
                        {
                            n.Right = newNode;
                            break;
                        }
                        n = n.Right;
                    }
                }
                newNode.Parent = n;
            }

            Insert1(newNode);
            NodeInserted?.Invoke(newNode);
        }

        private void Insert1(IRBNode n)
        {
            if (n.Parent == null) n.Color = Color.BLACK;
            else Insert2(n);
        }

        private void Insert2(IRBNode n)
        {
            if (NodeColor(n.Parent) == Color.BLACK) return;
            Insert3(n);
        }

        private void Insert3(IRBNode n)
        {
            if (NodeColor(n.Uncle()) == Color.RED)
            {
                n.Parent.Color = Color.BLACK;
                n.Uncle().Color = Color.BLACK;
                n.Grandparent().Color = Color.RED;
                Insert1(n.Grandparent());
            }
            else Insert4(n);
        }

        private void Insert4(IRBNode n)
        {
            if (n == n.Parent.Right && n.Parent == n.Grandparent().Left)
            {
                RotateL(n.Parent);
                n = n.Left;
            }
            else if (n == n.Parent.Left && n.Parent == n.Grandparent().Right)
            {
                RotateR(n.Parent);
                n = n.Right;
            }
            Insert5(n);
        }

        private void Insert5(IRBNode n)
        {
            n.Parent.Color = Color.BLACK;
            n.Grandparent().Color = Color.RED;
            if (n == n.Parent.Left && n.Parent == n.Grandparent().Left)
                RotateR(n.Grandparent());
            else RotateL(n.Grandparent());
        }

        private static IRBNode MaximumNode(IRBNode n)
        {
            while (n.Right != null)
                n = n.Right;
            return n;
        }

        public void VisitTree(Action<IRBNode> action)
        {
            IRBNode walker = Root;
            if (walker != null)
                DoVisitTree(action, walker);
        }

        private static void DoVisitTree(Action<IRBNode> action, IRBNode walker)
        {
            while (true)
            {
                if (walker.Left != null)
                {
                    DoVisitTree(action, walker.Left);
                }

                action?.Invoke(walker);
                if (walker.Right != null)
                {
                    walker = walker.Right;
                    continue;
                }

                break;
            }
        }

        internal void VisitTreeNodes(Action<IRBNode> action)
        {
            IRBNode walker = Root;
            if (walker != null)
            {
                DoVisitTreeNodes(action, walker);
            }
        }

        private void DoVisitTreeNodes(Action<IRBNode> action, IRBNode walker)
        {
            while (true)
            {
                if (walker.Left != null)
                {
                    DoVisitTreeNodes(action, walker.Left);
                }

                action?.Invoke(walker);
                if (walker.Right != null)
                {
                    walker = walker.Right;
                    continue;
                }

                break;
            }
        }

        public class RBTreeEnumerator : IEnumerator<IRBNode>
        {
            private int position = -1;
            private Queue<IRBNode> heap = new();

            internal RBTreeEnumerator(RBTree tree)
            {
                tree.VisitTreeNodes(heap.Enqueue);
            }

            public IRBNode Current => heap.ElementAt(position);

            public void Dispose()
            { }

            object System.Collections.IEnumerator.Current => heap.ElementAt(position);

            public bool MoveNext() => ++position < heap.Count;

            public void Reset()
            { position = -1; }
        }

        public RBTreeEnumerator GetEnumerator() => new(this);

        internal void FireNodeOperation(IRBNode node, NodeOp operation)
        {
            NodeOperation?.Invoke(node, operation);
        }

        internal event Action<IRBNode> NodeInserted;

        internal event Action<IRBNode, NodeOp> NodeOperation;
    }

    /* This Source Code Form is subject to the terms of the Mozilla Public
     * License, v. 2.0. If a copy of the MPL was not distributed with this
     * file, You can obtain one at http://mozilla.org/MPL/2.0/.
     *
     * The Original Code is OpenMCDF - Compound Document Format library.
     *
     * The Initial Developer of the Original Code is Federico Blaseotto.*/

    internal enum SectorType
    {
        Normal, Mini, FAT, DIFAT, RangeLockSector, Directory
    }

    internal sealed class Sector : IDisposable
    {
        public static int MinisectorSize = 64;

        public const int Freesect = unchecked((int)0xFFFFFFFF);
        public const int Endofchain = unchecked((int)0xFFFFFFFE);
        public const int Fatsect = unchecked((int)0xFFFFFFFD);
        public const int Difsect = unchecked((int)0xFFFFFFFC);

        public bool DirtyFlag { get; set; }

        public bool IsStreamed => _stream != null && Size != MinisectorSize && Id * Size + Size < _stream.Length;

        private readonly Stream _stream;

        public Sector(int size, Stream stream)
        {
            Size = size;
            _stream = stream;
        }

        public Sector(int size, byte[] data)
        {
            Size = size;
            _data = data;
            _stream = null;
        }

        public Sector(int size)
        {
            Size = size;
            _data = null;
            _stream = null;
        }

        internal SectorType Type { get; set; }

        public int Id { get; set; } = -1;

        public int Size { get; private set; } = 0;

        private byte[] _data;

        public byte[] GetData()
        {
            if (_data == null)
            {
                _data = new byte[Size];

                if (IsStreamed)
                {
                    _stream.Seek(Size + Id * Size, SeekOrigin.Begin);
                    _stream.Read(_data, 0, Size);
                }
            }

            return _data;
        }

        public void ZeroData()
        {
            _data = new byte[Size];
            DirtyFlag = true;
        }

        public void InitFATData()
        {
            _data = new byte[Size];

            for (int i = 0; i < Size; i++)
                _data[i] = 0xFF;

            DirtyFlag = true;
        }

        internal void ReleaseData() => _data = null;

        private readonly object _lockObject = new();

        #region IDisposable Members

        private bool _disposed;

        void IDisposable.Dispose()
        {
            try
            {
                if (!_disposed)
                {
                    lock (_lockObject)
                    {
                        _data = null;
                        DirtyFlag = false;
                        Id = Endofchain;
                        Size = 0;
                    }
                }
            }
            finally { _disposed = true; }
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }

    internal enum StgType : int
    {
        StgInvalid = 0,
        StgStorage = 1,
        StgStream = 2,
        StgLockbytes = 3,
        StgProperty = 4,
        StgRoot = 5
    }

    internal sealed class DirectoryEntry : IRBNode
    {
        internal const int ThisIsGreater = 1;
        internal const int OtherIsGreater = -1;
        private readonly IList<DirectoryEntry> _dirRepository;

        public int Sid { get; set; } = -1;

        internal const int Nostream = unchecked((int)0xFFFFFFFF);

        private DirectoryEntry(string name, StgType stgType, IList<DirectoryEntry> dirRepository)
        {
            _dirRepository = dirRepository;
            StgType = stgType;
            switch (stgType)
            {
                case StgType.StgStream:

                    StorageClsid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    CreationDate = new byte[8];
                    ModifyDate = new byte[8];
                    break;

                case StgType.StgStorage:
                    CreationDate = BitConverter.GetBytes((DateTime.Now.ToFileTime()));
                    break;

                case StgType.StgRoot:
                    CreationDate = new byte[8];
                    ModifyDate = new byte[8];
                    break;
            }

            SetEntryName(name);
        }

        public byte[] EntryName { get; private set; } = new byte[64];

        public string GetEntryName()
        {
            return EntryName is { Length: > 0 } ? Encoding.Unicode.GetString(EntryName).Remove((NameLength - 1) / 2) : string.Empty;
        }

        public void SetEntryName(string entryName)
        {
            if (entryName.Contains(@"\") || entryName.Contains(@"/") || entryName.Contains(@":") || entryName.Contains(@"!"))
                throw new Exception("Invalid character in entry: the characters '\\', '/', ':','!' cannot be used in entry name");

            if (entryName.Length > 31)
                throw new Exception("Entry name MUST be smaller than 31 characters");

            byte[] temp = Encoding.Unicode.GetBytes(entryName);
            var newName = new byte[64];
            Buffer.BlockCopy(temp, 0, newName, 0, temp.Length);
            newName[temp.Length] = 0x00;
            newName[temp.Length + 1] = 0x00;
            EntryName = newName;
            NameLength = (ushort)(temp.Length + 2);
        }

        public ushort NameLength { get; private set; }

        public StgType StgType { get; set; } = StgType.StgInvalid;

        public Color Color { get; set; } = Color.BLACK;

        public int LeftSibling { get; set; } = Nostream;

        public int RightSibling { get; set; } = Nostream;

        public int Child { get; set; } = Nostream;

        public Guid StorageClsid { get; set; } = Guid.NewGuid();

        public int StateBits { get; set; }

        public byte[] CreationDate { get; set; } = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public byte[] ModifyDate { get; set; } = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public int StartSetc { get; set; } = Sector.Endofchain;

        public long Size { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is not DirectoryEntry otherDir)
                throw new Exception("Invalid casting: compared object does not implement IDirectorEntry interface");

            if (NameLength > otherDir.NameLength)
                return ThisIsGreater;
            if (NameLength < otherDir.NameLength)
                return OtherIsGreater;
            string thisName = Encoding.Unicode.GetString(EntryName, 0, NameLength);
            string otherName = Encoding.Unicode.GetString(otherDir.EntryName, 0, otherDir.NameLength);
            for (int z = 0; z < thisName.Length; z++)
            {
                char thisChar = char.ToUpperInvariant(thisName[z]);
                char otherChar = char.ToUpperInvariant(otherName[z]);

                if (thisChar > otherChar)
                    return ThisIsGreater;
                if (thisChar < otherChar)
                    return OtherIsGreater;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        private static ulong Fnv_hash(byte[] buffer)
        {
            ulong h = 2166136261;
            int i;
            for (i = 0; i < buffer.Length; i++)
                h = (h * 16777619) ^ buffer[i];

            return h;
        }

        public override int GetHashCode()
        {
            return (int)Fnv_hash(EntryName);
        }

        public void Read(Stream stream, int ver = 3)
        {
            using (BinaryReader rw = new BinaryReader(stream, Encoding.UTF8, true))
            {
                EntryName = rw.ReadBytes(64);
                NameLength = rw.ReadUInt16();
                StgType = (StgType)rw.ReadByte();
                Color = (Color)rw.ReadByte();
                LeftSibling = rw.ReadInt32();
                RightSibling = rw.ReadInt32();
                Child = rw.ReadInt32();
                if (StgType == StgType.StgInvalid)
                {
                    LeftSibling = Nostream;
                    RightSibling = Nostream;
                    Child = Nostream;
                }

                StorageClsid = new Guid(rw.ReadBytes(16));
                StateBits = rw.ReadInt32();
                CreationDate = rw.ReadBytes(8);
                ModifyDate = rw.ReadBytes(8);
                StartSetc = rw.ReadInt32();
                if (ver == 3)
                {
                    Size = rw.ReadInt32();
                    rw.ReadBytes(4);
                }
                else
                    Size = rw.ReadInt64();
            }
        }

        public string Name => GetEntryName();

        public IRBNode Left
        {
            get
            {
                if (LeftSibling == Nostream)
                    return null;
                return _dirRepository[LeftSibling];
            }

            set
            {
                LeftSibling = (value as DirectoryEntry)?.Sid ?? Nostream;
                if (LeftSibling != Nostream)
                    _dirRepository[LeftSibling].Parent = this;
            }
        }

        public IRBNode Right
        {
            get => RightSibling == Nostream ? null : _dirRepository[RightSibling];

            set
            {
                RightSibling = ((DirectoryEntry)value)?.Sid ?? Nostream;
                if (RightSibling != Nostream)
                    _dirRepository[RightSibling].Parent = this;
            }
        }

        public IRBNode Parent { get; set; }

        public IRBNode Grandparent() => Parent?.Parent;

        public IRBNode Sibling() => (this == Parent.Left) ? Parent.Right : Parent.Left;

        public IRBNode Uncle() => Parent?.Sibling();

        internal static DirectoryEntry New(String name, StgType stgType, IList<DirectoryEntry> dirRepository)
        {
            DirectoryEntry de;
            if (dirRepository != null)
            {
                de = new DirectoryEntry(name, stgType, dirRepository);
                dirRepository.Add(de);
                de.Sid = dirRepository.Count - 1;
            }
            else
                throw new ArgumentNullException(nameof(dirRepository), "Directory repository cannot be null in New() method");

            return de;
        }

        internal static DirectoryEntry Mock(string name, StgType stgType) => new(name, stgType, null);

        internal static DirectoryEntry TryNew(string name, StgType stgType, IList<DirectoryEntry> dirRepository)
        {
            var de = new DirectoryEntry(name, stgType, dirRepository);
            if (de != null)
            {
                for (int i = 0; i < dirRepository.Count; i++)
                {
                    if (dirRepository[i].StgType == StgType.StgInvalid)
                    {
                        dirRepository[i] = de;
                        de.Sid = i;
                        return de;
                    }
                }
            }

            dirRepository.Add(de);
            de.Sid = dirRepository.Count - 1;
            return de;
        }

        public override string ToString() => $"{Name} [{Sid}]{(StgType == StgType.StgStream ? "Stream" : "Storage")}";

        public void AssignValueTo(IRBNode other)
        {
            var d = other as DirectoryEntry;
            d.SetEntryName(GetEntryName());
            d.CreationDate = new byte[CreationDate.Length];
            CreationDate.CopyTo(d.CreationDate, 0);
            d.ModifyDate = new byte[ModifyDate.Length];
            ModifyDate.CopyTo(d.ModifyDate, 0);
            d.Size = Size;
            d.StartSetc = StartSetc;
            d.StateBits = StateBits;
            d.StgType = StgType;
            d.StorageClsid = new Guid(StorageClsid.ToByteArray());
            d.Child = Child;
        }
    }

    internal abstract class CFItem : IComparable<CFItem>
    {
        private CompoundFile compoundFile;

        protected CompoundFile CompoundFile => compoundFile;

        protected void CheckDisposed()
        {
            if (compoundFile.IsClosed)
                throw new ObjectDisposedException("Owner Compound file has been closed and owned items have been invalidated");
        }

        protected CFItem()
        { }

        protected CFItem(CompoundFile compoundFile)
        { compoundFile = compoundFile; }

        internal DirectoryEntry DirEntry { get; set; }

        internal int CompareTo(CFItem other) => DirEntry.CompareTo(other.DirEntry);

        public int CompareTo(object obj) => DirEntry.CompareTo((obj as CFItem).DirEntry);

        public static bool operator ==(CFItem leftItem, CFItem rightItem)
        {
            if (ReferenceEquals(leftItem, rightItem))
                return true;
            if (((object)leftItem == null) || ((object)rightItem == null))
                return false;
            return leftItem.CompareTo(rightItem) == 0;
        }

        public static bool operator !=(CFItem leftItem, CFItem rightItem) => !(leftItem == rightItem);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => DirEntry.GetEntryName().GetHashCode();

        public string Name
        {
            get
            {
                var n = DirEntry.GetEntryName();
                return (n != null && n.Length > 0) ? n.TrimEnd('\0') : string.Empty;
            }
        }

        public long Size => DirEntry.Size;

        public bool IsStorage => DirEntry.StgType == StgType.StgStorage;

        public bool IsStream => DirEntry.StgType == StgType.StgStream;

        public bool IsRoot => DirEntry.StgType == StgType.StgRoot;

        public DateTime CreationDate
        {
            get => DateTime.FromFileTime(BitConverter.ToInt64(DirEntry.CreationDate, 0));

            set
            {
                if (DirEntry.StgType != StgType.StgStream && DirEntry.StgType != StgType.StgRoot)
                    DirEntry.CreationDate = BitConverter.GetBytes((value.ToFileTime()));
                else
                    throw new Exception("Creation Date can only be set on storage entries");
            }
        }

        public DateTime ModifyDate
        {
            get => DateTime.FromFileTime(BitConverter.ToInt64(DirEntry.ModifyDate, 0));

            set
            {
                if (DirEntry.StgType != StgType.StgStream && DirEntry.StgType != StgType.StgRoot)
                    DirEntry.ModifyDate = BitConverter.GetBytes((value.ToFileTime()));
                else
                    throw new Exception("Modify Date can only be set on storage entries");
            }
        }

        public Guid CLSID
        {
            get => DirEntry.StorageClsid;

            set
            {
                if (DirEntry.StgType != StgType.StgStream)
                    DirEntry.StorageClsid = value;
                else
                    throw new Exception("Object class GUID can only be set on Root and Storage entries");
            }
        }

        int IComparable<CFItem>.CompareTo(CFItem other) => DirEntry.CompareTo(other.DirEntry);

        public override string ToString()
        {
            return DirEntry != null ? $"[{DirEntry.LeftSibling},{DirEntry.Sid},{DirEntry.RightSibling}] {DirEntry.GetEntryName()}" : string.Empty;
        }
    }

    internal sealed class CFStream : CFItem
    {
        internal CFStream(CompoundFile compoundFile, DirectoryEntry dirEntry) : base(compoundFile)
        {
            if (dirEntry == null || dirEntry.Sid < 0)
                throw new Exception("Attempting to add a CFStream using an unitialized directory");
            DirEntry = dirEntry;
        }

        public byte[] GetData()
        {
            CheckDisposed();
            return CompoundFile.GetData(this);
        }

        public int Read(byte[] buffer, long position, int count)
        {
            CheckDisposed();
            return CompoundFile.ReadData(this, position, buffer, 0, count);
        }

        internal int Read(byte[] buffer, long position, int offset, int count)
        {
            CheckDisposed();
            return CompoundFile.ReadData(this, position, buffer, offset, count);
        }
    }

    internal sealed class CFStorage : CFItem
    {
        private RBTree children;

        internal RBTree Children => children ??= LoadChildren(DirEntry.Sid) ?? CompoundFile.CreateNewTree();

        internal CFStorage(CompoundFile compFile, DirectoryEntry dirEntry) : base(compFile)
        {
            if (dirEntry == null || dirEntry.Sid < 0)
                throw new Exception("Attempting to create a CFStorage using an unitialized directory");
            DirEntry = dirEntry;
        }

        private RBTree LoadChildren(int SID)
        {
            var childrenTree = CompoundFile.GetChildrenTree(SID);
            DirEntry.Child = (childrenTree.Root as DirectoryEntry)?.Sid ?? DirectoryEntry.Nostream;
            return childrenTree;
        }

        public CFStream GetStream(String streamName)
        {
            CheckDisposed();
            var tmp = DirectoryEntry.Mock(streamName, StgType.StgStream);
            if (Children.TryLookup(tmp, out IRBNode outDe) && (((DirectoryEntry)outDe).StgType == StgType.StgStream))
                return new CFStream(CompoundFile, (DirectoryEntry)outDe);
            throw new KeyNotFoundException("Cannot find item [" + streamName + "] within the current storage");
        }

        public CFStream TryGetStream(String streamName)
        {
            CheckDisposed();
            var tmp = DirectoryEntry.Mock(streamName, StgType.StgStream);
            if (Children.TryLookup(tmp, out IRBNode outDe) && ((outDe as DirectoryEntry).StgType == StgType.StgStream))
                return new CFStream(CompoundFile, (DirectoryEntry)outDe);
            return null;
        }

        public CFStorage GetStorage(String storageName)
        {
            CheckDisposed();
            var template = DirectoryEntry.Mock(storageName, StgType.StgInvalid);
            if (Children.TryLookup(template, out var outDe) && (outDe as DirectoryEntry).StgType == StgType.StgStorage)
                return new CFStorage(CompoundFile, outDe as DirectoryEntry);
            else
                throw new KeyNotFoundException("Cannot find item [" + storageName + "] within the current storage");
        }

        public CFStorage TryGetStorage(string storageName)
        {
            CheckDisposed();
            var template = DirectoryEntry.Mock(storageName, StgType.StgInvalid);
            if (Children.TryLookup(template, out var outDe) && ((DirectoryEntry)outDe).StgType == StgType.StgStorage)
                return new CFStorage(CompoundFile, outDe as DirectoryEntry);
            return null;
        }

        public void VisitEntries(Action<CFItem> action, bool recursive)
        {
            CheckDisposed();
            if (action != null)
            {
                var subStorages = new List<IRBNode>();
                void internalAction(IRBNode targetNode)
                {
                    DirectoryEntry d = targetNode as DirectoryEntry;
                    if (d.StgType == StgType.StgStream)
                        action(new CFStream(CompoundFile, d));
                    else
                        action(new CFStorage(CompoundFile, d));

                    if (d.Child != DirectoryEntry.Nostream)
                        subStorages.Add(targetNode);
                }

                Children.VisitTreeNodes(internalAction);

                if (recursive && subStorages.Count > 0)
                    foreach (var n in subStorages)
                        new CFStorage(CompoundFile, n as DirectoryEntry).VisitEntries(action, recursive);
            }
        }
    }

    internal class CFItemComparer : IComparer<CFItem>
    {
        public int Compare(CFItem x, CFItem y) => (x.DirEntry.CompareTo(y.DirEntry));
    }

    internal sealed class Header
    {
        public byte[] HeaderSignature { get; private set; } = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        public byte[] CLSID { get; set; } = new byte[16];

        public ushort MinorVersion { get; private set; } = 0x003E;

        public ushort MajorVersion { get; private set; } = 0x0003;

        public ushort ByteOrder { get; private set; } = 0xFFFE;

        public ushort SectorShift { get; private set; } = 9;

        public ushort MiniSectorShift { get; private set; } = 6;

        public int DirectorySectorsNumber { get; set; }

        public int FATSectorsNumber { get; set; }

        public int FirstDirectorySectorID { get; set; } = Sector.Endofchain;

        public uint MinSizeStandardStream { get; set; } = 4096;

        public int FirstMiniFATSectorID { get; set; } = unchecked((int)0xFFFFFFFE);

        public uint MiniFATSectorsNumber { get; set; }

        public int FirstDIFATSectorID { get; set; } = Sector.Endofchain;

        public uint DIFATSectorsNumber { get; set; }

        public int[] DIFAT { get; } = new int[109];

        public Header() : this(3)
        {
        }

        public Header(ushort version)
        {
            switch (version)
            {
                case 3:
                    MajorVersion = 3;
                    SectorShift = 0x0009;
                    break;

                case 4:
                    MajorVersion = 4;
                    SectorShift = 0x000C;
                    break;

                default:
                    throw new Exception("Invalid Compound File Format version");
            }

            for (int i = 0; i < 109; i++)
                DIFAT[i] = Sector.Freesect;
        }

        public void Read(Stream stream)
        {
            var rw = new BinaryReader(stream, Encoding.UTF8, true);
            HeaderSignature = rw.ReadBytes(8);
            CheckSignature();
            CLSID = rw.ReadBytes(16);
            MinorVersion = rw.ReadUInt16();
            MajorVersion = rw.ReadUInt16();
            CheckVersion();
            ByteOrder = rw.ReadUInt16();
            SectorShift = rw.ReadUInt16();
            MiniSectorShift = rw.ReadUInt16();
            rw.ReadBytes(6);
            DirectorySectorsNumber = rw.ReadInt32();
            FATSectorsNumber = rw.ReadInt32();
            FirstDirectorySectorID = rw.ReadInt32();
            rw.ReadUInt32();
            MinSizeStandardStream = rw.ReadUInt32();
            FirstMiniFATSectorID = rw.ReadInt32();
            MiniFATSectorsNumber = rw.ReadUInt32();
            FirstDIFATSectorID = rw.ReadInt32();
            DIFATSectorsNumber = rw.ReadUInt32();
            for (int i = 0; i < 109; i++)
                DIFAT[i] = rw.ReadInt32();
        }

        private void CheckVersion()
        {
            if (MajorVersion != 3 && MajorVersion != 4)
                throw new InvalidDataException("Unsupported Binary File Format version: OpenMcdf only supports Compound Files with major version equal to 3 or 4 ");
        }

        private byte[] OLE_CFS_SIGNATURE = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        private void CheckSignature()
        {
            for (int i = 0; i < HeaderSignature.Length; i++)
                if (HeaderSignature[i] != OLE_CFS_SIGNATURE[i])
                    throw new InvalidDataException("Invalid OLE structured storage file");
        }
    }

    internal sealed class StreamView : Stream
    {
        private readonly int _sectorSize;
        private long _position;
        private readonly List<Sector> _sectorChain;
        private readonly Stream _stream;
        private readonly bool _isFatStream = false;
        private readonly List<Sector> _freeSectors = [];

        public IEnumerable<Sector> FreeSectors => _freeSectors;

        public StreamView(List<Sector> sectorChain, int sectorSize, Stream stream)
        {
            if (sectorSize <= 0)
                throw new Exception("Sector size must be greater than zero");

            _sectorChain = sectorChain ?? throw new Exception("Sector Chain cannot be null");
            _sectorSize = sectorSize;
            _stream = stream;
        }

        public StreamView(List<Sector> sectorChain, int sectorSize, long length, Queue<Sector> availableSectors, Stream stream, bool isFatStream = false) : this(sectorChain, sectorSize, stream)
        {
            _isFatStream = isFatStream;
            AdjustLength(length, availableSectors);
        }

        public List<Sector> BaseSectorChain => _sectorChain;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        { }

        private long _length;

        public override long Length => _length;

        public override long Position
        {
            get => _position;

            set
            {
                if (_position > _length - 1)
                    throw new ArgumentOutOfRangeException("value");
                _position = value;
            }
        }

        private byte[] buf = new byte[4];

        public int ReadInt32()
        {
            Read(buf, 0, 4);
            return (((buf[0] | (buf[1] << 8)) | (buf[2] << 16)) | (buf[3] << 24));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int nRead = 0;
            if (_sectorChain is { Count: > 0 })
            {
                // First sector
                int secIndex = (int)(_position / _sectorSize);
                var nToRead = Math.Min(_sectorChain[0].Size - ((int)_position % _sectorSize), count);
                if (secIndex < _sectorChain.Count)
                {
                    Buffer.BlockCopy(_sectorChain[secIndex].GetData(), (int)(_position % _sectorSize), buffer, offset, nToRead);
                }

                nRead += nToRead;
                ++secIndex;

                // Central sectors
                while (nRead < (count - _sectorSize))
                {
                    nToRead = _sectorSize;
                    Buffer.BlockCopy(_sectorChain[secIndex].GetData(), 0, buffer, offset + nRead, nToRead);
                    nRead += nToRead;
                    ++secIndex;
                }

                // Last sector
                nToRead = count - nRead;
                if (nToRead != 0)
                {
                    Buffer.BlockCopy(_sectorChain[secIndex].GetData(), 0, buffer, offset + nRead, nToRead);
                    nRead += nToRead;
                }

                _position += nRead;
                return nRead;
            }

            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: _position = offset; break;
                case SeekOrigin.Current: _position += offset; break;
                case SeekOrigin.End: _position = Length - offset; break;
            }
            AdjustLength(_position);
            return _position;
        }

        private void AdjustLength(long value, Queue<Sector> availableSectors = null)
        {
            _length = value;
            long delta = value - _sectorChain.Count * (long)_sectorSize;

            if (delta > 0)
            {
                int nSec = (int)Math.Ceiling(((double)delta / _sectorSize));
                while (nSec > 0)
                {
                    Sector t;
                    if (availableSectors == null || availableSectors.Count == 0)
                    {
                        t = new Sector(_sectorSize, _stream);
                        if (_sectorSize == Sector.MinisectorSize)
                            t.Type = SectorType.Mini;
                    }
                    else
                        t = availableSectors.Dequeue();

                    if (_isFatStream)
                        t.InitFATData();
                    _sectorChain.Add(t);
                    nSec--;
                }
            }
        }

        public override void SetLength(long value) => AdjustLength(value);

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }

    [Flags]
    internal enum CFSConfiguration
    {
        Default = 1,
        LeaveOpen = 16,
    }

    internal sealed class CompoundFile : IDisposable
    {
        public CFSConfiguration Configuration { get; private set; } = CFSConfiguration.Default;

        internal int GetSectorSize() => 2 << (_header.SectorShift - 1);

        private const int HeaderDifatEntriesCount = 109;
        private readonly int _difatSectorFatEntriesCount = 127;
        private readonly int _fatSectorEntriesCount = 128;
        private const int SizeOfSid = 4;
        private const int FlushingQueueSize = 6000;
        private const int FlushingBufferMaxSize = 1024 * 1024 * 16;
        private List<Sector> _sectors = [];
        private Header _header;
        internal Stream SourceStream;

        public CompoundFile(Stream stream, CFSConfiguration configParameters)
        {
            closeStream = !configParameters.HasFlag(CFSConfiguration.LeaveOpen);
            LoadStream(stream);
            _difatSectorFatEntriesCount = (GetSectorSize() / 4) - 1;
            _fatSectorEntriesCount = (GetSectorSize() / 4);
        }

        private void Load(Stream stream)
        {
            try
            {
                _header = new Header();
                directoryEntries = new List<DirectoryEntry>();
                SourceStream = stream;
                _header.Read(stream);
                int nSector = Ceiling((stream.Length - GetSectorSize()) / (double)GetSectorSize());
                if (stream.Length > 0x7FFFFF0)
                    _transactionLockAllocated = true;

                _sectors = [];
                for (int i = 0; i < nSector; i++)
                    _sectors.Add(null);

                LoadDirectories();
                RootStorage = new CFStorage(this, directoryEntries[0]);
            }
            catch (Exception)
            {
                if (stream != null && closeStream)
                    stream.Dispose();
                throw;
            }
        }

        private void LoadStream(Stream stream)
        {
            if (stream == null)
                throw new Exception("Stream parameter cannot be null");
            if (!stream.CanSeek)
                throw new Exception("Cannot load a non-seekable Stream");
            stream.Seek(0, SeekOrigin.Begin);
            Load(stream);
        }

        private void AllocateMiniSectorChain(List<Sector> sectorChain)
        {
            List<Sector> miniFAT = GetSectorChain(_header.FirstMiniFATSectorID, SectorType.Normal);
            List<Sector> miniStream = GetSectorChain(RootEntry.StartSetc, SectorType.Normal);

            StreamView miniFATView = new StreamView(miniFAT, GetSectorSize(),
                _header.MiniFATSectorsNumber * Sector.MinisectorSize,
                null, SourceStream, true);

            StreamView miniStreamView = new StreamView(miniStream, GetSectorSize(),
                RootStorage.Size, null, SourceStream);

            for (int i = 0; i < sectorChain.Count; i++)
            {
                Sector s = sectorChain[i];
                if (s.Id == -1)
                {
                    miniStreamView.Seek(RootStorage.Size + Sector.MinisectorSize, SeekOrigin.Begin);
                    s.Id = (int)(miniStreamView.Position - Sector.MinisectorSize) / Sector.MinisectorSize;

                    RootStorage.DirEntry.Size = miniStreamView.Length;
                }
            }

            for (int i = 0; i < sectorChain.Count - 1; i++)
            {
                int currentId = sectorChain[i].Id;
                int nextId = sectorChain[i + 1].Id;
                miniFATView.Seek(currentId * 4, SeekOrigin.Begin);
                miniFATView.Write(BitConverter.GetBytes(nextId), 0, 4);
            }

            miniFATView.Seek(sectorChain[sectorChain.Count - 1].Id * SizeOfSid, SeekOrigin.Begin);
            miniFATView.Write(BitConverter.GetBytes(Sector.Endofchain), 0, 4);
            AllocateSectorChain(miniStreamView.BaseSectorChain);
            AllocateSectorChain(miniFATView.BaseSectorChain);
            if (miniFAT.Count > 0)
            {
                RootStorage.DirEntry.StartSetc = miniStream[0].Id;
                _header.MiniFATSectorsNumber = (uint)miniFAT.Count;
                _header.FirstMiniFATSectorID = miniFAT[0].Id;
            }
        }

        private void SetSectorChain(List<Sector> sectorChain)
        {
            if (sectorChain == null || sectorChain.Count == 0)
                return;

            SectorType _st = sectorChain[0].Type;
            if (_st == SectorType.Normal)
                AllocateSectorChain(sectorChain);
            else if (_st == SectorType.Mini)
                AllocateMiniSectorChain(sectorChain);
        }

        private void AllocateSectorChain(List<Sector> sectorChain)
        {
            foreach (Sector s in sectorChain)
            {
                if (s.Id == -1)
                {
                    _sectors.Add(s);
                    s.Id = _sectors.Count - 1;
                }
            }

            AllocateFATSectorChain(sectorChain);
        }

        internal bool _transactionLockAdded = false;
        internal int _lockSectorId = -1;
        internal bool _transactionLockAllocated = false;

        private void CheckForLockSector()
        {
            if (_transactionLockAdded && !_transactionLockAllocated)
            {
                StreamView fatStream = new StreamView(GetFatSectorChain(), GetSectorSize(), SourceStream);
                fatStream.Seek(_lockSectorId * 4, SeekOrigin.Begin);
                fatStream.Write(BitConverter.GetBytes(Sector.Endofchain), 0, 4);
                _transactionLockAllocated = true;
            }
        }

        private void AllocateFATSectorChain(List<Sector> sectorChain)
        {
            List<Sector> fatSectors = GetSectorChain(-1, SectorType.FAT);
            StreamView fatStream = new StreamView(fatSectors, GetSectorSize(),
                _header.FATSectorsNumber * GetSectorSize(), null,
                SourceStream, true);

            for (int i = 0; i < sectorChain.Count - 1; i++)
            {
                Sector sN = sectorChain[i + 1];
                Sector sC = sectorChain[i];
                fatStream.Seek(sC.Id * 4, SeekOrigin.Begin);
                fatStream.Write(BitConverter.GetBytes(sN.Id), 0, 4);
            }

            fatStream.Seek(sectorChain[sectorChain.Count - 1].Id * 4, SeekOrigin.Begin);
            fatStream.Write(BitConverter.GetBytes(Sector.Endofchain), 0, 4);
            AllocateDIFATSectorChain(fatStream.BaseSectorChain);
        }

        private void AllocateDIFATSectorChain(List<Sector> FATsectorChain)
        {
            _header.FATSectorsNumber = FATsectorChain.Count;
            foreach (Sector s in FATsectorChain)
            {
                if (s.Id == -1)
                {
                    _sectors.Add(s);
                    s.Id = _sectors.Count - 1;
                    s.Type = SectorType.FAT;
                }
            }

            int nCurrentSectors = _sectors.Count;
            int nDIFATSectors = (int)_header.DIFATSectorsNumber;
            if (FATsectorChain.Count > HeaderDifatEntriesCount)
            {
                nDIFATSectors = Ceiling((double)(FATsectorChain.Count - HeaderDifatEntriesCount) / _difatSectorFatEntriesCount);
                nDIFATSectors = LowSaturation(nDIFATSectors - (int)_header.DIFATSectorsNumber); //required DIFAT
            }

            nCurrentSectors += nDIFATSectors;
            while (_header.FATSectorsNumber * _fatSectorEntriesCount < nCurrentSectors)
            {
                Sector extraFATSector = new Sector(GetSectorSize(), SourceStream);
                _sectors.Add(extraFATSector);
                extraFATSector.Id = _sectors.Count - 1;
                extraFATSector.Type = SectorType.FAT;
                FATsectorChain.Add(extraFATSector);
                _header.FATSectorsNumber++;
                nCurrentSectors++;
                if (nDIFATSectors * _difatSectorFatEntriesCount < (_header.FATSectorsNumber > HeaderDifatEntriesCount ? _header.FATSectorsNumber - HeaderDifatEntriesCount : 0))
                {
                    nDIFATSectors++;
                    nCurrentSectors++;
                }
            }

            var difatSectors = GetSectorChain(-1, SectorType.DIFAT);
            var difatStream = new StreamView(difatSectors, GetSectorSize(), SourceStream);
            for (int i = 0; i < FATsectorChain.Count; i++)
            {
                if (i < HeaderDifatEntriesCount)
                    _header.DIFAT[i] = FATsectorChain[i].Id;
                else
                {
                    if (i != HeaderDifatEntriesCount && (i - HeaderDifatEntriesCount) % _difatSectorFatEntriesCount == 0)
                    {
                        difatStream.Write(new byte[sizeof(int)], 0, sizeof(int));
                    }

                    difatStream.Write(BitConverter.GetBytes(FATsectorChain[i].Id), 0, sizeof(int));
                }
            }

            for (int i = 0; i < difatStream.BaseSectorChain.Count; i++)
            {
                if (difatStream.BaseSectorChain[i].Id == -1)
                {
                    _sectors.Add(difatStream.BaseSectorChain[i]);
                    difatStream.BaseSectorChain[i].Id = _sectors.Count - 1;
                    difatStream.BaseSectorChain[i].Type = SectorType.DIFAT;
                }
            }

            _header.DIFATSectorsNumber = (uint)nDIFATSectors;
            if (difatStream.BaseSectorChain != null && difatStream.BaseSectorChain.Count > 0)
            {
                _header.FirstDIFATSectorID = difatStream.BaseSectorChain[0].Id;
                _header.DIFATSectorsNumber = (uint)difatStream.BaseSectorChain.Count;
                for (int i = 0; i < difatStream.BaseSectorChain.Count - 1; i++)
                    Buffer.BlockCopy(BitConverter.GetBytes(difatStream.BaseSectorChain[i + 1].Id), 0, difatStream.BaseSectorChain[i].GetData(), GetSectorSize() - sizeof(int), 4);

                Buffer.BlockCopy(BitConverter.GetBytes(Sector.Endofchain), 0, difatStream.BaseSectorChain[difatStream.BaseSectorChain.Count - 1].GetData(), GetSectorSize() - sizeof(int), sizeof(int));
            }
            else _header.FirstDIFATSectorID = Sector.Endofchain;
            StreamView fatSv = new StreamView(FATsectorChain, GetSectorSize(), _header.FATSectorsNumber * GetSectorSize(), null, SourceStream);
            for (int i = 0; i < _header.DIFATSectorsNumber; i++)
            {
                fatSv.Seek(difatStream.BaseSectorChain[i].Id * 4, SeekOrigin.Begin);
                fatSv.Write(BitConverter.GetBytes(Sector.Difsect), 0, 4);
            }

            for (int i = 0; i < _header.FATSectorsNumber; i++)
            {
                fatSv.Seek(fatSv.BaseSectorChain[i].Id * 4, SeekOrigin.Begin);
                fatSv.Write(BitConverter.GetBytes(Sector.Fatsect), 0, 4);
            }

            _header.FATSectorsNumber = fatSv.BaseSectorChain.Count;
        }

        private List<Sector> GetDifatSectorChain()
        {
            List<Sector> result = new List<Sector>();
            int nextSecID;
            if (_header.DIFATSectorsNumber != 0)
            {
                var validationCount = (int)_header.DIFATSectorsNumber;
                Sector s = _sectors[_header.FirstDIFATSectorID];
                if (s == null)
                {
                    _sectors[_header.FirstDIFATSectorID] = s = new Sector(GetSectorSize(), SourceStream)
                    {
                        Type = SectorType.DIFAT,
                        Id = _header.FirstDIFATSectorID
                    };
                }

                result.Add(s);
                while (validationCount >= 0)
                {
                    nextSecID = BitConverter.ToInt32(s.GetData(), GetSectorSize() - 4);
                    if (nextSecID == Sector.Freesect || nextSecID == Sector.Endofchain) break;
                    validationCount--;
                    if (validationCount < 0)
                    {
                        Dispose();
                        throw new InvalidDataException("DIFAT sectors count mismatched. Corrupted compound file");
                    }

                    s = _sectors[nextSecID];
                    if (s == null)
                        _sectors[nextSecID] = s = new Sector(GetSectorSize(), SourceStream) { Id = nextSecID };

                    result.Add(s);
                }
            }

            return result;
        }

        private List<Sector> GetFatSectorChain()
        {
            const int nHeaderFatEntry = 109;
            List<Sector> result = [];
            int nextSecId;
            List<Sector> difatSectors = GetDifatSectorChain();
            int idx = 0;
            while (idx < _header.FATSectorsNumber && idx < nHeaderFatEntry)
            {
                nextSecId = _header.DIFAT[idx];
                Sector s = _sectors[nextSecId];
                if (s == null)
                {
                    _sectors[nextSecId] = s = new Sector(GetSectorSize(), SourceStream)
                    {
                        Id = nextSecId,
                        Type = SectorType.FAT
                    };
                }

                result.Add(s);
                ++idx;
            }

            if (difatSectors.Count > 0)
            {
                var difatStream = new StreamView(difatSectors, GetSectorSize(), _header.FATSectorsNumber > nHeaderFatEntry ? (_header.FATSectorsNumber - nHeaderFatEntry) * 4 : 0, null, SourceStream);
                byte[] nextDifatSectorBuffer = new byte[4];
                difatStream.Read(nextDifatSectorBuffer, 0, 4);
                nextSecId = BitConverter.ToInt32(nextDifatSectorBuffer, 0);
                int i = 0;
                int nFat = nHeaderFatEntry;
                while (nFat < _header.FATSectorsNumber)
                {
                    if (difatStream.Position == ((GetSectorSize() - 4) + i * GetSectorSize()))
                    {
                        difatStream.Seek(4, SeekOrigin.Current);
                        ++i;
                        continue;
                    }

                    Sector s = _sectors[nextSecId];
                    if (s == null)
                    {
                        _sectors[nextSecId] = s = new Sector(GetSectorSize(), SourceStream)
                        {
                            Type = SectorType.FAT,
                            Id = nextSecId
                        };
                    }

                    result.Add(s);
                    difatStream.Read(nextDifatSectorBuffer, 0, 4);
                    nextSecId = BitConverter.ToInt32(nextDifatSectorBuffer, 0);
                    nFat++;
                }
            }

            return result;
        }

        private List<Sector> GetNormalSectorChain(int secID)
        {
            var result = new List<Sector>();
            int nextSecId = secID;
            var fatSectors = GetFatSectorChain();
            var fatStream = new StreamView(fatSectors, GetSectorSize(), fatSectors.Count * GetSectorSize(), null, SourceStream);
            while (true)
            {
                if (nextSecId == Sector.Endofchain) break;

                if (nextSecId < 0)
                    throw new InvalidDataException($"Next Sector ID reference is below zero. NextID : {nextSecId}");

                if (nextSecId >= _sectors.Count)
                    throw new InvalidDataException($"Next Sector ID reference an out of range sector. NextID : {nextSecId} while sector count {_sectors.Count}");

                Sector s = _sectors[nextSecId];
                if (s == null)
                {
                    _sectors[nextSecId] = s = new Sector(GetSectorSize(), SourceStream)
                    {
                        Id = nextSecId,
                        Type = SectorType.Normal
                    };
                }

                result.Add(s);
                fatStream.Seek(nextSecId * 4, SeekOrigin.Begin);
                int next = fatStream.ReadInt32();
                if (next != nextSecId)
                    nextSecId = next;
                else
                    throw new InvalidDataException("Cyclic sector chain found. File is corrupted");
            }

            return result;
        }

        private List<Sector> GetMiniSectorChain(int secID)
        {
            List<Sector> result = [];
            if (secID != Sector.Endofchain)
            {
                var miniFat = GetNormalSectorChain(_header.FirstMiniFATSectorID);
                var miniStream = GetNormalSectorChain(RootEntry.StartSetc);
                var miniFatView = new StreamView(miniFat, GetSectorSize(), _header.MiniFATSectorsNumber * Sector.MinisectorSize, null, SourceStream);
                var miniStreamView = new StreamView(miniStream, GetSectorSize(), RootStorage.Size, null, SourceStream);
                var miniFatReader = new BinaryReader(miniFatView);
                var nextSecId = secID;
                while (true)
                {
                    if (nextSecId == Sector.Endofchain)
                        break;

                    var ms = new Sector(Sector.MinisectorSize, SourceStream);
                    ms.Id = nextSecId;
                    ms.Type = SectorType.Mini;
                    miniStreamView.Seek(nextSecId * Sector.MinisectorSize, SeekOrigin.Begin);
                    miniStreamView.Read(ms.GetData(), 0, Sector.MinisectorSize);
                    result.Add(ms);
                    miniFatView.Seek(nextSecId * 4, SeekOrigin.Begin);
                    nextSecId = miniFatReader.ReadInt32();
                }
            }

            return result;
        }

        internal List<Sector> GetSectorChain(int secID, SectorType chainType)
        {
            switch (chainType)
            {
                case SectorType.DIFAT:
                    return GetDifatSectorChain();

                case SectorType.FAT:
                    return GetFatSectorChain();

                case SectorType.Normal:
                    return GetNormalSectorChain(secID);

                case SectorType.Mini:
                    return GetMiniSectorChain(secID);

                default:
                    throw new Exception("Unsupproted chain type");
            }
        }

        public CFStorage RootStorage { get; private set; }

        public int Version => _header.MajorVersion;

        internal RBTree CreateNewTree()
        {
            return new RBTree();
        }

        internal RBTree GetChildrenTree(int sid)
        {
            RBTree bst = new RBTree();
            DoLoadChildren(bst, directoryEntries[sid]);
            return bst;
        }

        private RBTree DoLoadChildrenTrusted(DirectoryEntry de)
        {
            RBTree bst = null;
            if (de.Child != DirectoryEntry.Nostream)
                bst = new RBTree(directoryEntries[de.Child]);
            return bst;
        }

        private void DoLoadChildren(RBTree bst, DirectoryEntry de)
        {
            if (de.Child != DirectoryEntry.Nostream)
            {
                if (directoryEntries[de.Child].StgType == StgType.StgInvalid) return;

                LoadSiblings(bst, directoryEntries[de.Child]);
                NullifyChildNodes(directoryEntries[de.Child]);
                bst.Insert(directoryEntries[de.Child]);
            }
        }

        private void NullifyChildNodes(DirectoryEntry de)
        {
            de.Parent = null;
            de.Left = null;
            de.Right = null;
        }

        private readonly List<int> _levelSiDs = [];

        private void LoadSiblings(RBTree bst, DirectoryEntry de)
        {
            _levelSiDs.Clear();
            if (de.LeftSibling != DirectoryEntry.Nostream)
                DoLoadSiblings(bst, directoryEntries[de.LeftSibling]);

            if (de.RightSibling != DirectoryEntry.Nostream)
            {
                _levelSiDs.Add(de.RightSibling);
                DoLoadSiblings(bst, directoryEntries[de.RightSibling]);
            }
        }

        private void DoLoadSiblings(RBTree bst, DirectoryEntry de)
        {
            if (ValidateSibling(de.LeftSibling))
            {
                _levelSiDs.Add(de.LeftSibling);
                DoLoadSiblings(bst, directoryEntries[de.LeftSibling]);
            }

            if (ValidateSibling(de.RightSibling))
            {
                _levelSiDs.Add(de.RightSibling);
                DoLoadSiblings(bst, directoryEntries[de.RightSibling]);
            }

            NullifyChildNodes(de);
            bst.Insert(de);
        }

        private bool ValidateSibling(int sid)
        {
            if (sid != DirectoryEntry.Nostream)
            {
                if (sid >= directoryEntries.Count)
                    return false;

                if (directoryEntries[sid].StgType == StgType.StgInvalid)
                    return false;

                if (!Enum.IsDefined(typeof(StgType), directoryEntries[sid].StgType))
                    return false;

                if (_levelSiDs.Contains(sid))
                    throw new InvalidDataException("Cyclic reference of directory item");

                return true;
            }

            return false;
        }

        private void LoadDirectories()
        {
            List<Sector> directoryChain = GetSectorChain(_header.FirstDirectorySectorID, SectorType.Normal);

            if (_header.FirstDirectorySectorID == Sector.Endofchain)
                _header.FirstDirectorySectorID = directoryChain[0].Id;

            var dirReader = new StreamView(directoryChain, GetSectorSize(), directoryChain.Count * GetSectorSize(), null, SourceStream);
            while (dirReader.Position < directoryChain.Count * GetSectorSize())
            {
                DirectoryEntry de = DirectoryEntry.New(String.Empty, StgType.StgInvalid, directoryEntries);
                de.Read(dirReader, Version);
            }
        }

        internal int ReadData(CFStream cFStream, long position, byte[] buffer, int offset, int count)
        {
            var de = cFStream.DirEntry;
            count = (int)Math.Min(de.Size - offset, count);
            var sView = de.Size < _header.MinSizeStandardStream ? new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, SourceStream) : new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, SourceStream);
            sView.Seek(position, SeekOrigin.Begin);
            return sView.Read(buffer, offset, count);
        }

        internal byte[] GetData(CFStream cFStream)
        {
            AssertDisposed();
            byte[] result;
            DirectoryEntry de = cFStream.DirEntry;
            if (de.Size < _header.MinSizeStandardStream)
            {
                var miniView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, SourceStream);
                using var br = new BinaryReader(miniView);
                result = br.ReadBytes((int)de.Size);
            }
            else
            {
                var sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, SourceStream);
                result = new byte[(int)de.Size];
                sView.Read(result, 0, result.Length);
            }

            return result;
        }

        public byte[] GetDataBySID(int sid)
        {
            AssertDisposed();
            if (sid < 0)
                return null;
            byte[] result;
            try
            {
                DirectoryEntry de = directoryEntries[sid];
                if (de.Size < _header.MinSizeStandardStream)
                {
                    var miniView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, SourceStream);
                    var br = new BinaryReader(miniView);
                    result = br.ReadBytes((int)de.Size);
                    br.Dispose();
                }
                else
                {
                    var sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, SourceStream);
                    result = new byte[(int)de.Size];
                    sView.Read(result, 0, result.Length);
                }
            }
            catch
            {
                throw new Exception("Cannot get data for SID");
            }
            return result;
        }

        public Guid GetGuidBySID(int sid)
        {
            AssertDisposed();
            if (sid < 0)
                throw new Exception("Invalid SID");
            var de = directoryEntries[sid];
            return de.StorageClsid;
        }

        public Guid GetGuidForStream(int sid)
        {
            AssertDisposed();
            if (sid < 0)
                throw new Exception("Invalid SID");
            var g = new Guid("00000000000000000000000000000000");
            for (int i = sid - 1; i >= 0; i--)
            {
                if (directoryEntries[i].StorageClsid != g && directoryEntries[i].StgType == StgType.StgStorage)
                    return directoryEntries[i].StorageClsid;
            }
            return g;
        }

        private static int Ceiling(double d) => (int)Math.Ceiling(d);

        private static int LowSaturation(int i) => i > 0 ? i : 0;

        private bool closeStream = true;

        internal bool IsClosed => _disposed;

        #region IDisposable Members

        private bool _disposed;//false
        private object lockObject = new();

        public void Dispose()
        {
            try
            {
                if (!_disposed)
                {
                    lock (lockObject)
                    {
                        if (_sectors != null)
                        {
                            _sectors.Clear();
                            _sectors = null;
                        }

                        RootStorage = null;
                        _header = null;
                        directoryEntries.Clear();
                        directoryEntries = null;
                    }

                    if (SourceStream != null && closeStream && !Configuration.HasFlag(CFSConfiguration.LeaveOpen))
                        SourceStream.Dispose();
                }
            }
            finally
            {
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        private List<DirectoryEntry> directoryEntries = [];

        internal IList<DirectoryEntry> GetDirectories() => directoryEntries;

        internal DirectoryEntry RootEntry => directoryEntries[0];

        private IEnumerable<DirectoryEntry> FindDirectoryEntries(string entryName)
        {
            return directoryEntries.Where(d => d.GetEntryName() == entryName && d.StgType != StgType.StgInvalid).ToList();
        }

        public IList<CFItem> GetAllNamedEntries(string entryName)
        {
            var r = FindDirectoryEntries(entryName);
            return (from id in r
                    where id.GetEntryName() == entryName && id.StgType != StgType.StgInvalid
                    select (CFItem)(id.StgType == StgType.StgStorage ? new CFStorage(this, id) : new CFStream(this, id))).ToList();
        }

        public int GetNumDirectories()
        {
            AssertDisposed();
            return directoryEntries.Count;
        }

        public string GetNameDirEntry(int id)
        {
            AssertDisposed();
            if (id < 0)
                throw new Exception("Invalid Storage ID");
            return directoryEntries[id].Name;
        }

        public StgType GetStorageType(int id)
        {
            AssertDisposed();
            if (id < 0)
                throw new Exception("Invalid Storage ID");
            return directoryEntries[id].StgType;
        }

        private void AssertDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Compound File closed: cannot access data");
        }
    }

    #endregion Modified OpenMCDF
}
