﻿using System;

namespace Mutagen.Bethesda.Skyrim
{
    namespace Internals
    {
        public partial class PlacedTrapBinaryOverlay
        {
            public FormLink<IProjectileGetter> Projectile { get; internal set; } = FormLink<IProjectileGetter>.Null;
        }
    }
}