using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Challenge.Collections.DebugViews;
using JetBrains.Annotations;
using ZLinq;
using ZLinq.Linq;

namespace Challenge.Collections;

/// <summary>
/// Dictionary which provides a default value when
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[PublicAPI, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
public sealed class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Default value implementation, can either be a constant value or a factory method
    /// </summary>
    public readonly struct DefaultValue
    {
        private readonly TValue defaultValue;
        private readonly Func<TValue>? defaultValueFactory;

        /// <summary>
        /// Creates a new <see cref="DefaultValue"/> which returns the default of TValue
        /// </summary>
        public DefaultValue()
        {
            this.defaultValue        = default!;
            this.defaultValueFactory = null;
        }

        /// <summary>
        /// Creates a new <see cref="DefaultValue"/> which returns the given default value
        /// </summary>
        /// <param name="value">Default value to return</param>
        private DefaultValue(TValue value)
        {
            this.defaultValue        = value;
            this.defaultValueFactory = null;
        }

        /// <summary>
        /// Creates a new <see cref="DefaultValue"/> which creates a new default value from the given factory method
        /// </summary>
        /// <param name="valueFactory">Default value factory method</param>
        public DefaultValue(Func<TValue> valueFactory)
        {
            this.defaultValue        = default!;
            this.defaultValueFactory = valueFactory;
        }

        /// <summary>
        /// Gets the default value
        /// </summary>
        /// <returns>Default value instance</returns>
        public TValue GetDefaultValue()
        {
            return this.defaultValueFactory is not null ? this.defaultValueFactory() : this.defaultValue;
        }

        /// <summary>
        /// Creates a new <see cref="DefaultValue"/> which returns the given default value
        /// </summary>
        /// <param name="value">Default value to return</param>
        /// <returns>The created <see cref="DefaultValue"/></returns>
        public static implicit operator DefaultValue(TValue value) => new(value);
    }

    private readonly Dictionary<TKey, TValue> dictionary;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Count" />
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.dictionary.Count;
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Capacity"/>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.dictionary.Capacity;
    }

    /// <summary>
    /// Default value of the dictionary
    /// </summary>
    public DefaultValue Default { get; }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Keys" />
    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.dictionary.Keys;
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Values" />
    public Dictionary<TKey, TValue>.ValueCollection Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.dictionary.Values;
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.this[TKey]"/>
    public TValue this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.dictionary.TryGetValue(key, out TValue? value) ? value : this.Default.GetDefaultValue();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.dictionary[key] = value;
    }

    /// <summary>
    /// Creates a new DefaultDictionary
    /// </summary>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>();
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(IDictionary<TKey, TValue> source, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(source);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> comparer, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(source, comparer);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data enumerable</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(source);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(source, comparer);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary with the given capacity
    /// </summary>
    /// <param name="capacity">Counter capacity</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(int capacity, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(capacity);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary with a specific <see cref="EqualityComparer{T}"/>
    /// </summary>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(IEqualityComparer<TKey> comparer, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(comparer);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary with the given capacity
    /// </summary>
    /// <param name="capacity">Counter capacity</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValue">Default value emmited by the dictionary when no value exists</param>
    public DefaultDictionary(int capacity, IEqualityComparer<TKey> comparer, DefaultValue defaultValue)
    {
        this.dictionary   = new Dictionary<TKey, TValue>(capacity, comparer);
        this.Default = defaultValue;
    }

    /// <summary>
    /// Creates a new DefaultDictionary with the given factory method
    /// </summary>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(Func<TValue> defaultValueFactory) : this(new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(IDictionary<TKey, TValue> source, Func<TValue> defaultValueFactory) : this(source, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> comparer, Func<TValue> defaultValueFactory) : this(source, comparer, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data enumerable</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TValue> defaultValueFactory) : this(source, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary from existing data
    /// </summary>
    /// <param name="source">Data dictionary</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer, Func<TValue> defaultValueFactory) : this(source, comparer, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary with the given capacity
    /// </summary>
    /// <param name="capacity">Counter capacity</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(int capacity, Func<TValue> defaultValueFactory) : this(capacity, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary with a specific <see cref="EqualityComparer{T}"/>
    /// </summary>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(IEqualityComparer<TKey> comparer, Func<TValue> defaultValueFactory) : this(comparer, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Creates a new DefaultDictionary with the given capacity
    /// </summary>
    /// <param name="capacity">Counter capacity</param>
    /// <param name="comparer">Match equality comparer</param>
    /// <param name="defaultValueFactory">Factory method that creates the new value emmited by this DefaultDictionary when no value exists</param>
    public DefaultDictionary(int capacity, IEqualityComparer<TKey> comparer, Func<TValue> defaultValueFactory) : this(capacity, comparer, new DefaultValue(defaultValueFactory)) { }

    /// <summary>
    /// Copies a DefaultDictionary
    /// </summary>
    /// <param name="other">Other dictionary to copy from</param>
    public DefaultDictionary(DefaultDictionary<TKey, TValue> other)
    {
        this.dictionary = new Dictionary<TKey, TValue>(other.dictionary);
        this.Default    = other.Default;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) => this.dictionary.Add(key, value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryAdd" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryAdd(TKey key, TValue value) => this.dictionary.TryAdd(key, value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsKey" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => this.dictionary.ContainsKey(key);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsValue(TValue value) => this.dictionary.ContainsValue(value);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) => this.dictionary.Remove(key);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value) => this.dictionary.TryGetValue(key, out value!);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => this.dictionary.Clear();

    /// <inheritdoc cref="Dictionary{TKey, TValue}.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => this.dictionary.GetEnumerator();

    /// <summary>
    /// ValueEnumerable over this DefaultDictionary
    /// </summary>
    /// <returns>ValueEnumerable of this DefaultDictionary</returns>
    public ValueEnumerable<FromDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return new ValueEnumerable<FromDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(new FromDictionary<TKey, TValue>(this.dictionary));
    }

    /// <inheritdoc />
    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Keys;
    }

    /// <inheritdoc />
    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Values;
    }

    /// <inheritdoc />
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Keys;
    }

    /// <inheritdoc />
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Values;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Add(item);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Contains(item);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Remove(item);
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
