using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mutagen.Bethesda.Skyrim
{
    public partial class MiscItem
    {
        #region Interfaces
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IModelGetter? IModeledGetter.Model => this.Model;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IIconsGetter? IHasIconsGetter.Icons => this.Icons;
        #endregion

        [Flags]
        public enum MajorFlag
        {
            NonPlayable = 0x4
        }
    }
}
