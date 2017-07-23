using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class ActivationState
    {
        private int _Value;

        public bool IsActive
        {
            get
            {
                return _Value == 1;
            }
            set
            {
                _Value = Interlocked.Exchange(ref _Value, value ? 1 : 0);
            }
        }

        public ActivationState(bool initialValue = false)
        {
            _Value = initialValue ? 1 : 0;
        }

        public bool TransitionToActive() => Interlocked.CompareExchange(ref _Value, 1, 0) == 0;

        public bool TransitionToInactive() => Interlocked.CompareExchange(ref _Value, 0, 1) == 1;

        public static implicit operator bool(ActivationState state) => state._Value == 1;

        public static implicit operator ActivationState(bool isActive) => new ActivationState(isActive);

        public override bool Equals(object obj)
        {
            if (obj is bool)
            {
                return this.Equals((bool)obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public bool Equals(bool isActive) => (isActive ? 1 : 0) == _Value;

        public static bool operator ==(ActivationState a, bool b) => (a._Value == 1) == b;

        public static bool operator !=(ActivationState a, bool b) => (a._Value == 1) != b;

        public static bool operator ==(bool a, ActivationState b) => a == (b._Value == 1);

        public static bool operator !=(bool a, ActivationState b) => a != (b._Value == 1);
    }
}
