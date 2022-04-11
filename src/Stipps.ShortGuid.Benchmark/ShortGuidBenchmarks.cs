using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.SlowestToFastest)]
[RankColumn]
public class ShortGuidBenchmarks
{
    const string SampleGuidString = "c9a646d3-9c61-4cb7-bfcd-ee2522c8f633";
    static readonly Guid SampleGuid = new (SampleGuidString);
    const string SampleShortGuidString = "00amyWGct0y_ze4lIsj2Mw";

    [Benchmark]
    public void EncodeFromString_CsharpVitamins()
    {
        CSharpVitamins.ShortGuid.Encode(SampleGuidString);
    }

    [Benchmark]
    public void EncodeFromString_Stipps()
    {
        Stipps.ShortGuid.Encode(SampleGuidString);
    }

    [Benchmark]
    public void EncodeFromGuid_CsharpVitamins()
    {
        CSharpVitamins.ShortGuid.Encode(SampleGuid);
    }
    
    [Benchmark]
    public void EncodeFromGuid_Stipps()
    {
        Stipps.ShortGuid.Encode(SampleGuid);
    }

    [Benchmark]
    public void DecodeFromShortGuidString_CSharVitamins()
    {
        CSharpVitamins.ShortGuid.Decode(SampleShortGuidString);
    }
    
    [Benchmark]
    public void DecodeFromShortGuidString_Stipps()
    {
        Stipps.ShortGuid.Decode(SampleShortGuidString);
    }

    [Benchmark]
    public void TryDecodeFromShortGuidString_CSharpVitamins()
    {
        CSharpVitamins.ShortGuid.TryDecode(SampleShortGuidString, out _);
    }
    
    [Benchmark]
    public void TryDecodeFromShortGuidStringStipps()
    {
        Stipps.ShortGuid.TryDecode(SampleShortGuidString, out _);
    }
    
    [Benchmark]
    public void TryParseGuidStringOutShortGuid_CSharpVitamins()
    {
        CSharpVitamins.ShortGuid.TryParse(SampleGuidString, out CSharpVitamins.ShortGuid _);
    }
    
    [Benchmark]
    public void TryParseGuidStringOutShortGuid_Stipps()
    {
        Stipps.ShortGuid.TryParse(SampleGuidString, out Stipps.ShortGuid _);
    }
    
    [Benchmark]
    public void TryParseGuidStringOutGuid_CSharpVitamins()
    {
        CSharpVitamins.ShortGuid.TryParse(SampleGuidString, out Guid _);
    }
    
    [Benchmark]
    public void TryParseGuidStringOutGuid_Stipps()
    {
        Stipps.ShortGuid.TryParse(SampleGuidString, out Guid _);
    }
    
    [Benchmark]
    public void TryParseShortGuidStringOutShortGuid_CSharpVitamins()
    {
        CSharpVitamins.ShortGuid.TryParse(SampleShortGuidString, out CSharpVitamins.ShortGuid _);
    }
    
    [Benchmark]
    public void TryParseShortGuidStringOutShortGuid_Stipps()
    {
        Stipps.ShortGuid.TryParse(SampleShortGuidString, out Stipps.ShortGuid _);
    }
    
    [Benchmark]
    public void TryParseShortGuidStringOutGuid_CSharpVitamins()
    {
        CSharpVitamins.ShortGuid.TryParse(SampleShortGuidString, out Guid _);
    }
    
    [Benchmark]
    public void TryParseShortGuidStringOutGuid_Stipps()
    {
        Stipps.ShortGuid.TryParse(SampleShortGuidString, out Guid _);
    }
}