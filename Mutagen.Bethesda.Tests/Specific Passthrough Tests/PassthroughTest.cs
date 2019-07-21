using Mutagen.Bethesda.Binary;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Oblivion.Internals;
using Mutagen.Bethesda.Preprocessing;
using Noggog;
using Noggog.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mutagen.Bethesda.Tests
{
    public abstract class PassthroughTest
    {
        public string Nickname { get; }
        public FilePath FilePath { get; set; }
        public byte NumMasters { get; }
        public PassthroughSettings Settings { get; }
        public Target Target { get; }
        public string ExportFileName(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_NormalExport");
        public string ObservableExportFileName(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_ObservableExport");
        public string UncompressedFileName(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_Uncompressed");
        public string AlignedFileName(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_Aligned");
        public string OrderedFileName(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_Ordered");
        public string ProcessedPath(TempFolder tmp) => Path.Combine(tmp.Dir.Path, $"{this.Nickname}_Processed");

        public abstract GameMode GameMode { get; }
        public readonly MetaDataConstants Meta;

        public PassthroughTest(TestingSettings settings, Target target)
        {
            this.FilePath = Path.Combine(settings.DataFolderLocations.Get(target.GameMode), target.Path);
            this.Nickname = target.Path;
            this.NumMasters = target.NumMasters;
            this.Settings = settings.PassthroughSettings;
            this.Target = target;
            this.Meta = MetaDataConstants.Get(this.GameMode);
        }

        public abstract ModRecordAligner.AlignmentRules GetAlignmentRules();

        protected virtual BinaryFileProcessor.Config GetInstructions(
            Dictionary<long, uint> lengthTracker,
            RecordLocator.FileLocations fileLocs)
        {
            return new BinaryFileProcessor.Config();
        }

        protected virtual void AddDynamicProcessorInstructions(
            IMutagenReadStream stream,
            byte numMasters,
            FormID formID,
            RecordType recType,
            BinaryFileProcessor.Config instr,
            RangeInt64 loc,
            RecordLocator.FileLocations fileLocs,
            Dictionary<long, uint> lengthTracker)
        {
        }

        protected virtual void PreProcessorJobs(
            IMutagenReadStream stream,
            RecordLocator.FileLocations fileLocs,
            BinaryFileProcessor.Config instructions,
            RecordLocator.FileLocations alignedFileLocs)
        {
        }

        public async Task<TempFolder> SetupProcessedFiles()
        {
            var tmp = new TempFolder(new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"Mutagen_Binary_Tests/{Nickname}")), deleteAfter: Settings.DeleteCachesAfter);

            var outputPath = ExportFileName(tmp);
            var observableOutputPath = ObservableExportFileName(tmp);
            var uncompressedPath = UncompressedFileName(tmp);
            var alignedPath = AlignedFileName(tmp);
            var orderedPath = OrderedFileName(tmp);
            var preprocessedPath = alignedPath;
            var processedPath = ProcessedPath(tmp);

            Mutagen.Bethesda.RecordInterest interest = null;
            if (this.Target.Interest != null)
            {
                interest = new Mutagen.Bethesda.RecordInterest(
                    this.Target.Interest.InterestingTypes
                        .Select(i => new RecordType(i)),
                    this.Target.Interest.UninterestingTypes
                        .Select(i => new RecordType(i)));
            }

            if (!Settings.ReuseCaches || !File.Exists(uncompressedPath))
            {
                try
                {
                    using (var outStream = new FileStream(uncompressedPath, FileMode.Create, FileAccess.Write))
                    {
                        ModDecompressor.Decompress(
                            streamCreator: () => File.OpenRead(this.FilePath.Path),
                            gameMode: this.GameMode,
                            outputStream: outStream,
                            interest: interest);
                    }
                }
                catch (Exception)
                {
                    if (File.Exists(uncompressedPath))
                    {
                        File.Delete(uncompressedPath);
                    }
                    throw;
                }
            }

            if (!Settings.ReuseCaches || !File.Exists(orderedPath))
            {
                try
                {
                    using (var outStream = new FileStream(orderedPath, FileMode.Create))
                    {
                        ModRecordSorter.Sort(
                            streamCreator: () => File.OpenRead(uncompressedPath),
                            outputStream: outStream,
                            gameMode: this.Target.GameMode);
                    }
                }
                catch (Exception)
                {
                    if (File.Exists(orderedPath))
                    {
                        File.Delete(orderedPath);
                    }
                    throw;
                }
            }

            if (!Settings.ReuseCaches || !File.Exists(alignedPath))
            {
                ModRecordAligner.Align(
                    inputPath: orderedPath,
                    outputPath: alignedPath,
                    gameMode: this.GameMode,
                    alignmentRules: GetAlignmentRules(),
                    temp: tmp);
            }

            BinaryFileProcessor.Config instructions;
            if (!Settings.ReuseCaches || !File.Exists(processedPath))
            {
                var alignedFileLocs = RecordLocator.GetFileLocations(preprocessedPath, this.GameMode);

                Dictionary<long, uint> lengthTracker = new Dictionary<long, uint>();

                using (var reader = new MutagenBinaryReadStream(preprocessedPath, this.GameMode))
                {
                    foreach (var grup in alignedFileLocs.GrupLocations.And(alignedFileLocs.ListedRecords.Keys))
                    {
                        reader.Position = grup + 4;
                        lengthTracker[grup] = reader.ReadUInt32();
                    }
                }

                instructions = GetInstructions(
                    lengthTracker,
                    alignedFileLocs);

                using (var stream = new MutagenBinaryReadStream(preprocessedPath, this.GameMode))
                {
                    var fileLocs = RecordLocator.GetFileLocations(this.FilePath.Path, this.GameMode);
                    PreProcessorJobs(
                        stream: stream,
                        fileLocs: fileLocs,
                        instructions: instructions,
                        alignedFileLocs: alignedFileLocs);
                    foreach (var rec in fileLocs.ListedRecords)
                    {
                        AddDynamicProcessorInstructions(
                            stream: stream,
                            formID: rec.Value.FormID,
                            recType: rec.Value.Record,
                            instr: instructions,
                            loc: alignedFileLocs[rec.Value.FormID],
                            fileLocs: alignedFileLocs,
                            lengthTracker: lengthTracker,
                            numMasters: this.NumMasters);
                    }
                }

                using (var reader = new MutagenBinaryReadStream(preprocessedPath, this.GameMode))
                {
                    foreach (var grup in lengthTracker)
                    {
                        reader.Position = grup.Key + 4;
                        if (grup.Value == reader.ReadUInt32()) continue;
                        instructions.SetSubstitution(
                            loc: grup.Key + 4,
                            sub: BitConverter.GetBytes(grup.Value));
                    }
                }

                using (var processor = new BinaryFileProcessor(
                    new FileStream(preprocessedPath, FileMode.Open, FileAccess.Read),
                    instructions))
                {
                    try
                    {
                        using (var outStream = new FileStream(processedPath, FileMode.Create, FileAccess.Write))
                        {
                            processor.CopyTo(outStream);
                        }
                    }
                    catch (Exception)
                    {
                        if (File.Exists(processedPath))
                        {
                            File.Delete(processedPath);
                        }
                        throw;
                    }
                }
            }

            return tmp;
        }

        protected abstract Task<IMod> ImportBinary(FilePath path, ModKey modKey);

        public async Task BinaryPassthroughTest()
        {
            using (var tmp = await SetupProcessedFiles())
            {
                var outputPath = Path.Combine(tmp.Dir.Path, $"{this.Nickname}_NormalExport");
                var processedPath = ProcessedPath(tmp);
                var orderedPath = Path.Combine(tmp.Dir.Path, $"{this.Nickname}_Ordered");
                var binaryWrapper = Path.Combine(tmp.Dir.Path, $"{this.Nickname}_BinaryWrapper");
                ModKey modKey = ModKey.Factory(this.FilePath.Name);

                // Do normal
                if (Settings.TestNormal)
                {
                    var mod = await ImportBinary(this.FilePath.Path, modKey);

                    foreach (var record in mod.MajorRecords.Items)
                    {
                        record.IsCompressed = false;
                    }
                    mod.WriteToBinary(
                        outputPath,
                        Mutagen.Bethesda.Oblivion.Constants.Oblivion);
                    GC.Collect();

                    using (var stream = new MutagenBinaryReadStream(processedPath, this.GameMode))
                    {
                        var ret = Passthrough_Tests.AssertFilesEqual(
                            stream,
                            outputPath,
                            amountToReport: 15);
                        if (ret.Exception != null)
                        {
                            throw ret.Exception;
                        }
                    }
                }

                if (Settings.TestBinaryWrapper)
                {
                    var bytes = File.ReadAllBytes(this.FilePath.Path);
                    var wrapper = OblivionModBinaryWrapper.OblivionModFactory(
                        new MemorySlice<byte>(bytes),
                        modKey);

                    wrapper.WriteToBinary(
                        binaryWrapper,
                        Mutagen.Bethesda.Oblivion.Constants.Oblivion);

                    using (var stream = new MutagenBinaryReadStream(processedPath, this.GameMode))
                    {
                        var ret = Passthrough_Tests.AssertFilesEqual(
                            stream,
                            binaryWrapper,
                            amountToReport: 15);
                        if (ret.Exception != null)
                        {
                            throw ret.Exception;
                        }
                    }
                }
            }
        }

        public async Task TestImport()
        {
            ModKey modKey = ModKey.Factory(this.FilePath.Name);
            await ImportBinary(this.FilePath.Path, modKey);
        }

        public static PassthroughTest Factory(TestingSettings settings, Target target)
        {
            switch (target.GameMode)
            {
                case GameMode.Oblivion:
                    return new Oblivion_Passthrough_Test(settings, target);
                case GameMode.Skyrim:
                    return new Skyrim_Passthrough_Test(settings, target);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
