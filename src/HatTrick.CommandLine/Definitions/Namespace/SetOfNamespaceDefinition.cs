// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

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

            this.EnsureAncestors(namespaceDef.Name);

            //promote a synthetic placeholder to this real definition; a real duplicate is an error.
            if (this.TryGet(namespaceDef.Name, out NamespaceDefinition existing))
            {
                if (existing.Synthetic)
                {
                    existing.Promote(namespaceDef.Help);
                    return;
                }

                throw new NamespaceDefinitionException($"Cannot insert namespace, duplicate key found: '{namespaceDef.Name}'");
            }

            base.Add(namespaceDef);
        }
        #endregion

        #region ensure ancestors
        //auto-vivify: create any missing ancestor namespaces as synthetic placeholders so the
        //namespace tree is always connected (gap-free registration is no longer required).
        private void EnsureAncestors(string name)
        {
            if (!name.Contains('.'))
                return;

            string[] segments = name.Split('.');
            string prefix = null;
            for (int i = 0; i < (segments.Length - 1); i++)
            {
                prefix = (i > 0)
                    ? string.Concat(prefix, '.', segments[i])
                    : segments[i];

                if (!this.ContainsName(prefix))
                    base.Add(NamespaceDefinition.CreateSynthetic(prefix));
            }
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
                bool isChild = ns.Depth == atDepth && ns.Name.StartsWith(parent.Name + ".");
                return includeHidden 
                    ? isChild 
                    : (isChild && !ns.Hidden);
            });

            return children;
        }
        #endregion

        #region get descendants
        internal NamespaceDefinition[] GetDescendants(NamespaceDefinition parent, bool includeHidden = false)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = base.FindAll((ns) =>
            {
                bool isDescendent = ns.Depth > parent.Depth && ns.Name.StartsWith(parent.Name + ".");
                return includeHidden
                    ? isDescendent
                    : (isDescendent && !ns.Hidden);
            });

            return descendents;
        }
        #endregion

        #region get ancestors
        internal NamespaceDefinition[] GetAncestors(CommandDefinition descendant, bool includeHidden = false)
        {
            if (descendant is null)
                throw new ArgumentNullException(nameof(descendant));

            var ancestors = base.FindAll((ns) =>
            {
                bool isAncestor = ns.Depth < descendant.Depth && descendant.Name.StartsWith(ns.Name + ".");
                return includeHidden
                    ? isAncestor
                    : (isAncestor && !ns.Hidden);
            });

            return ancestors;
        }
        #endregion
    }
}
