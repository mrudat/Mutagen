﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loqui;
using Loqui.Internal;
using Mutagen.Bethesda.Binary;
using Mutagen.Bethesda.Internals;
using Mutagen.Bethesda.Oblivion.Internals;

namespace Mutagen.Bethesda.Oblivion
{
    public partial class DialogTopic
    {
        private byte[] _overallTimeStamp;

        static partial void CustomBinaryEnd_Import(MutagenFrame frame, DialogTopic obj, ErrorMaskBuilder errorMask)
        {
            if (frame.Reader.Complete) return;
            var next = HeaderTranslation.GetNextType(frame.Reader, out var len, hopGroup: false);
            if (!next.Equals(Group_Registration.GRUP_HEADER)) return;
            frame.Reader.Position += 8;
            var id = FormID.Factory(frame.Reader.ReadUInt32());
            var grupType = (GroupTypeEnum)frame.Reader.ReadInt32();
            if (grupType == GroupTypeEnum.TopicChildren)
            {
                obj._overallTimeStamp = frame.Reader.ReadBytes(4);
                if (id != obj.FormID)
                {
                    throw new ArgumentException("Dialog children group did not match the FormID of the parent.");
                }
            }
            else
            {
                frame.Reader.Position -= 16;
                return;
            }
            using (var subFrame = frame.SpawnWithLength(len - Mutagen.Bethesda.Constants.RECORD_HEADER_LENGTH))
            {
                Mutagen.Bethesda.Binary.ListBinaryTranslation<DialogItem>.Instance.ParseRepeatedItem(
                    frame: subFrame,
                    fieldIndex: (int)DialogTopic_FieldIndex.Items,
                    lengthLength: 4,
                    item: obj.Items,
                    errorMask: errorMask,
                    transl: (MutagenFrame r, RecordType header, out DialogItem listItem, ErrorMaskBuilder listErrorMask) =>
                    {
                        return LoquiBinaryTranslation<DialogItem>.Instance.Parse(
                            frame: r,
                            item: out listItem,
                            errorMask: listErrorMask);
                    }
                    );
            }
        }

        static partial void CustomBinaryEnd_Export(MutagenWriter writer, DialogTopic obj, ErrorMaskBuilder errorMask)
        {
            if (obj.Items.Count == 0) return;
            using (HeaderExport.ExportHeader(writer, Group_Registration.GRUP_HEADER, ObjectType.Group))
            {
                FormIDBinaryTranslation.Instance.Write(
                    writer,
                    obj.FormID,
                    errorMask);
                writer.Write((int)GroupTypeEnum.TopicChildren);
                if (obj._overallTimeStamp != null)
                {
                    writer.Write(obj._overallTimeStamp);
                }
                else
                {
                    writer.WriteZeros(4);
                }
                Mutagen.Bethesda.Binary.ListBinaryTranslation<DialogItem>.Instance.Write(
                    writer: writer,
                    items: obj.Items,
                    fieldIndex: (int)DialogTopic_FieldIndex.Items,
                    errorMask: errorMask,
                    transl: (MutagenWriter subWriter, DialogItem subItem, ErrorMaskBuilder listErrMask) =>
                    {
                        LoquiBinaryTranslation<DialogItem>.Instance.Write(
                            writer: subWriter,
                            item: subItem,
                            errorMask: listErrMask);
                    });
            }
        }
    }
}
