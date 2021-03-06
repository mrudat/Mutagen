/*
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Autogenerated by Loqui.  Do not manually change.
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
*/
using System;
using System.Collections.Generic;
using Mutagen.Bethesda.Core;

namespace Mutagen.Bethesda.Fallout4.Internals
{
    public class LinkInterfaceMapping : ILinkInterfaceMapping
    {
        public IReadOnlyDictionary<Type, Type[]> InterfaceToObjectTypes { get; }

        public GameCategory GameCategory => GameCategory.Fallout4;

        public LinkInterfaceMapping()
        {
            var dict = new Dictionary<Type, Type[]>();
            dict[typeof(IKeywordLinkedReference)] = new Type[]
            {
                typeof(Keyword),
            };
            dict[typeof(IKeywordLinkedReferenceGetter)] = dict[typeof(IKeywordLinkedReference)];
            InterfaceToObjectTypes = dict;
        }
    }
}

