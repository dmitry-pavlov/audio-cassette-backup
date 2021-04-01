using Kaitai;
using System.Collections.Generic;

namespace AudioCassetteBackup.Storage
{
    /// <summary>
    /// A lemonade file stores blocks of data as if they are written to magnetic
    /// tape. Contents of this file can be viewed as a very
    /// simple linear filesystem, storing named files with some basic
    /// meta information prepended as a header.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://github.com/dmitry-pavlov/audio-cassette-backup">Source</a>
    /// </remarks>
    public partial class LemonadeFile : KaitaiStruct
    {
        public static LemonadeFile FromFile(string fileName)
        {
            return new LemonadeFile(new KaitaiStream(fileName));
        }


        public enum FlagEnum
        {
            Header = 0,
            Data = 255,
        }

        public enum HeaderTypeEnum
        {
            Program = 0,
            NumArray = 1,
            CharArray = 2,
            Bytes = 3,
        }
        public LemonadeFile(KaitaiStream p__io, KaitaiStruct p__parent = null, LemonadeFile p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _blocks = new List<Block>();
            {
                var i = 0;
                while (!m_io.IsEof)
                {
                    _blocks.Add(new Block(m_io, this, m_root));
                    i++;
                }
            }
        }
        public partial class Block : KaitaiStruct
        {
            public static Block FromFile(string fileName)
            {
                return new Block(new KaitaiStream(fileName));
            }

            public Block(KaitaiStream p__io, LemonadeFile p__parent = null, LemonadeFile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _lenBlock = m_io.ReadU2le();
                _flag = ((LemonadeFile.FlagEnum)m_io.ReadU1());
                if (((LenBlock == 19) && (Flag == LemonadeFile.FlagEnum.Header)))
                {
                    _header = new Header(m_io, this, m_root);
                }
                if (LenBlock == 19)
                {
                    _data = m_io.ReadBytes((Header.LenData + 4));
                }
                if (Flag == LemonadeFile.FlagEnum.Data)
                {
                    _headerlessData = m_io.ReadBytes((LenBlock - 1));
                }
            }
            private ushort _lenBlock;
            private FlagEnum _flag;
            private Header _header;
            private byte[] _data;
            private byte[] _headerlessData;
            private LemonadeFile m_root;
            private LemonadeFile m_parent;
            public ushort LenBlock { get { return _lenBlock; } }
            public FlagEnum Flag { get { return _flag; } }
            public Header Header { get { return _header; } }
            public byte[] Data { get { return _data; } }
            public byte[] HeaderlessData { get { return _headerlessData; } }
            public LemonadeFile M_Root { get { return m_root; } }
            public LemonadeFile M_Parent { get { return m_parent; } }
        }
        public partial class ProgramParams : KaitaiStruct
        {
            public static ProgramParams FromFile(string fileName)
            {
                return new ProgramParams(new KaitaiStream(fileName));
            }

            public ProgramParams(KaitaiStream p__io, LemonadeFile.Header p__parent = null, LemonadeFile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _autostartLine = m_io.ReadU2le();
                _lenProgram = m_io.ReadU2le();
            }
            private ushort _autostartLine;
            private ushort _lenProgram;
            private LemonadeFile m_root;
            private LemonadeFile.Header m_parent;
            public ushort AutostartLine { get { return _autostartLine; } }
            public ushort LenProgram { get { return _lenProgram; } }
            public LemonadeFile M_Root { get { return m_root; } }
            public LemonadeFile.Header M_Parent { get { return m_parent; } }
        }
        public partial class BytesParams : KaitaiStruct
        {
            public static BytesParams FromFile(string fileName)
            {
                return new BytesParams(new KaitaiStream(fileName));
            }

            public BytesParams(KaitaiStream p__io, LemonadeFile.Header p__parent = null, LemonadeFile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _startAddress = m_io.ReadU2le();
                _reserved = m_io.ReadBytes(2);
            }
            private ushort _startAddress;
            private byte[] _reserved;
            private LemonadeFile m_root;
            private LemonadeFile.Header m_parent;
            public ushort StartAddress { get { return _startAddress; } }
            public byte[] Reserved { get { return _reserved; } }
            public LemonadeFile M_Root { get { return m_root; } }
            public LemonadeFile.Header M_Parent { get { return m_parent; } }
        }
        public partial class Header : KaitaiStruct
        {
            public static Header FromFile(string fileName)
            {
                return new Header(new KaitaiStream(fileName));
            }

            public Header(KaitaiStream p__io, LemonadeFile.Block p__parent = null, LemonadeFile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _headerType = ((LemonadeFile.HeaderTypeEnum)m_io.ReadU1());
                _filename = KaitaiStream.BytesStripRight(m_io.ReadBytes(10), 32);
                _lenData = m_io.ReadU2le();
                switch (HeaderType)
                {
                    case LemonadeFile.HeaderTypeEnum.Program:
                        {
                            _params = new ProgramParams(m_io, this, m_root);
                            break;
                        }
                    case LemonadeFile.HeaderTypeEnum.NumArray:
                        {
                            _params = new ArrayParams(m_io, this, m_root);
                            break;
                        }
                    case LemonadeFile.HeaderTypeEnum.CharArray:
                        {
                            _params = new ArrayParams(m_io, this, m_root);
                            break;
                        }
                    case LemonadeFile.HeaderTypeEnum.Bytes:
                        {
                            _params = new BytesParams(m_io, this, m_root);
                            break;
                        }
                }
                _checksum = m_io.ReadU1();
            }
            private HeaderTypeEnum _headerType;
            private byte[] _filename;
            private ushort _lenData;
            private KaitaiStruct _params;
            private byte _checksum;
            private LemonadeFile m_root;
            private LemonadeFile.Block m_parent;
            public HeaderTypeEnum HeaderType { get { return _headerType; } }
            public byte[] Filename { get { return _filename; } }
            public ushort LenData { get { return _lenData; } }
            public KaitaiStruct Params { get { return _params; } }

            /// <summary>
            /// Bitwise XOR of all bytes including the flag byte
            /// </summary>
            public byte Checksum { get { return _checksum; } }
            public LemonadeFile M_Root { get { return m_root; } }
            public LemonadeFile.Block M_Parent { get { return m_parent; } }
        }
        public partial class ArrayParams : KaitaiStruct
        {
            public static ArrayParams FromFile(string fileName)
            {
                return new ArrayParams(new KaitaiStream(fileName));
            }

            public ArrayParams(KaitaiStream p__io, LemonadeFile.Header p__parent = null, LemonadeFile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _reserved = m_io.ReadU1();
                _varName = m_io.ReadU1();
                _reserved1 = m_io.ReadBytes(2);
                if (!((KaitaiStream.ByteArrayCompare(Reserved1, new byte[] { 0, 128 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 0, 128 }, Reserved1, M_Io, "/types/array_params/seq/2");
                }
            }
            private byte _reserved;
            private byte _varName;
            private byte[] _reserved1;
            private LemonadeFile m_root;
            private LemonadeFile.Header m_parent;
            public byte Reserved { get { return _reserved; } }

            /// <summary>
            /// Variable name (1..26 meaning A$..Z$ +192)
            /// </summary>
            public byte VarName { get { return _varName; } }
            public byte[] Reserved1 { get { return _reserved1; } }
            public LemonadeFile M_Root { get { return m_root; } }
            public LemonadeFile.Header M_Parent { get { return m_parent; } }
        }
        private List<Block> _blocks;
        private LemonadeFile m_root;
        private KaitaiStruct m_parent;
        public List<Block> Blocks { get { return _blocks; } }
        public LemonadeFile M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

