using System;
using System.Reflection;
using System.Threading.Tasks;

namespace HatTrick.CommandLine.Extensions
{
    public static class CommandDefinitionExtensions
    {
        #region map to
        public static IContinuation<T> MapTo<T>(this CommandDefinition cmdDef) where T : new()
        {
            return new MapOf<T>(cmdDef);
        }

        public static IContinuation<T> MapTo<T>(this CommandDefinition cmdDef, params (string optionKey, string propertyName)[] correlations) where T : new()
        {
            return new MapOf<T>(cmdDef, correlations);
        }
        #endregion

        #region map to signature
        public static ISignatureContinuation<T> MapToSignature<T>(this CommandDefinition cmdDef) where T : Delegate
        {
            return new SignatureMapOf<T>(cmdDef);
        }

        public static ISignatureContinuation<T> MapToSignature<T>(this CommandDefinition cmdDef, params (string optionKey, string parameterName)[] correlations) where T : Delegate
        {
            return new SignatureMapOf<T>(cmdDef, correlations);
        }
        #endregion

        #region i continuation of T
        public interface IContinuation<T>
        {
            public void Then(Action<T> handler);
            public void ThenAsync(Func<T, Task> handler);
        }
        #endregion

        #region i signature continuation of T
        public interface ISignatureContinuation<T>
        {
            public void Then(T target);
            public void ThenAsync(T target);
        }
        #endregion

        #region mapper [class]
        public abstract class Mapper
        {
            #region internals
            private CommandDefinition _cmdDef;
            private (string optionKey, string to)[] _correlations;
            #endregion

            #region const
            public const string IgnoreMapToken = "~";
            #endregion

            #region interface
            internal CommandDefinition CommandDefinition => _cmdDef;

            protected (string optionKey, string to)[] Correlations => _correlations;
            #endregion

            #region constructors
            public Mapper(CommandDefinition cmdDef) : this(cmdDef, null)
            {
            }

            public Mapper(CommandDefinition cmdDef, (string optionKey, string to)[] correlations)
            {
                _cmdDef = cmdDef ?? throw new ArgumentNullException(nameof(cmdDef));
                _correlations = correlations;
                this.RegisterValidator(this.Validate);
            }
            #endregion

            #region register validator
            protected void RegisterValidator(Action validator)
            {
                _cmdDef.RegisterValidator(validator);
            }
            #endregion

            #region validate
            private void Validate()
            {
                this.EnsureCorrelations();
            }
            #endregion

            #region correlation exists for option key
            public bool CorrelationExistsForOptionKey(string optionKey, out (string optionKey, string to) correlation)
            {
                correlation = _correlations is null || _correlations.Length == 0
                    ? default
                    : Array.Find(_correlations, (c) => c.optionKey == optionKey);

                return correlation != default;
            }
            #endregion

            #region correlation exists for map target
            public bool CorrelationExistsForMapTarget(string to, out (string optionKey, string to) correlation)
            {
                correlation = _correlations is null || _correlations.Length == 0
                    ? default
                    : Array.Find(_correlations, (c) => c.to == to);

                return correlation != default;
            }
            #endregion

            #region ensure correlations
            protected void EnsureCorrelations()
            {
                var correlations = _correlations;

                if (correlations is null || correlations.Length == 0)
                    return;

                var cmdDef = _cmdDef;

                foreach (var c in correlations)
                {
                    if (!cmdDef.OptionExists(c.optionKey))
                        throw new CommandMappingException($"Command '{cmdDef.Name} does not contain an option key that matches provided correlation: {c}");
                }
            }
            #endregion
        }
        #endregion

        #region map of [class]
        public class MapOf<T> : Mapper, IContinuation<T> where T : new()
        {
            #region constructors
            public MapOf(CommandDefinition cmdDef) : this(cmdDef, null)
            {
            }

            public MapOf(CommandDefinition cmdDef, (string optionKey, string propertyName)[] correlations) : base(cmdDef, correlations)
            {
                base.RegisterValidator(this.Validate);
            }
            #endregion

            #region validate
            public void Validate()
            {
                SetOf<OptionDefinition> options = base.CommandDefinition.Options;

                Type t = typeof(T);
                PropertyInfo[] props = t.GetProperties();

                for (int i = 0; i < options.Length; i++)
                {
                    var op = options[i];
                    bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                    string propName = isCorrelated ? correlation.property : op.Key;

                    //if correlation map specifies the 'ignore map' token for the property, it means DO NOT attempt to map.
                    if (propName == Mapper.IgnoreMapToken)
                        continue;

                    var prop = Array.Find(props, (p) => p.Name == propName);

                    if (prop is null)
                    {
                        string message = $"No property found on '{t.Name}' for option '{op.Key}'";

                        if (isCorrelated)
                            message += " via correlation " + correlation;

                        throw new CommandMappingException(message);
                    }

                    Type valType = op.GenericType;

                    if (!prop.PropertyType.IsAssignableFrom(valType))
                    {
                        var opTypeName = OptionTypeMap.GetAliasOrName(op.GenericType);
                        var propTypeName = OptionTypeMap.GetAliasOrName(prop.PropertyType);
                        var msg = $"Type mismatch while mapping option value to target...Option key: {op.Key}...Option type: {opTypeName}...Property type: {propTypeName}";
                        throw new CommandMappingException(msg);
                    }
                }
            }
            #endregion

            #region map
            public void Map(Command command, out T to)
            {
                to = new();

                Option[] options = command.GetOptions();

                Type t = typeof(T);
                PropertyInfo[] props = t.GetProperties();

                foreach (var op in options)
                {
                    bool isCorrelated = CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                    string propName = isCorrelated ? correlation.property : op.Key;

                    //if correlation map specifies the 'ignore map' token for the property, it means DO NOT attempt to map.
                    if (propName == Mapper.IgnoreMapToken)
                        continue;

                    var prop = Array.Find(props, (p) => p.Name == propName);

                    var val = op.GetValue<object>();

                    if (val is not null)
                        prop.SetValue(to, val);
                }
            }
            #endregion

            #region then
            public void Then(Action<T> handler)
            {
                Action<Command> action = (cmd) =>
                {
                    this.Map(cmd, out T instance);
                    handler(instance);
                };
                base.CommandDefinition.Handler += action;
            }
            #endregion

            #region then async
            public void ThenAsync(Func<T, Task> handler)
            {
                Func<Command, Task> function = async (cmd) =>
                {
                    this.Map(cmd, out T instance);
                    await handler(instance);
                };
                base.CommandDefinition.AsyncHandler = function;
            }
            #endregion
        }
        #endregion

        #region signature map of [class]
        public class SignatureMapOf<T> : Mapper, ISignatureContinuation<T> where T : Delegate
        {
            #region internals
            private T _target;
            #endregion

            #region constructors
            public SignatureMapOf(CommandDefinition cmdDef) : this(cmdDef, null)
            {
            }

            public SignatureMapOf(CommandDefinition cmdDef, (string optionKey, string parameterName)[] correlations) : base(cmdDef, correlations)
            {
                base.RegisterValidator(this.Validate);
            }
            #endregion

            #region set target
            internal void SetTarget(T target)
            {
                _target = target;
            }
            #endregion

            #region get target
            internal T GetTarget()
            {
                return _target;
            }
            #endregion

            #region validate
            //Yikes, this whole thing def works, but it's getting wild.
            public void Validate()
            {
                SetOf<OptionDefinition> options = base.CommandDefinition.Options;
                bool[] utilized = new bool[options.Length];

                ParameterInfo[] pInfos = _target.Method.GetParameters();

                for (int i = 0; i < pInfos.Length; i++)
                {
                    var parameter = pInfos[i];
                    bool isCorrelated = base.CorrelationExistsForMapTarget(parameter.Name, out (string opKey, string to) correlation);
                    string opKey = isCorrelated ? correlation.opKey : parameter.Name;

                    int idx = options.FindIndex(o => o.Key == opKey);
                    if (idx == -1)
                    {
                        string msg = $"No option definition found for signature parameter '{parameter.Name}'";

                        if (isCorrelated)
                            msg += " via correlation " + correlation;

                        throw new CommandMappingException(msg);
                    }

                    var op = options[idx];

                    utilized[idx] = true;

                    Type valType = op.GenericType;

                    if (!parameter.ParameterType.IsAssignableFrom(valType))
                    {
                        var opTypeName = OptionTypeMap.GetAliasOrName(op.GenericType);
                        var paramTypeName = OptionTypeMap.GetAliasOrName(parameter.ParameterType);
                        var msg = $"Type mismatch while mapping option value to parameter...Option key: {opKey}...Option type: {opTypeName}...Parameter type: {paramTypeName}";
                        throw new CommandMappingException(msg);
                    }
                }

                //TODO: Move this to a ThrowOn.... type of method
                for (int i = 0; i < utilized.Length; i++)
                {
                    if (utilized[i] == false)
                    {
                        var op = options[i];
                        bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string to) correlation);

                        if (isCorrelated && correlation.to == Mapper.IgnoreMapToken)
                            continue;

                        string msg = $"No parameter found on signature for option '{op.Key}'";

                        if (isCorrelated)
                            msg += " via correlation " + correlation;

                        throw new CommandMappingException(msg);
                    }
                }
            }
            #endregion

            #region map
            public void Map(Command command, out object[] parameters)
            {
                Option[] options = command.GetOptions();

                ParameterInfo[] pInfos = _target.Method.GetParameters();

                parameters = new object[pInfos.Length];

                //NOTE: parameters must be in the EXACT order as signature def... 
                //we must work on the set from the order of the parameter infos
                //we also need not be concerned about Ignored ops as we are mapping in the direction param -> op
                for (int i = 0; i < pInfos.Length; i++)
                {
                    var parameter = pInfos[i];

                    bool isCorrelated = CorrelationExistsForMapTarget(parameter.Name, out (string opKey, string parameterName) correlation);
                    string opKey = isCorrelated ? correlation.opKey : parameter.Name;

                    parameters[i] = command[opKey].GetValue<object>();
                }
            }
            #endregion

            #region then
            public void Then(T target)
            {
                this.SetTarget(target);

                Action<Command> action = (cmd) =>
                {
                    this.Map(cmd, out object[] parameters);
                    this.GetTarget().DynamicInvoke(parameters);
                };
                base.CommandDefinition.Handler += action;
            }
            #endregion

            #region then async
            public void ThenAsync(T target)
            {
                this.SetTarget(target);

                Func<Command, Task> function = async (cmd) =>
                {
                    this.Map(cmd, out object[] parameters);
                    await (Task)this.GetTarget().DynamicInvoke(parameters);
                };
                base.CommandDefinition.AsyncHandler += function;
            }
            #endregion
        }
        #endregion
    }
}