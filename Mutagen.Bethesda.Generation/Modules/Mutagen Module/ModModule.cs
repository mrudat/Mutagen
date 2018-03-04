﻿using Loqui;
using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation
{
    public class ModModule : GenerationModule
    {
        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj.GetObjectData().ObjectType != ObjectType.Mod) return;
            fg.AppendLine($"private Dictionary<FormID, MajorRecord> _majorRecords = new Dictionary<FormID, MajorRecord>();");
            fg.AppendLine($"public IEnumerable<MajorRecord> MajorRecords => _majorRecords.Values;");
            fg.AppendLine($"public MajorRecord this[FormID id]");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("get => _majorRecords[id];");
                fg.AppendLine("set => SetMajorRecord(id, value);");
            }

            using (var args = new FunctionWrapper(fg,
                "protected void SetMajorRecord"))
            {
                args.Add("FormID id");
                args.Add("MajorRecord record");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("switch (record)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        if (!(field is LoquiType loqui)) continue;
                        if (loqui.TargetObjectGeneration?.GetObjectData().ObjectType != ObjectType.Group) continue;
                        if (!loqui.TryGetSpecificationAsObject("T", out var subObj))
                        {
                            throw new ArgumentException();
                        }
                        fg.AppendLine($"case {subObj.Name} {field.Name.ToLower()}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"{loqui.ProtectedName}.Items.Set({field.Name.ToLower()});");
                            fg.AppendLine($"break;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException(\"Unknown Major Record type: {{record?.GetType()}}\");");
                    }
                }
            }

            await base.GenerateInClass(obj, fg);
            fg.AppendLine();
        }

        public override Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            foreach (var field in obj.IterateFields())
            {
                if (!(field is LoquiType loqui)) continue;
                if (loqui.TargetObjectGeneration?.GetObjectData().ObjectType != ObjectType.Group) continue;
                fg.AppendLine($"{field.ProtectedName}.Items.Subscribe_Enumerable_Single((change) => _majorRecords.Modify(change.Item.Key, change.Item.Value, change.AddRem));");
            }
            return base.GenerateInCtor(obj, fg);
        }
    }
}