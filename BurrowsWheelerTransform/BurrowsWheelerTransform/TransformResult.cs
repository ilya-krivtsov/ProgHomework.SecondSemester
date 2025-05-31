namespace BurrowsWheelerTransform;

/// <summary>
/// Result of Burrows-Wheeler Transform.
/// </summary>
public readonly struct TransformResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformResult"/> struct.
    /// </summary>
    /// <param name="value">Transformed string.</param>
    /// <param name="identityIndex">Index that is used to reconstruct string.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="identityIndex"/> is out of range (less than 0 or greater or equal to <paramref name="identityIndex"/>.Length)
    /// or not equal to -1 if <paramref name="identityIndex"/> is <see cref="string.Empty"/>.
    /// </exception>
    public TransformResult(string value, int identityIndex)
    {
        if (value.Length == 0)
        {
            if (identityIndex != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(identityIndex), "Identity index of an empty string must be equal to -1");
            }
        }
        else
        {
            ArgumentOutOfRangeException.ThrowIfNegative(identityIndex, nameof(identityIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(identityIndex, value.Length, nameof(identityIndex));
        }

        Value = value;
        IdentityIndex = identityIndex;
    }

    /// <summary>
    /// Gets transformed string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets identity index.
    /// </summary>
    public int IdentityIndex { get; }
}
