// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Resources;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Baselines
{
    internal interface IBaselineSpec
    {
        BindingOption Binding { get; set; }

        ConfigurationOption Configuration { get; set; }

        ConventionOption Convention { get; set; }

        RuleOption Rule { get; set; }
    }

    [Spec(Specs.V1, Specs.Baseline)]
    public sealed class Baseline : InternalResource<BaselineSpec>, IResource
    {
        public Baseline(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, BaselineSpec spec)
            : base(ResourceKind.Baseline, apiVersion, source, metadata, info, extent, spec) { }

        [YamlIgnore()]
        public string BaselineId => Name;

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public string Synopsis => Info.Synopsis.Text;
    }

    public sealed class BaselineSpec : Spec, IBaselineSpec
    {
        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        public ConventionOption Convention { get; set; }

        public RuleOption Rule { get; set; }
    }

    internal sealed class BaselineFilter : IResourceFilter
    {
        private readonly HashSet<string> _Include;
        private readonly WildcardPattern _WildcardMatch;

        public BaselineFilter(string[] include)
        {
            _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
            _WildcardMatch = null;
            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                _WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        ResourceKind IResourceFilter.Kind => ResourceKind.Baseline;

        public bool Match(IResource resource)
        {
            return _Include == null || _Include.Contains(resource.Name) || MatchWildcard(resource.Name);
        }

        private bool MatchWildcard(string name)
        {
            return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
        }
    }

    internal sealed class BaselineRef : ResourceRef
    {
        public readonly OptionContext.ScopeType Type;

        public BaselineRef(string id, OptionContext.ScopeType scopeType)
            : base(id, ResourceKind.Baseline)
        {
            Type = scopeType;
        }
    }
}
