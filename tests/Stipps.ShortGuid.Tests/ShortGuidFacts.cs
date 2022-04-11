using System;
using Shouldly;
using Xunit;
using Stipps;

namespace Tests;

public class ShortGuidFacts
{
    const string SampleGuidString = "c9a646d3-9c61-4cb7-bfcd-ee2522c8f633";
    static readonly Guid SampleGuid = new Guid(SampleGuidString);
    const string SampleShortGuidString = "00amyWGct0y_ze4lIsj2Mw";

    /// <summary>
    /// Literal: c9a646d3-9c61-4cb7-bfcd-ee2522c8f633 and some extra chars.
    /// </summary>
    const string LongerBase64String = "YzlhNjQ2ZDMtOWM2MS00Y2I3LWJmY2QtZWUyNTIyYzhmNjMzIGFuZCBzb21lIGV4dHJhIGNoYXJzLg";

    /// <summary>
    /// "bullshitmustnotbevalid" in this case does produce a valid Guid, which when output encodes as correctly
    /// as "bullshitmustnotbevaliQ".
    /// </summary>
    const string InvalidSampleShortGuidString = "bullshitmustnotbevalid";

    private static void assert_instance_equals_samples(ShortGuid instance)
    {
        instance.Value.ShouldBeEquivalentTo(SampleShortGuidString);
        instance.Guid.ShouldBeEquivalentTo(SampleGuid);
    }

    [Fact]
    void ctor_decodes_shortguid_string()
    {
        var actual = new ShortGuid(SampleShortGuidString);

        assert_instance_equals_samples(actual);
    }

    [Fact]
    void StrictDecode_parses_valid_shortGuid_strict_off()
    {
        ShortGuid.Decode(SampleShortGuidString);
    }

    [Fact]
    void StrictDecode_parses_valid_shortGuid_strict_on()
    {
        ShortGuid.Decode(SampleShortGuidString);
    }

    [Fact]
    void Decode_does_not_parse_longer_base64_string()
    {
        Should.Throw<ArgumentException>(() => ShortGuid.Decode(LongerBase64String));
    }

    [Fact]
    void StrictDecode_does_not_parse_longer_base64_string()
    {
        Should.Throw<ArgumentException>(
            () => ShortGuid.Decode(LongerBase64String)
        );
    }

    [Fact]
    void invalid_strings_must_not_return_true_on_try_parse_with_strict_true()
    {
        // try parse should return false
        ShortGuid.TryParse(InvalidSampleShortGuidString, out ShortGuid strictSguid)
            .ShouldBeFalse();

        // decode should throw
        Should.Throw<FormatException>(
            () => ShortGuid.Decode(InvalidSampleShortGuidString)
        );

        // .ctor should throw
        Should.Throw<FormatException>(
            () => new ShortGuid(InvalidSampleShortGuidString)
        );
    }

    [Fact]
    void ctor_throws_when_trying_to_decode_guid_string()
    {
        Should.Throw<ArgumentException>(
            () => new ShortGuid(SampleGuidString)
        );
    }

    [Fact]
    void TryParse_decodes_shortguid_string()
    {
        ShortGuid.TryParse(SampleShortGuidString, out ShortGuid actual);

        assert_instance_equals_samples(actual);
    }

    [Fact]
    void TryParse_decodes_guid_string()
    {
        ShortGuid.TryParse(SampleGuidString, out ShortGuid actual);

        assert_instance_equals_samples(actual);
    }

    [Fact]
    void TryParse_decodes_empty_guid_literal_as_empty()
    {
        var result = ShortGuid.TryParse(Guid.Empty.ToString(), out ShortGuid actual);

        result.ShouldBeTrue();
        actual.Guid.ShouldBeEquivalentTo(Guid.Empty);
        Assert.True(result);
        Assert.Equal(Guid.Empty, actual.Guid);
    }

    [Fact]
    void TryParse_decodes_empty_string_as_empty()
    {
        var result = ShortGuid.TryParse(string.Empty, out ShortGuid actual);
        result.ShouldBeFalse();
        actual.Guid.ShouldBeEquivalentTo(Guid.Empty);
    }

    [Fact]
    void TryParse_decodes_bad_string_as_empty()
    {
        var result = ShortGuid.TryParse("Nothing to see here...", out ShortGuid actual);
        result.ShouldBeFalse();
        actual.Guid.ShouldBeEquivalentTo(Guid.Empty);
    }

    [Fact]
    void Encode_creates_expected_string()
    {
        var actual = ShortGuid.Encode(SampleGuid);
        actual.ToString().ShouldBeEquivalentTo(SampleShortGuidString);
    }

    [Fact]
    void Decode_takes_expected_string()
    {
        var actual = ShortGuid.Decode(SampleShortGuidString);
        actual.ShouldBeEquivalentTo(SampleGuid);
    }

    [Fact]
    void Decode_fails_on_unexpected_string()
    {
        Should.Throw<ArgumentException>(
            () => ShortGuid.Decode("Am I valid?")
        );

        Should.Throw<FormatException>(
            () => ShortGuid.Decode("I am 22characters long")
        );
    }

    [Fact]
    void instance_equality_equals()
    {
        var actual = new ShortGuid(SampleShortGuidString);
        actual.Equals(actual).ShouldBeTrue();
        actual.Equals(SampleGuid).ShouldBeTrue();
        actual.Equals(SampleGuidString).ShouldBeTrue();
        actual.Equals(SampleShortGuidString).ShouldBeTrue();
    }

    [Fact]
    void operator_eqaulity_equals()
    {
        var actual = new ShortGuid(SampleShortGuidString);

#pragma warning disable CS1718 // Comparison made to same variable
        (actual == actual).ShouldBeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
        (actual == null).ShouldBeFalse();

        (actual == SampleGuid).ShouldBeTrue();
        (SampleGuid == actual).ShouldBeTrue();

        (actual == SampleGuidString).ShouldBeTrue();
        (actual == SampleShortGuidString).ShouldBeTrue();

        (null == ShortGuid.Empty).ShouldBeTrue();
    }

    [Fact]
    void ShortGuid_Emtpy_equals_Guid_Empty()
    {
        (Guid.Empty.Equals(ShortGuid.Empty)).ShouldBeTrue();
    }
}