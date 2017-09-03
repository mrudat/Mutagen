﻿using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loqui;
using System.Xml.Linq;

namespace Mutagen.Generation
{
    public class MutagenModule : GenerationModule
    {
        public override string RegionString => "Mutagen";

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            HashSet<RecordType> recordTypes = new HashSet<RecordType>();
            if (obj.TryGetTriggeringRecordType(out var recType))
            {
                recordTypes.Add(recType);
            }
            foreach (var field in obj.Fields)
            {
                var data = field.GetFieldData();
                if (!data.RecordType.HasValue) continue;
                recordTypes.Add(data.RecordType.Value);
            }
            foreach (var type in recordTypes)
            {
                fg.AppendLine($"public static readonly {nameof(RecordType)} {type.HeaderName} = new {nameof(RecordType)}(\"{type.Type}\");");
            }
        }

        public override void PreLoad(ObjectGeneration obj)
        {
            var record = obj.Node.GetAttribute("recordType");
            if (record != null)
            {
                obj.CustomData[Constants.RECORD_TYPE] = new RecordType(record);
                obj.CustomData[Constants.TRIGGERING_RECORD_TYPE] = new RecordType(record);
            }
            else
            {
                var field = obj.Node.Element(XName.Get("Fields", LoquiGenerator.Namespace))?.Elements().FirstOrDefault();
                if (field != null)
                {
                    record = field.GetAttribute("recordType");
                    if (record != null)
                    {
                        obj.CustomData[Constants.TRIGGERING_RECORD_TYPE] = new RecordType(record);
                    }
                }
            }
        }

        public override void PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            var data = new MutagenFieldData();
            var recordAttr = node.GetAttribute("recordType");
            if (recordAttr != null)
            {
                data.RecordType = new RecordType(recordAttr);
            }
            else if (field is LoquiType loqui
                && loqui.RefGen.Obj.TryGetRecordType(out var recType))
            {
                data.RecordType = recType;
            }
            else if (field is LoquiListType loquiList)
            {
                loqui = loquiList.SubTypeGeneration as LoquiType;
                if (loqui.RefGen.Obj.TryGetTriggeringRecordType(out recType))
                {
                    data.RecordType = recType;
                }
            }
            data.Optional = node.GetAttribute<bool>("optional", false);
            if (data.Optional && !data.RecordType.HasValue)
            {
                throw new ArgumentException("Cannot have an optional field if it is not a record typed field.");
            }
            data.Length = node.GetAttribute<long?>("length", null);
            if (data.Length.HasValue && data.RecordType.HasValue)
            {
                throw new ArgumentException("Cannot define both length and record type.");
            }
            if (field is ByteArrayType byteArray
                && !data.Length.HasValue)
            {
                data.Length = 4;
            }
            if (!data.Length.HasValue 
                && !data.RecordType.HasValue
                && !(field is PrimitiveType)
                && !(field is ContainerType))
            {
                throw new ArgumentException("Have to define either length or record type.");
            }
            data.IncludeInLength = node.GetAttribute<bool>("includeInLength", true);
            data.Vestigial = node.GetAttribute<bool>("vestigial", false);
            field.CustomData[Constants.DATA_KEY] = data;
        }
    }
}