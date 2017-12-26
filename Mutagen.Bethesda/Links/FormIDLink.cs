﻿using Loqui;
using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda
{
    public class FormIDLink<T> : NotifyingItem<T>, IFormIDLink<T>, IEquatable<FormIDLink<T>>
       where T : MajorRecord
    {
        public bool Linked => this.Item != null;
        public RawFormID? UnlinkedForm { get; private set; }
        public RawFormID FormID => LinkExt.GetFormID(this);

        public FormIDLink()
        {
            this.Subscribe(UpdateUnlinkedForm);
        }

        public FormIDLink(RawFormID unlinkedForm)
        {
            this.UnlinkedForm = unlinkedForm;
            this.Subscribe(UpdateUnlinkedForm);
        }

        private void UpdateUnlinkedForm(Change<T> change)
        {
            this.UnlinkedForm = change.New?.FormID ?? UnlinkedForm;
        }

        public void SetIfSucceeded(TryGet<RawFormID> formID)
        {
            if (formID.Failed) return;
            this.UnlinkedForm = formID.Value;
        }

        public static bool operator ==(FormIDLink<T> lhs, FormIDLink<T> rhs)
        {
            return lhs.FormID.Equals(rhs.FormID);
        }

        public static bool operator !=(FormIDLink<T> lhs, FormIDLink<T> rhs)
        {
            return !lhs.FormID.Equals(rhs.FormID);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FormIDLink<T> rhs)) return false;
            return this.Equals(rhs);
        }

        public bool Equals(FormIDLink<T> other) => LinkExt.Equals(this, other);

        public override int GetHashCode() => LinkExt.HashCode(this);

        public override string ToString() => LinkExt.ToString(this);
    }
}