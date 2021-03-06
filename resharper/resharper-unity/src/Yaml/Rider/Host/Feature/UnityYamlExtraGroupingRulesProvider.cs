using JetBrains.Annotations;
using JetBrains.Platform.RdFramework.Util;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Tree;
using JetBrains.ReSharper.Host.Features.Usages;
using JetBrains.ReSharper.Host.Platform.Icons;
using JetBrains.ReSharper.Plugins.Unity.Resources.Icons;
using JetBrains.ReSharper.Plugins.Unity.Rider;
using JetBrains.ReSharper.Plugins.Unity.Yaml.Psi;
using JetBrains.ReSharper.Plugins.Unity.Yaml.Psi.Resolve;
using JetBrains.Rider.Model;
using JetBrains.UI.Icons;

namespace JetBrains.ReSharper.Plugins.Unity.Yaml.Host.Feature
{
    [SolutionComponent]
    public class UnityYamlExtraGroupingRulesProvider : IRiderExtraGroupingRulesProvider
    {
        // IconHost is optional so that we don't fail if we're in tests
        public UnityYamlExtraGroupingRulesProvider(UnitySceneProcessor sceneProcessor = null, UnitySolutionTracker unitySolutionTracker = null, IconHost iconHost = null)
        {
            if (unitySolutionTracker != null && unitySolutionTracker.IsUnityProject.HasValue() && unitySolutionTracker.IsUnityProject.Value
                && iconHost != null && sceneProcessor != null)
            {
                ExtraRules = new IRiderUsageGroupingRule[]
                {
                    new GameObjectUsageGroupingRule(sceneProcessor, iconHost),
                    new ComponentUsageGroupingRule(iconHost)
                };
            }
            else
            {
                ExtraRules = new IRiderUsageGroupingRule[0];
            }
        }

        public IRiderUsageGroupingRule[] ExtraRules { get; }
    }

    public abstract class UnityYamlUsageGroupingRuleBase : IRiderUsageGroupingRule
    {
        [CanBeNull] private readonly IconHost myIconHost;

        protected UnityYamlUsageGroupingRuleBase(string name, IconId iconId, [CanBeNull] IconHost iconHost,
            double sortingPriority)
        {
            Name = name;
            IconId = iconId;
            SortingPriority = sortingPriority;
            myIconHost = iconHost;
        }

        protected RdUsageGroup CreateModel(string text)
        {
            return new RdUsageGroup(Name, text, myIconHost?.Transform(IconId));
        }

        protected RdUsageGroup EmptyModel()
        {
            return new RdUsageGroup(Name, string.Empty, null);
        }

        public abstract RdUsageGroup CreateModel(IOccurrence occurrence, IOccurrenceBrowserDescriptor descriptor);
        public abstract void Navigate(IOccurrence occurrence);

        public string Name { get; }
        public IconId IconId { get; }
        public bool IsSeparable => true;
        public abstract bool IsNavigateable { get; }
        public bool Configurable => true;
        public bool PriorityDependsOnUsages => true;
        public double SortingPriority { get; }
    }

    // The priorities here put us after directory, file, namespace, type and member
    public class GameObjectUsageGroupingRule : UnityYamlUsageGroupingRuleBase
    {
        [NotNull] private readonly UnitySceneProcessor mySceneProcessor;

        public GameObjectUsageGroupingRule([NotNull] UnitySceneProcessor sceneProcessor, [NotNull] IconHost iconHost)
            : base("Unity Game Object", UnityObjectTypeThemedIcons.UnityGameObject.Id, iconHost, 7.0)
        {
            mySceneProcessor = sceneProcessor;
        }

        public override RdUsageGroup CreateModel(IOccurrence occurrence, IOccurrenceBrowserDescriptor descriptor)
        {
            if (occurrence is ReferenceOccurrence referenceOccurrence &&
                referenceOccurrence.PrimaryReference is IUnityYamlReference reference)
            {
                return CreateModel(UnityObjectPsiUtil.GetGameObjectPathFromComponent(mySceneProcessor, reference.ComponentDocument));
            }
            return EmptyModel();
        }

        public override void Navigate(IOccurrence occurrence)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsNavigateable => false;
    }

    public class ComponentUsageGroupingRule : UnityYamlUsageGroupingRuleBase
    {
        public ComponentUsageGroupingRule([NotNull] IconHost iconHost)
            : base("Unity Component", UnityObjectTypeThemedIcons.UnityComponent.Id, iconHost, 8.0)
        {
        }

        public override RdUsageGroup CreateModel(IOccurrence occurrence, IOccurrenceBrowserDescriptor descriptor)
        {
            if (occurrence is ReferenceOccurrence referenceOccurrence &&
                referenceOccurrence.PrimaryReference is IUnityYamlReference reference)
            {
                return CreateModel(UnityObjectPsiUtil.GetComponentName(reference.ComponentDocument));
            }

            return EmptyModel();
        }

        public override void Navigate(IOccurrence occurrence)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsNavigateable => false;
    }
}