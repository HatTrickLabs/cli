namespace HatTrick.CommandLine
{
    public class EmptyOption : Option
    {
        public bool IsEmpty => true;

        public EmptyOption(string key, string flag) : base(key, flag)
        { }
    }
}
