using System;
using System.Reflection;

namespace Crypto.CommandLine
{
    public class Mapper
    {
        #region map command
        public static Continuation MapCommand(CommandDefinition commandDef)
        {
            return new Continuation(commandDef);
        }
        #endregion

        #region to
        //public static Continuation<T> To<T>() where T : new()
        //{
        //    return new Continuation<T>(new MapOf<T>());
        //}

        //public static Continuation<T> To<T>(params (string optionKey, string propertyName)[] correlationMap) where T : new()
        //{
        //    return new Continuation<T>(new MapOf<T>(correlationMap));
        //}
        #endregion

        #region to signature
        //public static SignatureContinuation<T> ToSignature<T>() where T : Delegate
        //{
        //    return new SignatureContinuation<T>(new SignatureMapOf<T>());
        //}

        //public static SignatureContinuation<T> ToSignature<T>(params (string optionKey, string parameterName)[] correlationMap) where T : Delegate
        //{
        //    return new SignatureContinuation<T>(new SignatureMapOf<T>(correlationMap));
        //}
        #endregion

        #region continuation [class]
        public class Continuation
        {
            #region internals
            private CommandDefinition _cmdDef;
            #endregion

            #region constructors
            internal Continuation(CommandDefinition commandDef)
            {
                _cmdDef = commandDef;
            }
            #endregion

            #region to
            public Continuation<T> To<T>() where T : new()
            {
                return new Continuation<T>(new MapOf<T>(_cmdDef));
            }

            public Continuation<T> To<T>(params (string optionKey, string propertyName)[] correlationMap) where T : new()
            {
                return new Continuation<T>(new MapOf<T>(_cmdDef, correlationMap));
            }
            #endregion

            #region to signature
            public SignatureContinuation<T> ToSignature<T>() where T : Delegate
            {
                return new SignatureContinuation<T>(new SignatureMapOf<T>(_cmdDef));
            }

            public SignatureContinuation<T> ToSignature<T>(params (string optionKey, string parameterName)[] correlationMap) where T : Delegate
            {
                return new SignatureContinuation<T>(new SignatureMapOf<T>(_cmdDef, correlationMap));
            }
            #endregion
        }
        #endregion

        #region continuation of T [class]
        public class Continuation<T> where T : new()
        {
            #region internals
            private MapOf<T> _mapper;
            #endregion

            #region constructors
            public Continuation(MapOf<T> mapper)
            {
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }
            #endregion

            #region then
            public Action<Command> Then(Action<T> handler)
            {
                Action<Command> action = (cmd) =>
                {
                    _mapper.Map(cmd, out T instance);
                    handler(instance);
                };

                return action;
            }
            #endregion
        }
        #endregion

        #region signature continuation of T [class]
        public class SignatureContinuation<T> where T : Delegate
        {
            #region internals
            private SignatureMapOf<T> _mapper;
            #endregion

            #region constructors
            public SignatureContinuation(SignatureMapOf<T> mapper)
            {
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }
            #endregion

            #region then
            public Action<Command> Then(T target)
            {
                _mapper.SetTarget(target);

                Action<Command> action = (cmd) =>
                {
                    _mapper.Map(cmd, out object[] parameters);
                    target.DynamicInvoke(parameters);
                };
                return action;
            }
            #endregion
        }
        #endregion
    }
}
