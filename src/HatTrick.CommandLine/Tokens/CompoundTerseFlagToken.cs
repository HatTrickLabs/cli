using System;

namespace HatTrick.CommandLine
{
    public class CompoundTerseFlagToken : FlagToken
    {
        #region constructors
        internal CompoundTerseFlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!FlagToken.IsValid(value))
                return false;

            if (value.Length < 3)
                return false;

            if (value[1] == '-')
                return false;

            for (int i = 1; i < value.Length; i++)
            {
                char c = value[i];

                if (!char.IsLetter(c))
                    return false;
            }

            return true;
        }
        #endregion

        #region unroll
        internal TerseFlagToken[] Unroll()
        {
            SetOf<TerseFlagToken> flags = new SetOf<TerseFlagToken>();
            string compound = base.Value;
            for (int i = 1; i < compound.Length; i++)
            {
                flags.Add(new TerseFlagToken("-" + compound[i]));
            }
            return flags.ToArray();
        }
        #endregion
    }
}
