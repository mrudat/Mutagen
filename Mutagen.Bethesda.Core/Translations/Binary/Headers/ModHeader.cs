using Noggog;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mutagen.Bethesda.Binary
{
    /// <summary>
    /// A ref struct that overlays on top of bytes that is able to retrive Mod header data on demand.
    /// </summary>
    public struct ModHeader
    {
        /// <summary>
        /// Game metadata to use as reference for alignment
        /// </summary>
        public GameConstants Meta { get; }
        
        /// <summary>
        /// Bytes overlaid onto
        /// </summary>
        public ReadOnlyMemorySlice<byte> Span { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="meta">Game metadata to use as reference for alignment</param>
        /// <param name="span">Span to overlay on, aligned to the start of the Group's header</param>
        public ModHeader(GameConstants meta, ReadOnlyMemorySlice<byte> span)
        {
            this.Meta = meta;
            this.Span = span.Slice(0, meta.ModHeaderLength);
        }

        /// <summary>
        /// Game release associated with header
        /// </summary>
        public GameRelease Release => Meta.Release;
        
        /// <summary>
        /// The length that the header itself takes
        /// </summary>
        public sbyte HeaderLength => Meta.ModHeaderLength;
        
        /// <summary>
        /// RecordType of the Mod header.
        /// </summary>
        public RecordType RecordType => new RecordType(BinaryPrimitives.ReadInt32LittleEndian(this.Span.Slice(0, 4)));
        
        /// <summary>
        /// The length explicitly contained in the length bytes of the header
        /// Note that for Mod headers, this is equivalent to ContentLength
        /// </summary>
        public uint RecordLength => BinaryPrimitives.ReadUInt32LittleEndian(this.Span.Slice(4, 4));
        
        /// <summary>
        /// The length of the content, excluding the header bytes.
        /// </summary>
        public uint ContentLength => BinaryPrimitives.ReadUInt32LittleEndian(this.Span.Slice(4, 4));
        
        /// <summary>
        /// Total length, including the header and its content.
        /// </summary>
        public long TotalLength => this.HeaderLength + this.ContentLength;

        /// <summary>
        /// The integer representing a Mod Header's flags enum.
        /// Since each game has its own flag Enum, this field is offered as an int that should
        /// be casted to the appropriate enum for use.
        /// </summary>
        public int Flags => BinaryPrimitives.ReadInt32LittleEndian(this.Span.Slice(8, 4));
    }

    /// <summary>
    /// A struct that overlays on top of bytes that is able to retrive Mod Record header and content data on demand.
    /// </summary>
    public struct ModHeaderFrame : IEnumerable<SubrecordPinFrame>
    {
        private readonly ModHeader _header;

        /// <summary>
        /// Raw bytes of both header and content data
        /// </summary>
        public ReadOnlyMemorySlice<byte> HeaderAndContentData { get; }

        /// <summary>
        /// Raw bytes of the content data, excluding the header
        /// </summary>
        public ReadOnlyMemorySlice<byte> Content => HeaderAndContentData.Slice(this._header.HeaderLength, checked((int)this._header.ContentLength));

        /// <summary>
        /// Total length of the Major Record, including the header and its content.
        /// </summary>
        public long TotalLength => HeaderAndContentData.Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="meta">Game metadata to use as reference for alignment</param>
        /// <param name="span">Span to overlay on, aligned to the start of the ModHeader</param>
        public ModHeaderFrame(GameConstants meta, ReadOnlyMemorySlice<byte> span)
        {
            this._header = meta.ModHeader(span);
            this.HeaderAndContentData = span.Slice(0, checked((int)this._header.TotalLength));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="header">Existing ModHeader struct</param>
        /// <param name="span">Span to overlay on, aligned to the start of the header</param>
        public ModHeaderFrame(ModHeader header, ReadOnlyMemorySlice<byte> span)
        {
            this._header = header;
            this.HeaderAndContentData = span.Slice(0, checked((int)this._header.TotalLength));
        }

        #region Header Forwarding
        /// <inheritdoc/>
        public override string? ToString() => this._header.ToString();

        /// <inheritdoc/>
        public IEnumerator<SubrecordPinFrame> GetEnumerator() => HeaderExt.EnumerateSubrecords(this).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Game metadata to use as reference for alignment
        /// </summary>
        public GameConstants Meta => _header.Meta;

        /// <summary>
        /// Game release associated with header
        /// </summary>
        public GameRelease Release => _header.Release;

        /// <summary>
        /// The length that the header itself takes
        /// </summary>
        public sbyte HeaderLength => _header.HeaderLength;

        /// <summary>
        /// RecordType of the Mod header.
        /// </summary>
        public RecordType RecordType => _header.RecordType;

        /// <summary>
        /// The length explicitly contained in the length bytes of the header
        /// Note that for Mod headers, this is equivalent to ContentLength
        /// </summary>
        public uint RecordLength => _header.RecordLength;

        /// <summary>
        /// The length of the content, excluding the header bytes.
        /// </summary>
        public uint ContentLength => _header.ContentLength;

        /// <summary>
        /// The integer representing a Mod Header's flags enum.
        /// Since each game has its own flag Enum, this field is offered as an int that should
        /// be casted to the appropriate enum for use.
        /// </summary>
        public int Flags => _header.Flags;
        #endregion
    }
}
