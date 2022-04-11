using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace Stipps;

public readonly struct ShortGuid
{
    private const char EqualsChar = '=';
    private const char HyphenChar = '-';
    private const char UnderscoreChar = '_';
    private const char SlashChar = '/';
    private const byte SlashByte = (byte) '/';
    private const char PlusChar = '+';
    private const byte PlusByte = (byte) '+';

    /// <summary>
    /// A read-only instance of the ShortGuid struct whose value is guaranteed to be all zeroes i.e. equivalent
    /// to <see cref="System.Guid.Empty"/>.
    /// </summary>
    public static readonly ShortGuid Empty = new (Guid.Empty);

    private readonly Guid _underlyingGuid;
    private readonly string _encodedValue;

    /// <summary>
    /// Creates a new instance with the given URL-safe Base64 encoded string.
    /// <para>See also <seealso cref="ShortGuid.TryParse(ReadOnlySpan{char}, out ShortGuid)"/> which will try to coerce the
    /// the value from URL-safe Base64 or normal Guid string.</para>
    /// </summary>
    /// <param name="value">A 22 character URL-safe Base64 encoded string to decode.</param>
    public ShortGuid(string value)
    {
        _encodedValue = value;
        _underlyingGuid = Decode(value);
    }

    /// <summary>
    /// Creates a new instance with the given <see cref="System.Guid"/>.
    /// </summary>
    /// <param name="guid">The <see cref="System.Guid"/> to encode.</param>
    public ShortGuid(Guid guid)
    {
        _underlyingGuid = guid;
        _encodedValue = Encode(_underlyingGuid);
    }

    /// <summary>
    /// Gets the underlying <see cref="System.Guid"/> for the encoded ShortGuid.
    /// </summary>
    public Guid Guid => _underlyingGuid;

    public override string ToString() => Value;

    /// <summary>
    /// Gets the encoded string value of the <see cref="Guid"/> as a URL-safe Base64 string.
    /// </summary>
    public string Value => _encodedValue;

    /// <summary>
    /// Encodes the given <see cref="System.Guid"/> as an encoded ShortGuid string. The encoding is
    /// similar to Base64, with some non-URL safe characters replaced, and padding removed, resulting
    /// in a 22 character string.
    /// </summary>
    /// <param name="guid">The <see cref="System.Guid"/> to encode.</param>
    /// <returns>A 22 character ShortGuid URL-safe Base64 string.</returns>
    public static string Encode(Guid guid)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        Span<byte> base64Bytes = stackalloc byte[24];

        MemoryMarshal.TryWrite(guidBytes, ref guid);
        Base64.EncodeToUtf8(guidBytes, base64Bytes, out _, out _);

        Span<char> finalChars = stackalloc char[22];

        for (var i = 0; i < 22; i++)
        {
            finalChars[i] = base64Bytes[i] switch
            {
                SlashByte => UnderscoreChar,
                PlusByte => HyphenChar,
                _ => (char)base64Bytes[i]
            };
        }

        return new string(finalChars);
    }

    /// <summary>
    /// Encodes the given value as an encoded ShortGuid string. The encoding is similar to Base64, with
    /// some non-URL safe characters replaced, and padding removed, resulting in a 22 character string.
    /// </summary>
    /// <param name="value">Any valid <see cref="System.Guid"/> string.</param>
    /// <returns>A 22 character ShortGuid URL-safe Base64 string.</returns>
    public static ReadOnlySpan<char> Encode(ReadOnlySpan<char> value)
    {
        var guid = Guid.Parse(value);
        return Encode(guid);
    }

    /// <summary>
    /// Decodes the given value from a 22 character URL-safe Base64 string to a <see cref="System.Guid"/>.
    /// <para>Supports: ShortGuid format only.</para>
    /// <para>See also <seealso cref="ShortGuid.TryDecode(ReadOnlySpan{char}, out System.Guid)"/> or <seealso cref="TryParse(ReadOnlySpan{char}, out System.Guid)"/>.</para>
    /// </summary>
    /// <param name="value">A 22 character URL-safe Base64 encoded ReadOnlySpan of char to decode.</param>
    /// <returns>A new <see cref="System.Guid"/> instance from the parsed string.</returns>
    /// <exception cref="FormatException">
    /// If <paramref name="value"/> is not a valid Base64 string (<seealso cref="Convert.FromBase64String(string)"/>)
    /// or if the decoded guid doesn't strictly match the input <paramref name="value"/>.
    /// </exception>
    public static Guid Decode(ReadOnlySpan<char> value)
    {
        // avoid parsing larger strings/blobs
        if (value.Length != 22)
        {
            throw new ArgumentException(
                $"A ShortGuid must be exactly 22 characters long. Received a {value.Length} character string.",
                paramName: nameof(value)
            );
        }
        
        Span<char> base64Chars = stackalloc char[24];

        for (var i = 0; i < 22; i++)
        {
            base64Chars[i] = value[i] switch
            {
                HyphenChar => PlusChar,
                UnderscoreChar => SlashChar,
                _ => value[i]
            };
        }

        base64Chars[22] = EqualsChar;
        base64Chars[23] = EqualsChar;

        Span<byte> idBytes = stackalloc byte[16];
        Convert.TryFromBase64Chars(base64Chars, idBytes, out _);

        var guid = new Guid(idBytes);
        var sanityCheck = Encode(guid);
        if (!value.SequenceEqual(sanityCheck))
        {
            throw new FormatException(
                $"Invalid strict ShortGuid encoded string. The string '{new string(value)}' " +
                $"but failed a round-trip test expecting '{new string(sanityCheck)}'."
            );
        }
        
        return guid;
    }

    /// <summary>
    /// <para>Supports ShortGuid format only.</para>
    /// <para>Attempts to decode the given value from a 22 character URL-safe Base64 string to
    /// a <see cref="System.Guid"/>.</para>
    /// <para>The difference between TryParse and TryDecode:</para>
    /// <list type="number">
    ///     <item>
    ///         <term><see cref="TryParse(ReadOnlySpan{char}, out ShortGuid)"/></term>
    ///         <description>Supports: Guid &amp; ShortGuid;</description>
    ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
    ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TryParse(ReadOnlySpan{char}, out System.Guid)"/></term>
    ///         <description>Supports: Guid &amp; ShortGuid;</description>
    ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
    ///         <see cref="System.Guid"/>, outputs the underlying <see cref="System.Guid"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TryDecode(ReadOnlySpan{char}, out System.Guid)"/></term>
    ///         <description>Supports: ShortGuid;</description>
    ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
    ///         <see cref="ShortGuid"/> only, but outputs the result as a <see cref="System.Guid"/> - this method.</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <param name="value">The ShortGuid encoded string to decode.</param>
    /// <param name="guid">A new <see cref="System.Guid"/> instance from the parsed string.</param>
    /// <returns>A boolean indicating if the decode was successful.</returns>
    public static bool TryDecode(ReadOnlySpan<char> value, out Guid guid)
    {
        try
        {
            guid = Decode(value);
            return true;
        }
        catch
        {
            guid = Guid.Empty;
            return false;
        }
    }

    /// <summary>
    /// <para>Supports ShortGuid &amp; Guid formats.</para>
    /// <para>Tries to parse the value from either a 22 character URL-safe Base64 string or
    /// a <see cref="System.Guid"/> string, and outputs a <see cref="ShortGuid"/> instance.</para>
    /// <para>The difference between TryParse and TryDecode:</para>
    /// <list type="number">
    ///     <item>
    ///         <term><see cref="TryParse(ReadOnlySpan{char}, out ShortGuid)"/></term>
    ///         <description>Supports: Guid &amp; ShortGuid; </description>
    ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
    ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance - this method.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TryParse(ReadOnlySpan{char}, out System.Guid)"/></term>
    ///         <description>Supports: Guid &amp; ShortGuid;</description>
    ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
    ///         <see cref="System.Guid"/>, outputs the <see cref="System.Guid"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TryDecode(ReadOnlySpan{char}, out System.Guid)"/></term>
    ///         <description>Supports: ShortGuid;</description>
    ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
    ///         <see cref="ShortGuid"/> only, but outputs the result as a <see cref="System.Guid"/>.</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <param name="value">The ShortGuid encoded string or string representation of a Guid.</param>
    /// <param name="shortGuid">A new <see cref="ShortGuid"/> instance from the parsed string.</param>
    public static bool TryParse(ReadOnlySpan<char> value, out ShortGuid shortGuid)
    {
        switch (value)
        {
            case {Length: 22} when TryDecode(value, out var sg):
                shortGuid = sg;
                return true;
            case {Length: 36} when Guid.TryParse(value, out var guid):
            {
                shortGuid = guid;
                return true;
            }
            default:
                shortGuid = Empty;
                return false;
        }
    }
    
    /// <summary>
        /// <para>Supports ShortGuid &amp; Guid formats.</para>
        /// <para>Tries to parse the value either a 22 character URL-safe Base64 string or
        /// <see cref="System.Guid"/> string, and outputs the <see cref="Guid"/> value.</para>
        /// <para>The difference between TryParse and TryDecode:</para>
        /// <list type="number">
        ///     <item>
        ///         <term><see cref="TryParse(ReadOnlySpan{char}, out ShortGuid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryParse(ReadOnlySpan{char}, out System.Guid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="System.Guid"/> - this method.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryDecode(ReadOnlySpan{char}, out System.Guid)"/></term>
        ///         <description>Supports: ShortGuid;</description>
        ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
        ///         <see cref="ShortGuid"/> only, outputting the result as a <see cref="System.Guid"/>.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="value">The ShortGuid encoded string or string representation of a Guid.</param>
        /// <param name="guid">A new <see cref="System.Guid"/> instance from the parsed string.</param>
        /// <returns>A boolean indicating if the parse was successful.</returns>
        public static bool TryParse(ReadOnlySpan<char> value, out Guid guid)
        {
            switch (value)
            {

                case {Length: 22} when TryDecode(value, out guid):
                case {Length: 36} when Guid.TryParse(value, out guid):
                    return true;
                default:
                    guid = Guid.Empty;
                    return false;
            }
        }

    /// <summary>
    /// Returns a value indicating whether this instance and a specified object represent the same type and value.
    /// <para>Compares for equality against other string, Guid and ShortGuid types.</para>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is ShortGuid shortGuid)
        {
            return _underlyingGuid.Equals(shortGuid._underlyingGuid);
        }

        if (obj is Guid guid)
        {
            return _underlyingGuid.Equals(guid);
        }

        if (obj is string span)
        {
            // Try a ShortGuid string
            if (TryDecode(span, out guid))
                return _underlyingGuid.Equals(guid);

            if (Guid.TryParse(span, out guid))
                return _underlyingGuid.Equals(guid);
        }

        if (obj is char[] chars)
        {
            // Try a ShortGuid string
            if (TryDecode(chars, out guid))
                return _underlyingGuid.Equals(guid);

            if (Guid.TryParse(chars, out guid))
                return _underlyingGuid.Equals(guid);
        }

        return false;
    }

    /// <summary>
    /// Returns the hash code for the underlying <see cref="System.Guid"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => _underlyingGuid.GetHashCode();

    /// <summary>
    /// Initialises a new instance of a ShortGuid using <see cref="M:Guid.NewGuid()"/>.
    /// <para>Equivalent of calling: <code>`new ShortGuid(Guid.NewGuid())`</code></para>
    /// </summary>
    /// <returns></returns>
    public static ShortGuid NewGuid() => new (Guid.NewGuid());

    public static bool operator ==(ShortGuid a, ShortGuid b)
        => a._underlyingGuid == b._underlyingGuid;

    public static bool operator ==(ShortGuid a, Guid b)
        => a._underlyingGuid == b;

    public static bool operator ==(Guid a, ShortGuid b)
        => b == a;
    
    public static bool operator !=(ShortGuid a, ShortGuid b)
        => a._underlyingGuid != b._underlyingGuid;

    public static bool operator !=(ShortGuid a, Guid b)
        => a._underlyingGuid != b;

    public static bool operator !=(Guid a, ShortGuid b)
        => b != a;

    public static implicit operator ReadOnlySpan<char>(ShortGuid shortGuid) => shortGuid.Value;

    public static implicit operator string(ShortGuid shortGuid) => shortGuid.Value;

    public static implicit operator Guid(ShortGuid shortGuid) => shortGuid._underlyingGuid;

    public static implicit operator ShortGuid(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return Empty;

        if (TryParse(value, out ShortGuid shortGuid))
            return shortGuid;

        throw new FormatException("ShortGuid should contain 22 Base64 characters " 
                                  + "or 32 characters with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
    }

    public static implicit operator ShortGuid(Guid guid)
        => guid == Guid.Empty 
            ? Empty
            : new ShortGuid(guid);
}