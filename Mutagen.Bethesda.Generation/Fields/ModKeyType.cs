﻿using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation
{
    public class ModKeyType : PrimitiveType
    {
        public override Type Type => typeof(ModKey);
    }
}