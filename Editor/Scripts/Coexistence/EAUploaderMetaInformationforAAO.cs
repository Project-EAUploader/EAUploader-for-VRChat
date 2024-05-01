/// <summary>
/// This code is for AAOs
/// </summary>
#if HAS_AAO && UNITY_EDITOR
using Anatawa12.AvatarOptimizer.API;
using EAUploader.Components;

[ComponentInformation(typeof(EAUploaderMeta))]
internal class EAUploaderMetaInformation : ComponentInformation<EAUploaderMeta>
{
    protected override void CollectDependency(EAUploaderMeta component, ComponentDependencyCollector collector)
    {
        
    }

    protected override void CollectMutations(EAUploaderMeta component, ComponentMutationsCollector collector)
    {
        collector.ModifyProperties(component,
            nameof(EAUploaderMeta.status),
            nameof(EAUploaderMeta.type));
    }
}
#endif