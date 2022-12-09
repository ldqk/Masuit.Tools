using Masuit.Tools.AspNetCore.Mime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
			foreach (var chunk in Chunks)
			{
				var compoundFileStream = cf.RootStorage.GetStream(chunk);
				if (compoundFileStream == null || !IsValidChunk(chunk, compoundFileStream.GetData()))
				{
					return false;
				}
			}

			return true;
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
			IRBNode insertedNode = newNode;

			if (Root == null) Root = insertedNode;
			else
			{
				IRBNode n = Root;
				while (true)
				{
					int compResult = newNode.CompareTo(n);
					if (compResult == 0) throw new Exception($"RBNode {newNode} already present in tree");
					if (compResult < 0)
					{
						if (n.Left == null)
						{
							n.Left = insertedNode;
							break;
						}

						n = n.Left;
					}
					else
					{
						if (n.Right == null)
						{
							n.Right = insertedNode;
							break;
						}
						n = n.Right;
					}
				}
				insertedNode.Parent = n;
			}

			Insert1(insertedNode);
			NodeInserted?.Invoke(insertedNode);
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

		private void DoVisitTree(Action<IRBNode> action, IRBNode walker)
		{
			if (walker.Left != null)
			{
				DoVisitTree(action, walker.Left);
			}

			action?.Invoke(walker);
			if (walker.Right != null)
			{
				DoVisitTree(action, walker.Right);
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
			if (walker.Left != null)
			{
				DoVisitTreeNodes(action, walker.Left);
			}

			action?.Invoke(walker);
			if (walker.Right != null)
			{
				DoVisitTreeNodes(action, walker.Right);
			}
		}

		public class RBTreeEnumerator : IEnumerator<IRBNode>
		{
			private int position = -1;
			private Queue<IRBNode> heap = new();

			internal RBTreeEnumerator(RBTree tree)
			{ tree.VisitTreeNodes(item => heap.Enqueue(item)); }

			public IRBNode Current => heap.ElementAt(position);

			public void Dispose()
			{ }

			object System.Collections.IEnumerator.Current => heap.ElementAt(position);

			public bool MoveNext() => (++position < heap.Count);

			public void Reset()
			{ position = -1; }
		}

		public RBTreeEnumerator GetEnumerator() => new RBTreeEnumerator(this);

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
			this.Size = size;
			this._stream = stream;
		}

		public Sector(int size, byte[] data)
		{
			this.Size = size;
			this._data = data;
			this._stream = null;
		}

		public Sector(int size)
		{
			this.Size = size;
			this._data = null;
			this._stream = null;
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

		internal void ReleaseData() => this._data = null;

		private readonly object _lockObject = new Object();

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
						this._data = null;
						this.DirtyFlag = false;
						this.Id = Sector.Endofchain;
						this.Size = 0;
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
		internal const int THIS_IS_GREATER = 1;
		internal const int OTHER_IS_GREATER = -1;
		private IList<DirectoryEntry> dirRepository;

		public int SID { get; set; } = -1;

		internal const Int32 NOSTREAM = unchecked((int)0xFFFFFFFF);

		private DirectoryEntry(String name, StgType stgType, IList<DirectoryEntry> dirRepository)
		{
			this.dirRepository = dirRepository;

			this.StgType = stgType;

			switch (stgType)
			{
				case StgType.StgStream:

					StorageCLSID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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

			this.SetEntryName(name);
		}

		public byte[] EntryName { get; private set; } = new byte[64];

		public String GetEntryName()
		{
			if (EntryName != null && EntryName.Length > 0)
				return Encoding.Unicode.GetString(EntryName).Remove((NameLength - 1) / 2);
			else return String.Empty;
		}

		public void SetEntryName(String entryName)
		{
			if (entryName.Contains(@"\") || entryName.Contains(@"/") ||
				entryName.Contains(@":") || entryName.Contains(@"!"))
				throw new Exception("Invalid character in entry: the characters '\\', '/', ':','!' cannot be used in entry name");

			if (entryName.Length > 31)
				throw new Exception("Entry name MUST be smaller than 31 characters");

			byte[] newName = null;
			byte[] temp = Encoding.Unicode.GetBytes(entryName);
			newName = new byte[64];
			Buffer.BlockCopy(temp, 0, newName, 0, temp.Length);
			newName[temp.Length] = 0x00;
			newName[temp.Length + 1] = 0x00;

			EntryName = newName;
			NameLength = (ushort)(temp.Length + 2);
		}

		public ushort NameLength { get; private set; }

		public StgType StgType { get; set; } = StgType.StgInvalid;

		public Color Color { get; set; } = Color.BLACK;

		public Int32 LeftSibling { get; set; } = NOSTREAM;

		public Int32 RightSibling { get; set; } = NOSTREAM;

		public Int32 Child { get; set; } = NOSTREAM;

		public Guid StorageCLSID { get; set; } = Guid.NewGuid();

		public Int32 StateBits { get; set; }

		public byte[] CreationDate { get; set; } = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		public byte[] ModifyDate { get; set; } = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		public Int32 StartSetc { get; set; } = Sector.Endofchain;

		public long Size { get; set; }

		public int CompareTo(object obj)
		{
			DirectoryEntry otherDir = obj as DirectoryEntry;

			if (otherDir == null)
				throw new Exception("Invalid casting: compared object does not implement IDirectorEntry interface");

			if (this.NameLength > otherDir.NameLength)
				return THIS_IS_GREATER;
			else if (this.NameLength < otherDir.NameLength)
				return OTHER_IS_GREATER;
			else
			{
				String thisName = Encoding.Unicode.GetString(this.EntryName, 0, this.NameLength);
				String otherName = Encoding.Unicode.GetString(otherDir.EntryName, 0, otherDir.NameLength);

				for (int z = 0; z < thisName.Length; z++)
				{
					char thisChar = char.ToUpperInvariant(thisName[z]);
					char otherChar = char.ToUpperInvariant(otherName[z]);

					if (thisChar > otherChar)
						return THIS_IS_GREATER;
					else if (thisChar < otherChar)
						return OTHER_IS_GREATER;
				}

				return 0;
			}
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
					LeftSibling = NOSTREAM;
					RightSibling = NOSTREAM;
					Child = NOSTREAM;
				}

				StorageCLSID = new Guid(rw.ReadBytes(16));
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
				if (LeftSibling == NOSTREAM)
					return null;
				return dirRepository[LeftSibling];
			}

			set
			{
				LeftSibling = value != null ? (value as DirectoryEntry).SID : NOSTREAM;
				if (LeftSibling != NOSTREAM)
					dirRepository[LeftSibling].Parent = this;
			}
		}

		public IRBNode Right
		{
			get
			{
				if (RightSibling == NOSTREAM)
					return null;
				return dirRepository[RightSibling];
			}

			set
			{
				RightSibling = value != null ? ((DirectoryEntry)value).SID : NOSTREAM;
				if (RightSibling != NOSTREAM)
					dirRepository[RightSibling].Parent = this;
			}
		}

		public IRBNode Parent { get; set; }

		public IRBNode Grandparent() => Parent?.Parent;

		public IRBNode Sibling() => (this == Parent.Left) ? Parent.Right : Parent.Left;

		public IRBNode Uncle() => Parent?.Sibling();

		internal static DirectoryEntry New(String name, StgType stgType, IList<DirectoryEntry> dirRepository)
		{
			DirectoryEntry de = null;
			if (dirRepository != null)
			{
				de = new DirectoryEntry(name, stgType, dirRepository);
				dirRepository.Add(de);
				de.SID = dirRepository.Count - 1;
			}
			else
				throw new ArgumentNullException("dirRepository", "Directory repository cannot be null in New() method");

			return de;
		}

		internal static DirectoryEntry Mock(String name, StgType stgType) => new DirectoryEntry(name, stgType, null);

		internal static DirectoryEntry TryNew(String name, StgType stgType, IList<DirectoryEntry> dirRepository)
		{
			DirectoryEntry de = new DirectoryEntry(name, stgType, dirRepository);

			if (de != null)
			{
				for (int i = 0; i < dirRepository.Count; i++)
				{
					if (dirRepository[i].StgType == StgType.StgInvalid)
					{
						dirRepository[i] = de;
						de.SID = i;
						return de;
					}
				}
			}

			dirRepository.Add(de);
			de.SID = dirRepository.Count - 1;

			return de;
		}

		public override string ToString() => $"{Name} [{SID}]{(StgType == StgType.StgStream ? "Stream" : "Storage")}";

		public void AssignValueTo(IRBNode other)
		{
			DirectoryEntry d = other as DirectoryEntry;

			d.SetEntryName(GetEntryName());

			d.CreationDate = new byte[CreationDate.Length];
			CreationDate.CopyTo(d.CreationDate, 0);

			d.ModifyDate = new byte[ModifyDate.Length];
			ModifyDate.CopyTo(d.ModifyDate, 0);

			d.Size = Size;
			d.StartSetc = StartSetc;
			d.StateBits = StateBits;
			d.StgType = StgType;
			d.StorageCLSID = new Guid(StorageCLSID.ToByteArray());
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
		{ this.compoundFile = compoundFile; }

		internal DirectoryEntry DirEntry { get; set; }

		internal int CompareTo(CFItem other) => DirEntry.CompareTo(other.DirEntry);

		public int CompareTo(object obj) => DirEntry.CompareTo((obj as CFItem).DirEntry);

		public static bool operator ==(CFItem leftItem, CFItem rightItem)
		{
			if (System.Object.ReferenceEquals(leftItem, rightItem))
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
			get => DirEntry.StorageCLSID;

			set
			{
				if (DirEntry.StgType != StgType.StgStream)
					DirEntry.StorageCLSID = value;
				else
					throw new Exception("Object class GUID can only be set on Root and Storage entries");
			}
		}

		int IComparable<CFItem>.CompareTo(CFItem other) => DirEntry.CompareTo(other.DirEntry);

		public override string ToString()
		{
			return (DirEntry != null)
				? $"[{DirEntry.LeftSibling},{DirEntry.SID},{DirEntry.RightSibling}] {DirEntry.GetEntryName()}"
				: string.Empty;
		}
	}

	internal sealed class CFStream : CFItem
	{
		internal CFStream(CompoundFile compoundFile, DirectoryEntry dirEntry)
			: base(compoundFile)
		{
			if (dirEntry == null || dirEntry.SID < 0)
				throw new Exception("Attempting to add a CFStream using an unitialized directory");
			this.DirEntry = dirEntry;
		}

		public Byte[] GetData()
		{
			CheckDisposed();
			return this.CompoundFile.GetData(this);
		}

		public int Read(byte[] buffer, long position, int count)
		{
			CheckDisposed();
			return this.CompoundFile.ReadData(this, position, buffer, 0, count);
		}

		internal int Read(byte[] buffer, long position, int offset, int count)
		{
			CheckDisposed();
			return this.CompoundFile.ReadData(this, position, buffer, offset, count);
		}
	}

	internal sealed class CFStorage : CFItem
	{
		private RBTree children;

		internal RBTree Children
		{
			get
			{
				if (children == null)
					children = LoadChildren(this.DirEntry.SID) ?? this.CompoundFile.CreateNewTree();
				return children;
			}
		}

		internal CFStorage(CompoundFile compFile, DirectoryEntry dirEntry)
			: base(compFile)
		{
			if (dirEntry == null || dirEntry.SID < 0)
				throw new Exception("Attempting to create a CFStorage using an unitialized directory");
			this.DirEntry = dirEntry;
		}

		private RBTree LoadChildren(int SID)
		{
			RBTree childrenTree = this.CompoundFile.GetChildrenTree(SID);

			if (childrenTree.Root != null)
				this.DirEntry.Child = (childrenTree.Root as DirectoryEntry).SID;
			else
				this.DirEntry.Child = DirectoryEntry.NOSTREAM;

			return childrenTree;
		}

		public CFStream GetStream(String streamName)
		{
			CheckDisposed();

			DirectoryEntry tmp = DirectoryEntry.Mock(streamName, StgType.StgStream);

			if (Children.TryLookup(tmp, out IRBNode outDe) && (((DirectoryEntry)outDe).StgType == StgType.StgStream))
				return new CFStream(this.CompoundFile, (DirectoryEntry)outDe);
			else
				throw new KeyNotFoundException("Cannot find item [" + streamName + "] within the current storage");
		}

		public CFStream TryGetStream(String streamName)
		{
			CheckDisposed();

			DirectoryEntry tmp = DirectoryEntry.Mock(streamName, StgType.StgStream);

			if (Children.TryLookup(tmp, out IRBNode outDe) && ((outDe as DirectoryEntry).StgType == StgType.StgStream))
				return new CFStream(this.CompoundFile, (DirectoryEntry)outDe);
			else
				return null;
		}

		public CFStorage GetStorage(String storageName)
		{
			CheckDisposed();

			DirectoryEntry template = DirectoryEntry.Mock(storageName, StgType.StgInvalid);

			if (Children.TryLookup(template, out IRBNode outDe) && (outDe as DirectoryEntry).StgType == StgType.StgStorage)
				return new CFStorage(this.CompoundFile, outDe as DirectoryEntry);
			else
				throw new KeyNotFoundException("Cannot find item [" + storageName + "] within the current storage");
		}

		public CFStorage TryGetStorage(String storageName)
		{
			CheckDisposed();

			DirectoryEntry template = DirectoryEntry.Mock(storageName, StgType.StgInvalid);

			if (Children.TryLookup(template, out IRBNode outDe) && ((DirectoryEntry)outDe).StgType == StgType.StgStorage)
				return new CFStorage(this.CompoundFile, outDe as DirectoryEntry);
			else
				return null;
		}

		public void VisitEntries(Action<CFItem> action, bool recursive)
		{
			CheckDisposed();

			if (action != null)
			{
				List<IRBNode> subStorages = new List<IRBNode>();

				void internalAction(IRBNode targetNode)
				{
					DirectoryEntry d = targetNode as DirectoryEntry;
					if (d.StgType == StgType.StgStream)
						action(new CFStream(this.CompoundFile, d));
					else
						action(new CFStorage(this.CompoundFile, d));

					if (d.Child != DirectoryEntry.NOSTREAM)
						subStorages.Add(targetNode);

					return;
				}

				this.Children.VisitTreeNodes(internalAction);

				if (recursive && subStorages.Count > 0)
					foreach (IRBNode n in subStorages)
						(new CFStorage(this.CompoundFile, n as DirectoryEntry)).VisitEntries(action, recursive);
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

		public int[] DIFAT { get; private set; } = new int[109];

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
			using (BinaryReader rw = new BinaryReader(stream, Encoding.UTF8, true))
			{
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
		private readonly List<Sector> _freeSectors = new List<Sector>();

		public IEnumerable<Sector> FreeSectors => _freeSectors;

		public StreamView(List<Sector> sectorChain, int sectorSize, Stream stream)
		{
			if (sectorSize <= 0)
				throw new Exception("Sector size must be greater than zero");

			this._sectorChain = sectorChain ?? throw new Exception("Sector Chain cannot be null");
			this._sectorSize = sectorSize;
			this._stream = stream;
		}

		public StreamView(List<Sector> sectorChain, int sectorSize, long length, Queue<Sector> availableSectors, Stream stream, bool isFatStream = false)
			: this(sectorChain, sectorSize, stream)
		{
			this._isFatStream = isFatStream;
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
			this.Read(buf, 0, 4);
			return (((buf[0] | (buf[1] << 8)) | (buf[2] << 16)) | (buf[3] << 24));
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int nRead = 0;
			int nToRead = 0;

			if (_sectorChain != null && _sectorChain.Count > 0)
			{
				// First sector
				int secIndex = (int)(_position / _sectorSize);

				nToRead = Math.Min(_sectorChain[0].Size - ((int)_position % _sectorSize), count);

				if (secIndex < _sectorChain.Count)
				{
					Buffer.BlockCopy(_sectorChain[secIndex].GetData(),
						(int)(_position % _sectorSize), buffer, offset, nToRead);
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
			else
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
			this._length = value;
			long delta = value - (_sectorChain.Count * (long)_sectorSize);

			if (delta > 0)
			{
				int nSec = (int)Math.Ceiling(((double)delta / _sectorSize));
				while (nSec > 0)
				{
					Sector t = null;

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

		internal int GetSectorSize() => 2 << (header.SectorShift - 1);

		private const int HEADER_DIFAT_ENTRIES_COUNT = 109;
		private readonly int DIFAT_SECTOR_FAT_ENTRIES_COUNT = 127;
		private readonly int FAT_SECTOR_ENTRIES_COUNT = 128;
		private const int SIZE_OF_SID = 4;
		private const int FLUSHING_QUEUE_SIZE = 6000;
		private const int FLUSHING_BUFFER_MAX_SIZE = 1024 * 1024 * 16;

		private List<Sector> sectors = new List<Sector>();

		private Header header;

		internal Stream sourceStream = null;

		public CompoundFile(Stream stream, CFSConfiguration configParameters)
		{
			this.closeStream = !configParameters.HasFlag(CFSConfiguration.LeaveOpen);

			LoadStream(stream);

			DIFAT_SECTOR_FAT_ENTRIES_COUNT = (GetSectorSize() / 4) - 1;
			FAT_SECTOR_ENTRIES_COUNT = (GetSectorSize() / 4);
		}

		private string fileName = string.Empty;

		private void Load(Stream stream)
		{
			try
			{
				this.header = new Header();
				this.directoryEntries = new List<DirectoryEntry>();

				this.sourceStream = stream;

				header.Read(stream);

				int n_sector = Ceiling(((stream.Length - GetSectorSize()) / (double)GetSectorSize()));

				if (stream.Length > 0x7FFFFF0)
					this._transactionLockAllocated = true;

				sectors = new List<Sector>();
				for (int i = 0; i < n_sector; i++)
					sectors.Add(null);

				LoadDirectories();

				this.RootStorage = new CFStorage(this, directoryEntries[0]);
			}
			catch (Exception)
			{
				if (stream != null && closeStream)
					stream.Dispose();
				throw;
			}
		}

		private void LoadFile(String fileName)
		{
			this.fileName = fileName;
			FileStream fs = null;

			try
			{
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				Load(fs);
			}
			catch
			{
				if (fs != null)
					fs.Dispose();
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

		public bool HasSourceStream => sourceStream != null;

		private void PersistMiniStreamToStream(List<Sector> miniSectorChain)
		{
			List<Sector> miniStream = GetSectorChain(RootEntry.StartSetc, SectorType.Normal);

			StreamView miniStreamView = new StreamView(miniStream, GetSectorSize(), this.RootStorage.Size, null, sourceStream);

			for (int i = 0; i < miniSectorChain.Count; i++)
			{
				Sector s = miniSectorChain[i];

				if (s.Id == -1)
					throw new Exception("Invalid minisector index");

				miniStreamView.Seek(Sector.MinisectorSize * s.Id, SeekOrigin.Begin);
				miniStreamView.Write(s.GetData(), 0, Sector.MinisectorSize);
			}
		}

		private void AllocateMiniSectorChain(List<Sector> sectorChain)
		{
			List<Sector> miniFAT = GetSectorChain(header.FirstMiniFATSectorID, SectorType.Normal);
			List<Sector> miniStream = GetSectorChain(RootEntry.StartSetc, SectorType.Normal);

			StreamView miniFATView = new StreamView(miniFAT, GetSectorSize(),
				header.MiniFATSectorsNumber * Sector.MinisectorSize,
				null, this.sourceStream, true);

			StreamView miniStreamView = new StreamView(miniStream, GetSectorSize(),
				this.RootStorage.Size, null, sourceStream);

			for (int i = 0; i < sectorChain.Count; i++)
			{
				Sector s = sectorChain[i];

				if (s.Id == -1)
				{
					miniStreamView.Seek(this.RootStorage.Size + Sector.MinisectorSize, SeekOrigin.Begin);
					s.Id = (int)(miniStreamView.Position - Sector.MinisectorSize) / Sector.MinisectorSize;

					this.RootStorage.DirEntry.Size = miniStreamView.Length;
				}
			}

			for (int i = 0; i < sectorChain.Count - 1; i++)
			{
				Int32 currentId = sectorChain[i].Id;
				Int32 nextId = sectorChain[i + 1].Id;

				miniFATView.Seek(currentId * 4, SeekOrigin.Begin);
				miniFATView.Write(BitConverter.GetBytes(nextId), 0, 4);
			}

			miniFATView.Seek(sectorChain[sectorChain.Count - 1].Id * SIZE_OF_SID, SeekOrigin.Begin);
			miniFATView.Write(BitConverter.GetBytes(Sector.Endofchain), 0, 4);

			AllocateSectorChain(miniStreamView.BaseSectorChain);
			AllocateSectorChain(miniFATView.BaseSectorChain);

			if (miniFAT.Count > 0)
			{
				this.RootStorage.DirEntry.StartSetc = miniStream[0].Id;
				header.MiniFATSectorsNumber = (uint)miniFAT.Count;
				header.FirstMiniFATSectorID = miniFAT[0].Id;
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
					sectors.Add(s);
					s.Id = sectors.Count - 1;
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
				StreamView fatStream = new StreamView(GetFatSectorChain(), GetSectorSize(), sourceStream);

				fatStream.Seek(_lockSectorId * 4, SeekOrigin.Begin);
				fatStream.Write(BitConverter.GetBytes(Sector.Endofchain), 0, 4);

				_transactionLockAllocated = true;
			}
		}

		private void AllocateFATSectorChain(List<Sector> sectorChain)
		{
			List<Sector> fatSectors = GetSectorChain(-1, SectorType.FAT);

			StreamView fatStream = new StreamView(fatSectors, GetSectorSize(),
				header.FATSectorsNumber * GetSectorSize(), null,
				sourceStream, true);

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
			header.FATSectorsNumber = FATsectorChain.Count;

			foreach (Sector s in FATsectorChain)
			{
				if (s.Id == -1)
				{
					sectors.Add(s);
					s.Id = sectors.Count - 1;
					s.Type = SectorType.FAT;
				}
			}

			int nCurrentSectors = sectors.Count;

			int nDIFATSectors = (int)header.DIFATSectorsNumber;

			if (FATsectorChain.Count > HEADER_DIFAT_ENTRIES_COUNT)
			{
				nDIFATSectors = Ceiling((double)(FATsectorChain.Count - HEADER_DIFAT_ENTRIES_COUNT) / DIFAT_SECTOR_FAT_ENTRIES_COUNT);
				nDIFATSectors = LowSaturation(nDIFATSectors - (int)header.DIFATSectorsNumber); //required DIFAT
			}

			nCurrentSectors += nDIFATSectors;

			while (header.FATSectorsNumber * FAT_SECTOR_ENTRIES_COUNT < nCurrentSectors)
			{
				Sector extraFATSector = new Sector(GetSectorSize(), sourceStream);
				sectors.Add(extraFATSector);

				extraFATSector.Id = sectors.Count - 1;
				extraFATSector.Type = SectorType.FAT;

				FATsectorChain.Add(extraFATSector);

				header.FATSectorsNumber++;
				nCurrentSectors++;

				if (nDIFATSectors * DIFAT_SECTOR_FAT_ENTRIES_COUNT <
					(header.FATSectorsNumber > HEADER_DIFAT_ENTRIES_COUNT ?
						header.FATSectorsNumber - HEADER_DIFAT_ENTRIES_COUNT :
						0))
				{
					nDIFATSectors++;
					nCurrentSectors++;
				}
			}

			List<Sector> difatSectors = GetSectorChain(-1, SectorType.DIFAT);
			StreamView difatStream = new StreamView(difatSectors, GetSectorSize(), sourceStream);

			for (int i = 0; i < FATsectorChain.Count; i++)
			{
				if (i < HEADER_DIFAT_ENTRIES_COUNT)
					header.DIFAT[i] = FATsectorChain[i].Id;
				else
				{
					if (i != HEADER_DIFAT_ENTRIES_COUNT && (i - HEADER_DIFAT_ENTRIES_COUNT) % DIFAT_SECTOR_FAT_ENTRIES_COUNT == 0)
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
					sectors.Add(difatStream.BaseSectorChain[i]);
					difatStream.BaseSectorChain[i].Id = sectors.Count - 1;
					difatStream.BaseSectorChain[i].Type = SectorType.DIFAT;
				}
			}

			header.DIFATSectorsNumber = (uint)nDIFATSectors;

			if (difatStream.BaseSectorChain != null && difatStream.BaseSectorChain.Count > 0)
			{
				header.FirstDIFATSectorID = difatStream.BaseSectorChain[0].Id;
				header.DIFATSectorsNumber = (uint)difatStream.BaseSectorChain.Count;

				for (int i = 0; i < difatStream.BaseSectorChain.Count - 1; i++)
					Buffer.BlockCopy(BitConverter.GetBytes(difatStream.BaseSectorChain[i + 1].Id),
						0, difatStream.BaseSectorChain[i].GetData(),
						GetSectorSize() - sizeof(int), 4);

				Buffer.BlockCopy(BitConverter.GetBytes(Sector.Endofchain), 0,
					difatStream.BaseSectorChain[difatStream.BaseSectorChain.Count - 1].GetData(),
					GetSectorSize() - sizeof(int), sizeof(int));
			}
			else header.FirstDIFATSectorID = Sector.Endofchain;

			StreamView fatSv = new StreamView(FATsectorChain, GetSectorSize(), header.FATSectorsNumber * GetSectorSize(), null, sourceStream);

			for (int i = 0; i < header.DIFATSectorsNumber; i++)
			{
				fatSv.Seek(difatStream.BaseSectorChain[i].Id * 4, SeekOrigin.Begin);
				fatSv.Write(BitConverter.GetBytes(Sector.Difsect), 0, 4);
			}

			for (int i = 0; i < header.FATSectorsNumber; i++)
			{
				fatSv.Seek(fatSv.BaseSectorChain[i].Id * 4, SeekOrigin.Begin);
				fatSv.Write(BitConverter.GetBytes(Sector.Fatsect), 0, 4);
			}

			header.FATSectorsNumber = fatSv.BaseSectorChain.Count;
		}

		private List<Sector> GetDifatSectorChain()
		{
			int validationCount = 0;

			List<Sector> result = new List<Sector>();

			int nextSecID
				= Sector.Endofchain;

			if (header.DIFATSectorsNumber != 0)
			{
				validationCount = (int)header.DIFATSectorsNumber;

				Sector s = sectors[header.FirstDIFATSectorID];

				if (s == null)
				{
					sectors[header.FirstDIFATSectorID] = s = new Sector(GetSectorSize(), sourceStream)
					{
						Type = SectorType.DIFAT,
						Id = header.FirstDIFATSectorID
					};
				}

				result.Add(s);

				while (true && validationCount >= 0)
				{
					nextSecID = BitConverter.ToInt32(s.GetData(), GetSectorSize() - 4);

					if (nextSecID == Sector.Freesect || nextSecID == Sector.Endofchain) break;

					validationCount--;

					if (validationCount < 0)
					{
						Dispose();
						throw new InvalidDataException("DIFAT sectors count mismatched. Corrupted compound file");
					}

					s = sectors[nextSecID];

					if (s == null)
						sectors[nextSecID] = s = new Sector(GetSectorSize(), sourceStream) { Id = nextSecID };

					result.Add(s);
				}
			}

			return result;
		}

		private List<Sector> GetFatSectorChain()
		{
			int N_HEADER_FAT_ENTRY = 109;

			List<Sector> result = new List<Sector>();
			int nextSecID = Sector.Endofchain;
			List<Sector> difatSectors = GetDifatSectorChain();

			int idx = 0;
			while (idx < header.FATSectorsNumber && idx < N_HEADER_FAT_ENTRY)
			{
				nextSecID = header.DIFAT[idx];
				Sector s = sectors[nextSecID];

				if (s == null)
				{
					sectors[nextSecID] = s = new Sector(GetSectorSize(), sourceStream)
					{
						Id = nextSecID,
						Type = SectorType.FAT
					};
				}

				result.Add(s);
				++idx;
			}

			if (difatSectors.Count > 0)
			{
				var difatStream = new StreamView(difatSectors, GetSectorSize(),
					header.FATSectorsNumber > N_HEADER_FAT_ENTRY ? (header.FATSectorsNumber - N_HEADER_FAT_ENTRY) * 4 : 0,
					null, sourceStream);

				byte[] nextDIFATSectorBuffer = new byte[4];
				difatStream.Read(nextDIFATSectorBuffer, 0, 4);
				nextSecID = BitConverter.ToInt32(nextDIFATSectorBuffer, 0);

				int i = 0;
				int nFat = N_HEADER_FAT_ENTRY;

				while (nFat < header.FATSectorsNumber)
				{
					if (difatStream.Position == ((GetSectorSize() - 4) + i * GetSectorSize()))
					{
						difatStream.Seek(4, SeekOrigin.Current);
						++i;
						continue;
					}

					Sector s = sectors[nextSecID];

					if (s == null)
					{
						sectors[nextSecID] = s = new Sector(GetSectorSize(), sourceStream)
						{
							Type = SectorType.FAT,
							Id = nextSecID
						};
					}

					result.Add(s);

					difatStream.Read(nextDIFATSectorBuffer, 0, 4);
					nextSecID = BitConverter.ToInt32(nextDIFATSectorBuffer, 0);
					nFat++;
				}
			}
			return result;
		}

		private List<Sector> GetNormalSectorChain(int secID)
		{
			List<Sector> result = new List<Sector>();

			int nextSecID = secID;

			List<Sector> fatSectors = GetFatSectorChain();

			var fatStream = new StreamView(fatSectors, GetSectorSize(), fatSectors.Count * GetSectorSize(), null, sourceStream);

			while (true)
			{
				if (nextSecID == Sector.Endofchain) break;

				if (nextSecID < 0)
					throw new InvalidDataException(String.Format("Next Sector ID reference is below zero. NextID : {0}", nextSecID));

				if (nextSecID >= sectors.Count)
					throw new InvalidDataException(String.Format("Next Sector ID reference an out of range sector. NextID : {0} while sector count {1}", nextSecID, sectors.Count));

				Sector s = sectors[nextSecID];
				if (s == null)
				{
					sectors[nextSecID] = s = new Sector(GetSectorSize(), sourceStream)
					{
						Id = nextSecID,
						Type = SectorType.Normal
					};
				}

				result.Add(s);

				fatStream.Seek(nextSecID * 4, SeekOrigin.Begin);
				int next = fatStream.ReadInt32();

				if (next != nextSecID)
					nextSecID = next;
				else
					throw new InvalidDataException("Cyclic sector chain found. File is corrupted");
			}

			return result;
		}

		private List<Sector> GetMiniSectorChain(int secID)
		{
			List<Sector> result = new List<Sector>();

			if (secID != Sector.Endofchain)
			{
				int nextSecID = secID;

				List<Sector> miniFAT = GetNormalSectorChain(header.FirstMiniFATSectorID);
				List<Sector> miniStream = GetNormalSectorChain(RootEntry.StartSetc);

				StreamView miniFATView = new StreamView(miniFAT, GetSectorSize(), header.MiniFATSectorsNumber * Sector.MinisectorSize, null, sourceStream);
				StreamView miniStreamView = new StreamView(miniStream, GetSectorSize(), RootStorage.Size, null, sourceStream);
				BinaryReader miniFATReader = new BinaryReader(miniFATView);

				nextSecID = secID;

				while (true)
				{
					if (nextSecID == Sector.Endofchain)
						break;

					Sector ms = new Sector(Sector.MinisectorSize, sourceStream);
					byte[] temp = new byte[Sector.MinisectorSize];

					ms.Id = nextSecID;
					ms.Type = SectorType.Mini;

					miniStreamView.Seek(nextSecID * Sector.MinisectorSize, SeekOrigin.Begin);
					miniStreamView.Read(ms.GetData(), 0, Sector.MinisectorSize);

					result.Add(ms);

					miniFATView.Seek(nextSecID * 4, SeekOrigin.Begin);
					nextSecID = miniFATReader.ReadInt32();
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

		public int Version => this.header.MajorVersion;

		internal RBTree CreateNewTree()
		{
			RBTree bst = new RBTree();
			return bst;
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
			if (de.Child != DirectoryEntry.NOSTREAM)
				bst = new RBTree(directoryEntries[de.Child]);
			return bst;
		}

		private void DoLoadChildren(RBTree bst, DirectoryEntry de)
		{
			if (de.Child != DirectoryEntry.NOSTREAM)
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

		private readonly List<int> _levelSiDs = new List<int>();

		private void LoadSiblings(RBTree bst, DirectoryEntry de)
		{
			_levelSiDs.Clear();

			if (de.LeftSibling != DirectoryEntry.NOSTREAM)
				DoLoadSiblings(bst, directoryEntries[de.LeftSibling]);

			if (de.RightSibling != DirectoryEntry.NOSTREAM)
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
			if (sid != DirectoryEntry.NOSTREAM)
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
			List<Sector> directoryChain = GetSectorChain(header.FirstDirectorySectorID, SectorType.Normal);

			if (header.FirstDirectorySectorID == Sector.Endofchain)
				header.FirstDirectorySectorID = directoryChain[0].Id;

			StreamView dirReader = new StreamView(directoryChain, GetSectorSize(), directoryChain.Count * GetSectorSize(), null, sourceStream);

			while (dirReader.Position < directoryChain.Count * GetSectorSize())
			{
				DirectoryEntry de = DirectoryEntry.New(String.Empty, StgType.StgInvalid, directoryEntries);
				de.Read(dirReader, this.Version);
			}
		}

		private void CheckFileLength() => throw new NotImplementedException();

		internal int ReadData(CFStream cFStream, long position, byte[] buffer, int count)
		{
			if (count > buffer.Length)
				throw new ArgumentException("count parameter exceeds buffer size");

			DirectoryEntry de = cFStream.DirEntry;

			count = (int)Math.Min(de.Size - position, count);

			StreamView sView = null;
			if (de.Size < header.MinSizeStandardStream)
				sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, sourceStream);
			else
				sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, sourceStream);

			sView.Seek(position, SeekOrigin.Begin);
			int result = sView.Read(buffer, 0, count);

			return result;
		}

		internal int ReadData(CFStream cFStream, long position, byte[] buffer, int offset, int count)
		{
			DirectoryEntry de = cFStream.DirEntry;

			count = (int)Math.Min(de.Size - offset, count);

			StreamView sView = null;
			if (de.Size < header.MinSizeStandardStream)
				sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, sourceStream);
			else
				sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, sourceStream);

			sView.Seek(position, SeekOrigin.Begin);
			int result = sView.Read(buffer, offset, count);

			return result;
		}

		internal byte[] GetData(CFStream cFStream)
		{
			AssertDisposed();

			byte[] result = null;

			DirectoryEntry de = cFStream.DirEntry;

			if (de.Size < header.MinSizeStandardStream)
			{
				var miniView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, sourceStream);

				using (BinaryReader br = new BinaryReader(miniView))
					result = br.ReadBytes((int)de.Size);
			}
			else
			{
				var sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, sourceStream);
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
			byte[] result = null;
			try
			{
				DirectoryEntry de = directoryEntries[sid];
				if (de.Size < header.MinSizeStandardStream)
				{
					var miniView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Mini), Sector.MinisectorSize, de.Size, null, sourceStream);
					BinaryReader br = new BinaryReader(miniView);
					result = br.ReadBytes((int)de.Size);
					br.Dispose();
				}
				else
				{
					var sView = new StreamView(GetSectorChain(de.StartSetc, SectorType.Normal), GetSectorSize(), de.Size, null, sourceStream);
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
			DirectoryEntry de = directoryEntries[sid];
			return de.StorageCLSID;
		}

		public Guid GetGuidForStream(int sid)
		{
			AssertDisposed();
			if (sid < 0)
				throw new Exception("Invalid SID");
			Guid g = new Guid("00000000000000000000000000000000");
			for (int i = sid - 1; i >= 0; i--)
			{
				if (directoryEntries[i].StorageCLSID != g && directoryEntries[i].StgType == StgType.StgStorage)
					return directoryEntries[i].StorageCLSID;
			}
			return g;
		}

		private static int Ceiling(double d) => (int)Math.Ceiling(d);

		private static int LowSaturation(int i) => i > 0 ? i : 0;

		private bool closeStream = true;

		internal bool IsClosed => _disposed;

		#region IDisposable Members

		private bool _disposed;//false
		private object lockObject = new Object();

		public void Dispose()
		{
			try
			{
				if (!_disposed)
				{
					lock (lockObject)
					{
						if (sectors != null)
						{
							sectors.Clear();
							sectors = null;
						}

						this.RootStorage = null;
						this.header = null;
						this.directoryEntries.Clear();
						this.directoryEntries = null;
						this.fileName = null;
					}

					if (this.sourceStream != null && closeStream && !Configuration.HasFlag(CFSConfiguration.LeaveOpen))
						this.sourceStream.Dispose();
				}
			}
			finally
			{
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Members

		private List<DirectoryEntry> directoryEntries = new List<DirectoryEntry>();

		internal IList<DirectoryEntry> GetDirectories() => directoryEntries;

		internal DirectoryEntry RootEntry => directoryEntries[0];

		private IList<DirectoryEntry> FindDirectoryEntries(String entryName)
		{
			List<DirectoryEntry> result = new List<DirectoryEntry>();

			foreach (DirectoryEntry d in directoryEntries)
				if (d.GetEntryName() == entryName && d.StgType != StgType.StgInvalid)
					result.Add(d);

			return result;
		}

		public IList<CFItem> GetAllNamedEntries(String entryName)
		{
			IList<DirectoryEntry> r = FindDirectoryEntries(entryName);
			List<CFItem> result = new List<CFItem>();

			foreach (DirectoryEntry id in r)
				if (id.GetEntryName() == entryName && id.StgType != StgType.StgInvalid)
					result.Add(id.StgType == StgType.StgStorage ? new CFStorage(this, id) : new CFStream(this, id));

			return result;
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
