using System.Reflection;
using System.Text;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector;

public abstract class AbstractCompoundFileDetailDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] CfSignatureInfo = {
        new() { Position = 0, Signature = [0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1]
        },
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

    internal enum Color
    {
        Red = 0,
        Black = 1
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

        private static Color NodeColor(IRBNode n) => n?.Color ?? Color.Black;

        public RBTree()
        {
        }

        public RBTree(IRBNode root)
        {
            Root = root;
        }

        private IRBNode Lookup(IRBNode template)
        {
            var n = Root;
            while (n != null)
            {
                int compResult = template.CompareTo(n);
                if (compResult == 0)
                {
                    return n;
                }

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
            if (oldn.Parent == null)
            {
                Root = newn;
            }
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

            if (newn != null)
            {
                newn.Parent = oldn.Parent;
            }
        }

        private void RotateL(IRBNode n)
        {
            var r = n.Right;
            Replace(n, r);
            n.Right = r.Left;
            if (r.Left != null)
            {
                r.Left.Parent = n;
            }

            r.Left = n;
            n.Parent = r;
        }

        private void RotateR(IRBNode n)
        {
            var l = n.Left;
            Replace(n, l);
            n.Left = l.Right;
            if (l.Right != null)
            {
                l.Right.Parent = n;
            }

            l.Right = n;
            n.Parent = l;
        }

        public void Insert(IRBNode newNode)
        {
            newNode.Color = Color.Red;
            if (Root == null)
            {
                Root = newNode;
            }
            else
            {
                var n = Root;
                while (true)
                {
                    var compResult = newNode.CompareTo(n);
                    if (compResult == 0)
                    {
                        throw new Exception($"RBNode {newNode} already present in tree");
                    }

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
            if (n.Parent == null)
            {
                n.Color = Color.Black;
            }
            else Insert2(n);
        }

        private void Insert2(IRBNode n)
        {
            if (NodeColor(n.Parent) == Color.Black)
            {
                return;
            }

            Insert3(n);
        }

        private void Insert3(IRBNode n)
        {
            if (NodeColor(n.Uncle()) == Color.Red)
            {
                n.Parent.Color = Color.Black;
                n.Uncle().Color = Color.Black;
                n.Grandparent().Color = Color.Red;
                Insert1(n.Grandparent());
            }
            else
            {
                Insert4(n);
            }
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
            n.Parent.Color = Color.Black;
            n.Grandparent().Color = Color.Red;
            if (n == n.Parent.Left && n.Parent == n.Grandparent().Left)
            {
                RotateR(n.Grandparent());
            }
            else
            {
                RotateL(n.Grandparent());
            }
        }

        internal void VisitTreeNodes(Action<IRBNode> action)
        {
            var walker = Root;
            if (walker != null)
            {
                DoVisitTreeNodes(action, walker);
            }
        }

        private static void DoVisitTreeNodes(Action<IRBNode> action, IRBNode walker)
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

        internal event Action<IRBNode> NodeInserted;
    }

    // http://mozilla.org/MPL/2.0/.

    internal enum SectorType
    {
        Normal,
        Mini,
        FAT,
        DIFAT,
        RangeLockSector,
        Directory
    }

    internal sealed class Sector(int size, Stream stream) : IDisposable
    {
        public static int MinisectorSize = 64;
        public const int Freesect = unchecked((int) 0xFFFFFFFF);
        public const int Endofchain = unchecked((int) 0xFFFFFFFE);

        public bool DirtyFlag { get; set; }

        public bool IsStreamed => stream != null && Size != MinisectorSize && Id * Size + Size < stream.Length;

        internal SectorType Type { get; set; }

        public int Id { get; set; } = -1;

        public int Size { get; private set; } = size;

        private byte[] _data;

        public byte[] GetData()
        {
            if (_data == null)
            {
                _data = new byte[Size];
                if (IsStreamed)
                {
                    stream.Seek(Size + Id * Size, SeekOrigin.Begin);
                    _=stream.Read(_data, 0, Size);
                }
            }

            return _data;
        }

        public void InitFATData()
        {
            _data = new byte[Size];
            for (int i = 0; i < Size; i++)
            {
                _data[i] = 0xFF;
            }

            DirtyFlag = true;
        }

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
            finally
            {
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }

    internal enum StgType
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

        internal const int Nostream = unchecked((int) 0xFFFFFFFF);

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
            return EntryName is {Length: > 0} ? Encoding.Unicode.GetString(EntryName).Remove((NameLength - 1) / 2) : string.Empty;
        }

        public void SetEntryName(string entryName)
        {
            if (entryName.Contains(@"\") || entryName.Contains("/") || entryName.Contains(":") || entryName.Contains("!"))
            {
                throw new ArgumentException("Invalid character in entry: the characters '\\', '/', ':','!' cannot be used in entry name");
            }

            if (entryName.Length > 31)
            {
                throw new ArgumentException("Entry name MUST be smaller than 31 characters");
            }

            byte[] temp = Encoding.Unicode.GetBytes(entryName);
            var newName = new byte[64];
            Buffer.BlockCopy(temp, 0, newName, 0, temp.Length);
            newName[temp.Length] = 0x00;
            newName[temp.Length + 1] = 0x00;
            EntryName = newName;
            NameLength = (ushort) (temp.Length + 2);
        }

        public ushort NameLength { get; private set; }

        public StgType StgType { get; set; }

        public Color Color { get; set; } = Color.Black;

        public int LeftSibling { get; set; } = Nostream;

        public int RightSibling { get; set; } = Nostream;

        public int Child { get; set; } = Nostream;

        public Guid StorageClsid { get; set; } = Guid.NewGuid();

        public int StateBits { get; set; }

        public byte[] CreationDate { get; set; } = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        public byte[] ModifyDate { get; set; } = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        public int StartSetc { get; set; } = Sector.Endofchain;

        public long Size { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is not DirectoryEntry otherDir)
            {
                throw new ArgumentException("Invalid casting: compared object does not implement IDirectorEntry interface");
            }

            if (NameLength > otherDir.NameLength)
            {
                return ThisIsGreater;
            }

            if (NameLength < otherDir.NameLength)
            {
                return OtherIsGreater;
            }

            var thisName = Encoding.Unicode.GetString(EntryName, 0, NameLength);
            var otherName = Encoding.Unicode.GetString(otherDir.EntryName, 0, otherDir.NameLength);
            for (int z = 0; z < thisName.Length; z++)
            {
                var thisChar = char.ToUpperInvariant(thisName[z]);
                var otherChar = char.ToUpperInvariant(otherName[z]);
                if (thisChar > otherChar)
                {
                    return ThisIsGreater;
                }

                if (thisChar < otherChar)
                {
                    return OtherIsGreater;
                }
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
            {
                h = (h * 16777619) ^ buffer[i];
            }

            return h;
        }

        public override int GetHashCode()
        {
            return (int) Fnv_hash(EntryName);
        }

        public void Read(Stream stream, int ver = 3)
        {
            using var rw = new BinaryReader(stream, Encoding.UTF8, true);
            EntryName = rw.ReadBytes(64);
            NameLength = rw.ReadUInt16();
            StgType = (StgType) rw.ReadByte();
            Color = (Color) rw.ReadByte();
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
            {
                Size = rw.ReadInt64();
            }
        }

        public string Name => GetEntryName();

        public IRBNode Left
        {
            get => LeftSibling == Nostream ? null : _dirRepository[LeftSibling];
            set
            {
                LeftSibling = (value as DirectoryEntry)?.Sid ?? Nostream;
                if (LeftSibling != Nostream)
                {
                    _dirRepository[LeftSibling].Parent = this;
                }
            }
        }

        public IRBNode Right
        {
            get => RightSibling == Nostream ? null : _dirRepository[RightSibling];
            set
            {
                RightSibling = ((DirectoryEntry) value)?.Sid ?? Nostream;
                if (RightSibling != Nostream)
                {
                    _dirRepository[RightSibling].Parent = this;
                }
            }
        }

        public IRBNode Parent { get; set; }

        public IRBNode Grandparent() => Parent?.Parent;

        public IRBNode Sibling() => (this == Parent.Left) ? Parent.Right : Parent.Left;

        public IRBNode Uncle() => Parent?.Sibling();

        internal static DirectoryEntry New(string name, StgType stgType, IList<DirectoryEntry> dirRepository)
        {
            DirectoryEntry de;
            if (dirRepository != null)
            {
                de = new DirectoryEntry(name, stgType, dirRepository);
                dirRepository.Add(de);
                de.Sid = dirRepository.Count - 1;
            }
            else
            {
                throw new ArgumentNullException(nameof(dirRepository), "Directory repository cannot be null in New() method");
            }

            return de;
        }

        internal static DirectoryEntry Mock(string name, StgType stgType) => new(name, stgType, null);

        public override string ToString() => $"{Name} [{Sid}]{(StgType == StgType.StgStream ? "Stream" : "Storage")}";
    }

    internal abstract class CFItem(CompoundFile compoundFile) : IComparable<CFItem>
    {
        protected CompoundFile CompoundFile { get; } = compoundFile;

        protected void CheckDisposed()
        {
            if (CompoundFile.IsClosed)
            {
                throw new ObjectDisposedException("Owner Compound file has been closed and owned items have been invalidated");
            }
        }

        internal DirectoryEntry DirEntry { get; set; }

        internal int CompareTo(CFItem other) => DirEntry.CompareTo(other.DirEntry);

        public int CompareTo(object obj) => DirEntry.CompareTo((obj as CFItem).DirEntry);

        public static bool operator ==(CFItem leftItem, CFItem rightItem)
        {
            if (ReferenceEquals(leftItem, rightItem))
            {
                return true;
            }

            if (((object) leftItem == null) || (object) rightItem == null)
            {
                return false;
            }

            return leftItem.CompareTo(rightItem) == 0;
        }

        public static bool operator !=(CFItem leftItem, CFItem rightItem) => !(leftItem == rightItem);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => DirEntry.GetEntryName().GetHashCode();

        public long Size => DirEntry.Size;

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
            {
                throw new Exception("Attempting to add a CFStream using an unitialized directory");
            }

            DirEntry = dirEntry;
        }

        public byte[] GetData()
        {
            CheckDisposed();
            return CompoundFile.GetData(this);
        }
    }

    internal sealed class CfStorage : CFItem
    {
        private RBTree _children;

        internal RBTree Children => _children ??= LoadChildren(DirEntry.Sid) ?? CompoundFile.CreateNewTree();

        internal CfStorage(CompoundFile compFile, DirectoryEntry dirEntry) : base(compFile)
        {
            if (dirEntry == null || dirEntry.Sid < 0)
            {
                throw new ArgumentException("Attempting to create a CFStorage using an unitialized directory");
            }

            DirEntry = dirEntry;
        }

        private RBTree LoadChildren(int sid)
        {
            var childrenTree = CompoundFile.GetChildrenTree(sid);
            DirEntry.Child = (childrenTree.Root as DirectoryEntry)?.Sid ?? DirectoryEntry.Nostream;
            return childrenTree;
        }

        public CFStream GetStream(string streamName)
        {
            CheckDisposed();
            var tmp = DirectoryEntry.Mock(streamName, StgType.StgStream);
            if (Children.TryLookup(tmp, out var outDe) && ((DirectoryEntry) outDe).StgType == StgType.StgStream)
            {
                return new CFStream(CompoundFile, (DirectoryEntry) outDe);
            }

            throw new KeyNotFoundException("Cannot find item [" + streamName + "] within the current storage");
        }

        public void VisitEntries(Action<CFItem> action, bool recursive)
        {
            CheckDisposed();
            if (action != null)
            {
                var subStorages = new List<IRBNode>();
                void InternalAction(IRBNode targetNode)
                {
                    var d = targetNode as DirectoryEntry;
                    if (d.StgType == StgType.StgStream)
                    {
                        action(new CFStream(CompoundFile, d));
                    }
                    else
                    {
                        action(new CfStorage(CompoundFile, d));
                    }

                    if (d.Child != DirectoryEntry.Nostream)
                    {
                        subStorages.Add(targetNode);
                    }
                }

                Children.VisitTreeNodes(InternalAction);
                if (recursive)
                {
                    foreach (var n in subStorages)
                    {
                        new CfStorage(CompoundFile, n as DirectoryEntry).VisitEntries(action, true);
                    }
                }
            }
        }
    }

    internal sealed class Header
    {
        public byte[] HeaderSignature { get; private set; } = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

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

        public int FirstMiniFATSectorID { get; set; } = unchecked((int) 0xFFFFFFFE);

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
            {
                DIFAT[i] = Sector.Freesect;
            }
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
            {
                DIFAT[i] = rw.ReadInt32();
            }
        }

        private void CheckVersion()
        {
            if (MajorVersion != 3 && MajorVersion != 4)
            {
                throw new InvalidDataException("Unsupported Binary File Format version: OpenMcdf only supports Compound Files with major version equal to 3 or 4 ");
            }
        }

        private readonly byte[] _oleCfsSignature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

        private void CheckSignature()
        {
            if (HeaderSignature.Where((t, i) => t != _oleCfsSignature[i]).Any())
            {
                throw new InvalidDataException("Invalid OLE structured storage file");
            }
        }
    }

    internal sealed class StreamView : Stream
    {
        private readonly int _sectorSize;
        private long _position;
        private readonly List<Sector> _sectorChain;
        private readonly Stream _stream;
        private readonly bool _isFatStream;

        public StreamView(List<Sector> sectorChain, int sectorSize, Stream stream)
        {
            if (sectorSize <= 0)
            {
                throw new Exception("Sector size must be greater than zero");
            }

            _sectorChain = sectorChain ?? throw new Exception("Sector Chain cannot be null");
            _sectorSize = sectorSize;
            _stream = stream;
        }

        public StreamView(List<Sector> sectorChain, int sectorSize, long length, Queue<Sector> availableSectors, Stream stream, bool isFatStream = false) : this(sectorChain, sectorSize, stream)
        {
            _isFatStream = isFatStream;
            AdjustLength(length, availableSectors);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
        }

        private long _length;

        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                if (_position > _length - 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _position = value;
            }
        }

        private byte[] buf = new byte[4];

        public int ReadInt32()
        {
            _=Read(buf, 0, 4);
            return buf[0] | (buf[1] << 8) | (buf[2] << 16) | (buf[3] << 24);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int nRead = 0;
            if (_sectorChain is {Count: > 0})
            {
                int secIndex = (int) (_position / _sectorSize);
                var nToRead = Math.Min(_sectorChain[0].Size - ((int) _position % _sectorSize), count);
                if (secIndex < _sectorChain.Count)
                {
                    Buffer.BlockCopy(_sectorChain[secIndex].GetData(), (int) (_position % _sectorSize), buffer, offset, nToRead);
                }

                nRead += nToRead;
                ++secIndex;
                while (nRead < (count - _sectorSize))
                {
                    nToRead = _sectorSize;
                    Buffer.BlockCopy(_sectorChain[secIndex].GetData(), 0, buffer, offset + nRead, nToRead);
                    nRead += nToRead;
                    ++secIndex;
                }

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
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = Length - offset;
                    break;
            }

            AdjustLength(_position);
            return _position;
        }

        private void AdjustLength(long value, Queue<Sector> availableSectors = null)
        {
            _length = value;
            long delta = value - _sectorChain.Count * (long) _sectorSize;

            if (delta > 0)
            {
                int nSec = (int) Math.Ceiling(((double) delta / _sectorSize));
                while (nSec > 0)
                {
                    Sector t;
                    if (availableSectors == null || availableSectors.Count == 0)
                    {
                        t = new Sector(_sectorSize, _stream);
                        if (_sectorSize == Sector.MinisectorSize)
                        {
                            t.Type = SectorType.Mini;
                        }
                    }
                    else
                    {
                        t = availableSectors.Dequeue();
                    }

                    if (_isFatStream)
                    {
                        t.InitFATData();
                    }

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
        public CFSConfiguration Configuration => CFSConfiguration.Default;

        internal int GetSectorSize() => 2 << (_header.SectorShift - 1);
        private List<Sector> _sectors = [];
        private Header _header;
        internal Stream SourceStream;

        public CompoundFile(Stream stream, CFSConfiguration configParameters)
        {
            _closeStream = !configParameters.HasFlag(CFSConfiguration.LeaveOpen);
            LoadStream(stream);
        }

        private void Load(Stream stream)
        {
            try
            {
                _header = new Header();
                _directoryEntries = new List<DirectoryEntry>();
                SourceStream = stream;
                _header.Read(stream);
                int nSector = Ceiling((stream.Length - GetSectorSize()) / (double) GetSectorSize());
                if (stream.Length > 0x7FFFFF0)
                {
                    TransactionLockAllocated = true;
                }

                _sectors = [];
                for (int i = 0; i < nSector; i++)
                {
                    _sectors.Add(null);
                }

                LoadDirectories();
                RootStorage = new CfStorage(this, _directoryEntries[0]);
            }
            catch (Exception)
            {
                if (stream != null && _closeStream)
                {
                    stream.Dispose();
                }

                throw;
            }
        }

        private void LoadStream(Stream stream)
        {
            if (stream == null)
            {
                throw new Exception("Stream parameter cannot be null");
            }

            if (!stream.CanSeek)
            {
                throw new Exception("Cannot load a non-seekable Stream");
            }

            stream.Seek(0, SeekOrigin.Begin);
            Load(stream);
        }

        internal bool TransactionLockAllocated;

        private List<Sector> GetDifatSectorChain()
        {
            var result = new List<Sector>();
            if (_header.DIFATSectorsNumber != 0)
            {
                var validationCount = (int) _header.DIFATSectorsNumber;
                var s = _sectors[_header.FirstDIFATSectorID];
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
                    var nextSecId = BitConverter.ToInt32(s.GetData(), GetSectorSize() - 4);
                    if (nextSecId is Sector.Freesect or Sector.Endofchain)
                    {
                        break;
                    }

                    validationCount--;
                    if (validationCount < 0)
                    {
                        Dispose();
                        throw new InvalidDataException("DIFAT sectors count mismatched. Corrupted compound file");
                    }

                    s = _sectors[nextSecId];
                    if (s == null)
                    {
                        _sectors[nextSecId] = s = new Sector(GetSectorSize(), SourceStream)
                        {
                            Id = nextSecId
                        };
                    }

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
            var difatSectors = GetDifatSectorChain();
            var idx = 0;
            while (idx < _header.FATSectorsNumber && idx < nHeaderFatEntry)
            {
                nextSecId = _header.DIFAT[idx];
                var s = _sectors[nextSecId];
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
                var nextDifatSectorBuffer = new byte[4];
                _=difatStream.Read(nextDifatSectorBuffer, 0, 4);
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

                    var s = _sectors[nextSecId];
                    if (s == null)
                    {
                        _sectors[nextSecId] = s = new Sector(GetSectorSize(), SourceStream)
                        {
                            Type = SectorType.FAT,
                            Id = nextSecId
                        };
                    }

                    result.Add(s);
                    _=difatStream.Read(nextDifatSectorBuffer, 0, 4);
                    nextSecId = BitConverter.ToInt32(nextDifatSectorBuffer, 0);
                    nFat++;
                }
            }

            return result;
        }

        private List<Sector> GetNormalSectorChain(int secId)
        {
            var result = new List<Sector>();
            int nextSecId = secId;
            var fatSectors = GetFatSectorChain();
            var fatStream = new StreamView(fatSectors, GetSectorSize(), fatSectors.Count * GetSectorSize(), null, SourceStream);
            while (true)
            {
                if (nextSecId == Sector.Endofchain)
                {
                    break;
                }

                if (nextSecId < 0)
                {
                    throw new InvalidDataException($"Next Sector ID reference is below zero. NextID : {nextSecId}");
                }

                if (nextSecId >= _sectors.Count)
                {
                    throw new InvalidDataException($"Next Sector ID reference an out of range sector. NextID : {nextSecId} while sector count {_sectors.Count}");
                }

                var s = _sectors[nextSecId];
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
                {
                    nextSecId = next;
                }
                else
                {
                    throw new InvalidDataException("Cyclic sector chain found. File is corrupted");
                }
            }

            return result;
        }

        private List<Sector> GetMiniSectorChain(int secId)
        {
            List<Sector> result = [];
            if (secId == Sector.Endofchain)
            {
                return result;
            }

            var miniFat = GetNormalSectorChain(_header.FirstMiniFATSectorID);
            var miniStream = GetNormalSectorChain(RootEntry.StartSetc);
            var miniFatView = new StreamView(miniFat, GetSectorSize(), _header.MiniFATSectorsNumber * Sector.MinisectorSize, null, SourceStream);
            var miniStreamView = new StreamView(miniStream, GetSectorSize(), RootStorage.Size, null, SourceStream);
            var miniFatReader = new BinaryReader(miniFatView);
            var nextSecId = secId;
            while (true)
            {
                if (nextSecId == Sector.Endofchain)
                {
                    break;
                }

                var ms = new Sector(Sector.MinisectorSize, SourceStream);
                ms.Id = nextSecId;
                ms.Type = SectorType.Mini;
                miniStreamView.Seek(nextSecId * Sector.MinisectorSize, SeekOrigin.Begin);
                _=miniStreamView.Read(ms.GetData(), 0, Sector.MinisectorSize);
                result.Add(ms);
                miniFatView.Seek(nextSecId * 4, SeekOrigin.Begin);
                nextSecId = miniFatReader.ReadInt32();
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

        public CfStorage RootStorage { get; private set; }

        public int Version => _header.MajorVersion;

        internal RBTree CreateNewTree()
        {
            return new RBTree();
        }

        internal RBTree GetChildrenTree(int sid)
        {
            var bst = new RBTree();
            DoLoadChildren(bst, _directoryEntries[sid]);
            return bst;
        }

        private void DoLoadChildren(RBTree bst, DirectoryEntry de)
        {
            if (de.Child == DirectoryEntry.Nostream)
            {
                return;
            }

            if (_directoryEntries[de.Child].StgType == StgType.StgInvalid)
            {
                return;
            }

            LoadSiblings(bst, _directoryEntries[de.Child]);
            NullifyChildNodes(_directoryEntries[de.Child]);
            bst.Insert(_directoryEntries[de.Child]);
        }

        private static void NullifyChildNodes(DirectoryEntry de)
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
            {
                DoLoadSiblings(bst, _directoryEntries[de.LeftSibling]);
            }

            if (de.RightSibling == DirectoryEntry.Nostream)
            {
                return;
            }

            _levelSiDs.Add(de.RightSibling);
            DoLoadSiblings(bst, _directoryEntries[de.RightSibling]);
        }

        private void DoLoadSiblings(RBTree bst, DirectoryEntry de)
        {
            if (ValidateSibling(de.LeftSibling))
            {
                _levelSiDs.Add(de.LeftSibling);
                DoLoadSiblings(bst, _directoryEntries[de.LeftSibling]);
            }

            if (ValidateSibling(de.RightSibling))
            {
                _levelSiDs.Add(de.RightSibling);
                DoLoadSiblings(bst, _directoryEntries[de.RightSibling]);
            }

            NullifyChildNodes(de);
            bst.Insert(de);
        }

        private bool ValidateSibling(int sid)
        {
            if (sid == DirectoryEntry.Nostream)
            {
                return false;
            }

            if (sid >= _directoryEntries.Count)
            {
                return false;
            }

            if (_directoryEntries[sid].StgType == StgType.StgInvalid)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(StgType), _directoryEntries[sid].StgType))
            {
                return false;
            }

            if (_levelSiDs.Contains(sid))
            {
                throw new InvalidDataException("Cyclic reference of directory item");
            }

            return true;

        }

        private void LoadDirectories()
        {
            var directoryChain = GetSectorChain(_header.FirstDirectorySectorID, SectorType.Normal);
            if (_header.FirstDirectorySectorID == Sector.Endofchain)
            {
                _header.FirstDirectorySectorID = directoryChain[0].Id;
            }

            var dirReader = new StreamView(directoryChain, GetSectorSize(), directoryChain.Count * GetSectorSize(), null, SourceStream);
            while (dirReader.Position < directoryChain.Count * GetSectorSize())
            {
                var de = DirectoryEntry.New(string.Empty, StgType.StgInvalid, _directoryEntries);
                de.Read(dirReader, Version);
            }
        }

        internal byte[] GetData(CFStream cFStream)
        {
            AssertDisposed();
            byte[] result;
            var de = cFStream.DirEntry;
            if (de.Size < _header.MinSizeStandardStream)
            {
                var miniView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, SourceStream);
                using var br = new BinaryReader(miniView);
                result = br.ReadBytes((int) de.Size);
            }
            else
            {
                var sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, SourceStream);
                result = new byte[(int) de.Size];
                _=sView.Read(result, 0, result.Length);
            }

            return result;
        }

        private static int Ceiling(double d) => (int) Math.Ceiling(d);

        private readonly bool _closeStream = true;

        internal bool IsClosed { get; private set; }

        #region IDisposable Members

        private readonly object _lockObject = new();

        public void Dispose()
        {
            try
            {
                if (!IsClosed)
                {
                    lock (_lockObject)
                    {
                        if (_sectors != null)
                        {
                            _sectors.Clear();
                            _sectors = null;
                        }

                        RootStorage = null;
                        _header = null;
                        _directoryEntries.Clear();
                        _directoryEntries = null;
                    }

                    if (SourceStream != null && _closeStream && !Configuration.HasFlag(CFSConfiguration.LeaveOpen))
                    {
                        SourceStream.Dispose();
                    }
                }
            }
            finally
            {
                IsClosed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        private List<DirectoryEntry> _directoryEntries = [];

        internal DirectoryEntry RootEntry => _directoryEntries[0];

        private void AssertDisposed()
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException("Compound File closed: cannot access data");
            }
        }
    }

    #endregion Modified OpenMCDF
}