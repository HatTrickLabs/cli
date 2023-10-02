using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HatTrick.CommandLine
{
    internal class SetOfNamespaceDefinition : SetOf<NamespaceDefinition>
    {
        #region interface
        public NamespaceDefinition this[string name]
        {
            get
            {
                if (name is null)
                    throw new ArgumentNullException(nameof(name));

                if (base.Length == 0)
                    throw new KeyNotFoundException($"No '{nameof(NamespaceDefinition)}' found for provided {nameof(name)} '{name}'");

                int i = base.FindIndex((ns) => string.Compare(ns.Name, name, false) == 0);

                if (i == -1)
                    throw new KeyNotFoundException($"No '{nameof(NamespaceDefinition)}' found for provided {nameof(name)} '{name}'");

                return base[i];
            }
        }
        #endregion

        #region constructors
        public SetOfNamespaceDefinition() : base()
        { }

        public SetOfNamespaceDefinition(int minimumCapacity) : base(minimumCapacity)
        { }
        #endregion

        #region contains name
        public bool ContainsName(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (base.Length == 0)
                return false;

            int i = base.FindIndex((ns) => string.Compare(ns.Name, name, false) == 0);

            return i > -1;
        }
        #endregion

        #region add
        public override void Add(NamespaceDefinition namespaceDef)
        {
            if (namespaceDef is null)
                throw new ArgumentNullException(nameof(namespaceDef));

            namespaceDef.Validate();

            this.ThrowOnSegmentGap(namespaceDef.Name);
            this.ThrowOnDuplicateName(namespaceDef.Name);

            base.Add(namespaceDef);
        }
        #endregion

        #region throw on segment gap
        private void ThrowOnSegmentGap(string name)
        {
            if (name.Contains('.'))
            {
                //ensure no segment gaps
                string[] segments = name.Split('.');
                string segment = null;
                for (int i = 0; i < (segments.Length - 1); i++)
                {
                    segment = (i > 0)
                        ? string.Concat(segment, '.', segments[i])
                        : segments[i];

                    if (!this.ContainsName(segment))
                    {
                        string msg = $"Namespace cannot be added for '{name}'...no parent namespace for '{segment}' exists.";
                        throw new NamespaceDefinitionException(msg);
                    }
                }
            }
        }
        #endregion

        #region throw on duplicate name
        private void ThrowOnDuplicateName(string name)
        {
            if (this.ContainsName(name))
                throw new NamespaceDefinitionException($"Cannot insert namespace, duplicate key found: '{name}'");
        }
        #endregion

        #region try get
        public bool TryGet(string name, out NamespaceDefinition namespaceDef)
        {
            namespaceDef = base.Find((ns) => string.Compare(ns.Name, name, false) == 0);
            return namespaceDef is not null; ;
        }
        #endregion

        #region get roots
        internal NamespaceDefinition[] GetRoots(bool includeHidden = false)
        {
            var roots = base.FindAll((ns) =>
            {
                bool isRoot = ns.Depth == 0;
                return includeHidden ? isRoot : (isRoot && !ns.Hidden);
            });

            return roots;
        }
        #endregion

        #region get children
        internal NamespaceDefinition[] GetChildren(NamespaceDefinition parent, bool includeHidden = false)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            int atDepth = parent.Depth + 1;
            var children = base.FindAll((ns) =>
            {
                bool isChild = ns.Depth == atDepth && ns.Name.StartsWith(parent.Name);
                return includeHidden 
                    ? isChild 
                    : (isChild && !ns.Hidden);
            });

            return children;
        }
        #endregion

        #region get descendents
        internal NamespaceDefinition[] GetDescendents(NamespaceDefinition parent, bool includeHidden = false)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = base.FindAll((ns) =>
            {
                bool isDescendent = ns.Depth > parent.Depth && ns.Name.StartsWith(parent.Name);
                return includeHidden
                    ? isDescendent
                    : (isDescendent && !ns.Hidden);
            });

            return descendents;
        }
        #endregion
    }
}
