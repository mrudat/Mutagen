﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Internals;
using Noggog;

namespace Mutagen.Bethesda.Tests
{
    public class Skyrim_Passthrough_Test : PassthroughTest
    {
        public override GameMode GameMode => GameMode.Skyrim;

        public Skyrim_Passthrough_Test(TestingSettings settings, Target target) 
            : base(settings, target)
        {
        }

        public override ModRecordAligner.AlignmentRules GetAlignmentRules()
        {
            return new ModRecordAligner.AlignmentRules();
        }

        protected override async Task<IModGetter> ImportBinaryWrapper(FilePath path)
        {
            var bytes = File.ReadAllBytes(this.FilePath.Path);
            return SkyrimModBinaryWrapper.SkyrimModFactory(
                new MemorySlice<byte>(bytes),
                this.ModKey);
        }

        protected override async Task<IMod> ImportBinary(FilePath path)
        {
            return await SkyrimMod.CreateFromBinary(path.Path, this.ModKey);
        }

        protected override Processor ProcessorFactory() => new SkyrimProcessor();

        protected override async Task<IMod> ImportXmlFolder(DirectoryPath dir)
        {
            return await SkyrimMod.CreateFromXmlFolder(dir, this.ModKey);
        }

        protected override Task WriteXmlFolder(IModGetter mod, DirectoryPath dir)
        {
            return ((SkyrimMod)mod).WriteToXmlFolder(dir);
        }
    }
}
