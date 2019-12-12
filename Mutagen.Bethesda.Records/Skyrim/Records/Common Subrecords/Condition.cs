﻿using Loqui.Internal;
using Mutagen.Bethesda.Binary;
using Mutagen.Bethesda.Skyrim.Internals;
using Noggog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutagen.Bethesda.Skyrim
{
    public partial class Condition
    {
        // ToDo
        // Confirm correctness and completeness
        [Flags]
        public enum Flag
        {
            OR = 0x01,
            ParametersUseAliases = 0x02,
            UseGlobal = 0x04,
            UsePackData = 0x08,
            SwapSubjectAndTarget = 0x10
        }

        public enum RunOnType
        {
            Subject = 0,
            Target = 1,
            Reference = 2,
            CombatTarget = 3,
            LinkedReference = 4,
            QuestAlias = 5,
            PackageData = 6,
            EventData = 7,
        }

        public static Condition CreateFromBinary(
            MutagenFrame frame,
            MasterReferences masterReferences,
            RecordTypeConverter recordTypeConverter,
            ErrorMaskBuilder errorMask)
        {
            var subRecMeta = frame.MetaData.GetSubRecord(frame);
            if (subRecMeta.RecordType != Condition_Registration.CTDA_HEADER)
            {
                throw new ArgumentException();
            }
            var flagByte = frame.GetUInt8(subRecMeta.HeaderLength);
            Condition.Flag flag = ConditionBinaryCreateTranslation.GetFlag(flagByte);
            if (flag.HasFlag(Condition.Flag.UseGlobal))
            {
                return ConditionGlobal.CreateFromBinary(frame.SpawnWithLength(subRecMeta.RecordLength, checkFraming: false), masterReferences);
            }
            else
            {
                return ConditionFloat.CreateFromBinary(frame.SpawnWithLength(subRecMeta.RecordLength, checkFraming: false), masterReferences);
            }
        }
    }

    namespace Internals
    {
        public partial class Condition_Registration
        {
            public static readonly RecordType CIS1_HEADER = new RecordType("CIS1");
            public static readonly RecordType CIS2_HEADER = new RecordType("CIS2");
        }

        public partial class ConditionBinaryCreateTranslation
        {
            public const byte CompareMask = 0xE0;
            public const byte FlagMask = 0x1F;
            public const int EventFunctionIndex = 4672;

            public static Condition.Flag GetFlag(byte b)
            {
                return (Condition.Flag)(FlagMask & b);
            }

            public static CompareOperator GetCompareOperator(byte b)
            {
                return (CompareOperator)((CompareMask & b) >> 5);
            }

            static partial void FillBinaryFlagsCustom(MutagenFrame frame, ICondition item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                byte b = frame.ReadUInt8();
                item.Flags = GetFlag(b);
                item.CompareOperator = GetCompareOperator(b);
            }

            public static void CustomStringImports(MutagenFrame frame, IConditionData item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                if (!frame.MetaData.TryGetSubrecordFrame(frame.Reader, out var subMeta)) return;
                if (!(item is IFunctionConditionDataInternal funcData)) return;
                switch (subMeta.Header.RecordType.TypeInt)
                {
                    case 0x31534943: // CIS1
                        funcData.ParameterOneString = BinaryStringUtility.ProcessWholeToZString(subMeta.ContentSpan);
                        break;
                    case 0x32534943: // CIS2
                        funcData.ParameterTwoString = BinaryStringUtility.ProcessWholeToZString(subMeta.ContentSpan);
                        break;
                    default:
                        return;
                }
                frame.Position += subMeta.Header.TotalLength;
            }
        }

        public partial class ConditionBinaryWriteTranslation
        {
            public static byte GetFlagWriteByte(Condition.Flag flag, CompareOperator compare)
            {
                int b = ((int)flag) & 0x1F;
                int b2 = ((int)compare) << 5;
                return (byte)(b & b2);
            }

            static partial void WriteBinaryFlagsCustom(MutagenWriter writer, IConditionGetter item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                writer.Write(GetFlagWriteByte(item.Flags, item.CompareOperator));
            }

            public static void CustomStringExports(MutagenWriter writer, IConditionDataGetter obj, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                if (!(obj is IFunctionConditionDataGetter funcData)) return;
                if (funcData.ParameterOneString_IsSet)
                {
                    using (HeaderExport.ExportSubRecordHeader(writer, Condition_Registration.CIS1_HEADER))
                    {
                        StringBinaryTranslation.WriteString(writer, funcData.ParameterOneString, true);
                    }
                }
                if (funcData.ParameterTwoString_IsSet)
                {
                    using (HeaderExport.ExportSubRecordHeader(writer, Condition_Registration.CIS2_HEADER))
                    {
                        StringBinaryTranslation.WriteString(writer, funcData.ParameterTwoString, true);
                    }
                }
            }
        }

        public partial class ConditionBinaryWrapper
        {
            private Condition.Flag GetFlagsCustom(int location) => ConditionBinaryCreateTranslation.GetFlag(_data.Span[location]);
            public CompareOperator CompareOperator => ConditionBinaryCreateTranslation.GetCompareOperator(_data.Span[0]);

            public static ConditionBinaryWrapper ConditionFactory(BinaryMemoryReadStream stream, BinaryWrapperFactoryPackage package)
            {
                throw new NotImplementedException();
            }
        }

        public partial class ConditionGlobalBinaryCreateTranslation
        {
            static partial void FillBinaryDataCustom(MutagenFrame frame, IConditionGlobal item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                var functionIndex = frame.GetUInt16();
                if (functionIndex == ConditionBinaryCreateTranslation.EventFunctionIndex)
                {
                    item.Data = GetEventData.CreateFromBinary(frame, masterReferences);
                }
                else
                {
                    item.Data = FunctionConditionData.CreateFromBinary(frame, masterReferences);
                }
            }

            static partial void CustomBinaryEndImport(MutagenFrame frame, IConditionGlobal obj, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                ConditionBinaryCreateTranslation.CustomStringImports(frame, obj.Data, masterReferences, errorMask);
            }
        }

        public partial class ConditionGlobalBinaryWriteTranslation
        {
            static partial void WriteBinaryDataCustom(MutagenWriter writer, IConditionGlobalGetter item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                item.Data.WriteToBinary(writer, masterReferences);
            }

            static partial void CustomBinaryEndExport(MutagenWriter writer, IConditionGlobalGetter obj, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                ConditionBinaryWriteTranslation.CustomStringExports(writer, obj.Data, masterReferences, errorMask);
            }
        }

        public partial class ConditionFloatBinaryCreateTranslation
        {
            static partial void FillBinaryDataCustom(MutagenFrame frame, IConditionFloat item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                var functionIndex = frame.GetUInt16();
                if (functionIndex == ConditionBinaryCreateTranslation.EventFunctionIndex)
                {
                    item.Data = GetEventData.CreateFromBinary(frame, masterReferences);
                }
                else
                {
                    item.Data = FunctionConditionData.CreateFromBinary(frame, masterReferences);
                }
            }

            static partial void CustomBinaryEndImport(MutagenFrame frame, IConditionFloat obj, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                ConditionBinaryCreateTranslation.CustomStringImports(frame, obj.Data, masterReferences, errorMask);
            }
        }

        public partial class ConditionFloatBinaryWriteTranslation
        {
            static partial void WriteBinaryDataCustom(MutagenWriter writer, IConditionFloatGetter item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                item.Data.WriteToBinary(writer, masterReferences);
            }

            static partial void CustomBinaryEndExport(MutagenWriter writer, IConditionFloatGetter obj, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                ConditionBinaryWriteTranslation.CustomStringExports(writer, obj.Data, masterReferences, errorMask);
            }
        }

        public partial class FunctionConditionDataBinaryCreateTranslation
        {
            static partial void FillBinaryParameterParsingCustom(MutagenFrame frame, IFunctionConditionDataInternal item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                item.ParameterOneNumber = frame.ReadInt32();
                item.ParameterTwoNumber = frame.ReadInt32();
                item.ParameterOneRecord = new FormIDLink<SkyrimMajorRecord>(FormKey.Factory(masterReferences, (uint)item.ParameterOneNumber));
                item.ParameterTwoRecord = new FormIDLink<SkyrimMajorRecord>(FormKey.Factory(masterReferences, (uint)item.ParameterTwoNumber));
                item.Unknown3 = frame.ReadInt32();
                item.Unknown4 = frame.ReadInt32();
                item.Unknown5 = frame.ReadInt32();
            }
        }

        public partial class FunctionConditionDataBinaryWriteTranslation
        {
            static partial void WriteBinaryParameterParsingCustom(MutagenWriter writer, IFunctionConditionDataGetter item, MasterReferences masterReferences, ErrorMaskBuilder errorMask)
            {
                writer.Write(item.ParameterOneNumber);
                writer.Write(item.ParameterTwoNumber);
                writer.Write(item.Unknown3);
                writer.Write(item.Unknown4);
                writer.Write(item.Unknown5);
            }
        }

        public partial class FunctionConditionDataBinaryWrapper
        {
            public IFormIDLinkGetter<ISkyrimMajorRecordGetter> ParameterOneRecord => throw new NotImplementedException();

            public int ParameterOneNumber => throw new NotImplementedException();

            public IFormIDLinkGetter<ISkyrimMajorRecordGetter> ParameterTwoRecord => throw new NotImplementedException();

            public int ParameterTwoNumber => throw new NotImplementedException();

            public string ParameterOneString => throw new NotImplementedException();

            public string ParameterTwoString => throw new NotImplementedException();

            public bool ParameterOneString_IsSet => throw new NotImplementedException();

            public bool ParameterTwoString_IsSet => throw new NotImplementedException();

            public int Unknown3 => throw new NotImplementedException();

            public int Unknown4 => throw new NotImplementedException();

            public int Unknown5 => throw new NotImplementedException();
        }

        public partial class ConditionGlobalBinaryWrapper
        {
            private IConditionDataGetter GetDataCustom(int location) => throw new NotImplementedException();
        }

        public partial class ConditionFloatBinaryWrapper
        {
            private IConditionDataGetter GetDataCustom(int location) => throw new NotImplementedException();
        }
    }
}
