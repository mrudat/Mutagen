using Loqui;
using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation
{
    public class NothingBinaryTranslationGeneration : BinaryTranslationGeneration
    {
        public override async Task<int?> ExpectedLength(ObjectGeneration objGen, TypeGeneration typeGen)
        {
            return 0;
        }

        public override async Task GenerateCopyIn(
            FileGeneration fg, 
            ObjectGeneration objGen, 
            TypeGeneration typeGen,
            Accessor readerAccessor, 
            Accessor itemAccessor, 
            Accessor errorMaskAccessor,
            Accessor translationAccessor)
        {
        }

        public override void GenerateCopyInRet(
            FileGeneration fg, 
            ObjectGeneration objGen, 
            TypeGeneration targetGen, 
            TypeGeneration typeGen, 
            Accessor readerAccessor, 
            AsyncMode asyncMode, 
            Accessor retAccessor, 
            Accessor outItemAccessor, 
            Accessor errorMaskAccessor, 
            Accessor translationAccessor,
            Accessor converterAccessor,
            bool inline)
        {
        }

        public override async Task GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen, 
            Accessor writerAccessor, 
            Accessor itemAccessor, 
            Accessor errorMaskAccessor, 
            Accessor translationAccessor,
            Accessor converterAccessor)
        {
        }

        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            throw new NotImplementedException();
        }
    }
}
