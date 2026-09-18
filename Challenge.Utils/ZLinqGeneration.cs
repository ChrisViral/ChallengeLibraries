using ZLinq;

[assembly: ZLinqDropIn("", DropInGenerateTypes.Collection, GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("", "System.String", GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("Challenge.Utils.Extensions.Ranges", "System.Range", "Challenge.Utils.ValueEnumerators.FromRange", GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("Challenge.Utils.Extensions.Spans", "CommunityToolkit.HighPerformance.ReadOnlySpan2D`1", "Challenge.Utils.ValueEnumerators.FromSpan2D`1", GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("Challenge.Utils.Extensions.Spans", "CommunityToolkit.HighPerformance.Span2D`1", "Challenge.Utils.ValueEnumerators.FromSpan2D`1", GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("Challenge.Utils.Extensions.Enumerables", "CommunityToolkit.HighPerformance.Enumerables.RefEnumerable`1", "Challenge.Utils.ValueEnumerators.FromRefEnumerable`1", GenerateAsPublic = true)]
[assembly: ZLinqDropInExternalExtension("Challenge.Utils.Extensions.Regexes", "System.Text.RegularExpressions.Regex+ValueMatchEnumerator", "Challenge.Utils.ValueEnumerators.FromValueMatchEnumerator", GenerateAsPublic = true)]
