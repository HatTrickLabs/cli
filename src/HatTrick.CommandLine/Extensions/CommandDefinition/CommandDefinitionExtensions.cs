using System;
using System.Reflection;
using System.Threading.Tasks;

namespace HatTrick.CommandLine.Extensions
{
    public static class CommandDefinitionExtensions
    {
        #region map to
        public static Continuation<T> MapTo<T>(this CommandDefinition commandDefinition) where T : new()
        {
            return new Continuation<T>(new MapOf<T>(commandDefinition));
        }

        public static Continuation<T> MapTo<T>(this CommandDefinition commandDefinition, params (string optionKey, string propertyName)[] correlationMap) where T : new()
        {
            return new Continuation<T>(new MapOf<T>(commandDefinition, correlationMap));
        }
        #endregion

        #region map to signature
        public static SignatureContinuation<T> MapToSignature<T>(this CommandDefinition commandDefinition) where T : Delegate
        {
            return new SignatureContinuation<T>(new SignatureMapOf<T>(commandDefinition));
        }

        public static SignatureContinuation<T> MapToSignature<T>(this CommandDefinition commandDefinition, params (string optionKey, string parameterName)[] correlationMap) where T : Delegate
        {
            return new SignatureContinuation<T>(new SignatureMapOf<T>(commandDefinition, correlationMap));
        }
        #endregion

        #region continuation of T [class]
        public class Continuation<T> where T : new()
        {
            #region internals
            private MapOf<T> _mapOf;
            #endregion

            #region constructors
            public Continuation(MapOf<T> mapOf)
            {
                _mapOf = mapOf ?? throw new ArgumentNullException(nameof(mapOf));
            }
            #endregion

            #region then
            public void Then(Action<T> handler)
            {
                Action<Command> action = (cmd) =>
                {
                    _mapOf.Map(cmd, out T instance);
                    handler(instance);
                };
                _mapOf.CommandDefinition.Handler += action;
            }
            #endregion

            #region then async
            public void ThenAsync(Func<T, Task> handler)
            {
                Func<Command, Task> function = async (cmd) =>
                {
                    _mapOf.Map(cmd, out T instance);
                    await handler(instance);
                };
                _mapOf.CommandDefinition.AsyncHandler = function;
            }
            #endregion
        }
        #endregion

        #region signature continuation of T [class]
        public class SignatureContinuation<T> where T : Delegate
        {
            #region internals
            private SignatureMapOf<T> _signatureMapOf;
            #endregion

            #region constructors
            public SignatureContinuation(SignatureMapOf<T> signatureMapOf)
            {
                _signatureMapOf = signatureMapOf ?? throw new ArgumentNullException(nameof(signatureMapOf));
            }
            #endregion

            #region then
            public void Then(T target)
            {
                _signatureMapOf.SetTarget(target);

                Action<Command> action = (cmd) =>
                {
                    _signatureMapOf.Map(cmd, out object[] parameters);
                    target.DynamicInvoke(parameters);
                };
                _signatureMapOf.CommandDefinition.Handler += action;
            }
            #endregion

            #region then async
            public void ThenAsync(T target)
            {
                _signatureMapOf.SetTarget(target);

                Func<Command, Task> function = async (cmd) =>
                {
                    _signatureMapOf.Map(cmd, out object[] parameters);
                    await (Task)_signatureMapOf.Target.DynamicInvoke(parameters);
                };
                _signatureMapOf.CommandDefinition.AsyncHandler += function;
            }
            #endregion
        }
        #endregion

        #region map [class]
        public abstract class Map
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
            public Map(CommandDefinition commandDef) : this(commandDef, null)
            {
            }

            public Map(CommandDefinition commandDef, (string optionKey, string to)[] correlations)
            {
                _cmdDef = commandDef ?? throw new ArgumentNullException(nameof(commandDef));
                _correlations = correlations;
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
        public class MapOf<T> : Map where T : new()
        {
            #region constructors
            public MapOf(CommandDefinition commandDef) : this(commandDef, null)
            {
            }

            public MapOf(CommandDefinition commandDef, (string optionKey, string propertyName)[] correlations) : base(commandDef, correlations)
            {
                base.RegisterValidator(this.Validate);
            }
            #endregion

            #region validate
            public void Validate()
            {
                SetOf<OptionDefinition> options = CommandDefinition.Options;

                Type t = typeof(T);
                PropertyInfo[] props = t.GetProperties();

                for (int i = 0; i < options.Length; i++)
                {
                    var op = options[i];
                    bool isCorrelated = CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                    string propName = isCorrelated ? correlation.property : op.Key;

                    //if correlation map specifies the 'ignore map' token for the property, it means DO NOT attempt to map.
                    if (propName == MapOf<T>.IgnoreMapToken)
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
                    if (propName == MapOf<T>.IgnoreMapToken)
                        continue;

                    var prop = Array.Find(props, (p) => p.Name == propName);

                    var val = op.GetValue<object>();

                    if (val is not null)
                        prop.SetValue(to, val);
                }
            }
            #endregion
        }
        #endregion

        #region signature map of [class]
        public class SignatureMapOf<T> : Map where T : Delegate
        {
            #region internals
            private T _target;
            #endregion

            #region interface
            internal T Target => _target;
            #endregion

            #region constructors
            public SignatureMapOf(CommandDefinition commandDef) : this(commandDef, null)
            {
            }

            public SignatureMapOf(CommandDefinition commandDef, (string optionKey, string parameterName)[] correlations) : base(commandDef, correlations)
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

                        if (isCorrelated && correlation.to == SignatureMapOf<T>.IgnoreMapToken)
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
        }
        #endregion
    }
}