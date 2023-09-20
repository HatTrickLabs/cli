namespace HatTrick.CommandLine
{
    public class DefaultOption : Option
    {
        public bool IsDefault => true;

        public DefaultOption(string key, string flag) : base(key, flag)
        { }
    }
}
