// Code by Community Toolkit:
// https://github.com/CommunityToolkit/Windows/blob/main/components/Extensions/src/Tree/DependencyObjectExtensions.cs
// https://github.com/CommunityToolkit/Windows/blob/main/components/Extensions/src/Tree/Predicates/Interfaces/IPredicate%7BT%7D.cs
// https://github.com/CommunityToolkit/Windows/blob/main/components/Extensions/src/Tree/Predicates/PredicateByFunc%7BT%7D.cs#L14
// https://github.com/CommunityToolkit/Windows/blob/main/components/Extensions/src/Tree/Predicates/PredicateByAny%7BT%7D.cs
using System.Runtime.CompilerServices;

namespace CommunityToolkit.WinUI;

static class DependencyObjectExtensions
{
    /// <summary>
    /// Find the first descendant element of a given type, using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <param name="element">The root element.</param>
    /// <returns>The descendant that was found, or <see langword="null"/>.</returns>
    public static T? FindDescendant<T>(this DependencyObject element) where T : notnull, DependencyObject
    {
        PredicateByAny<T> predicateByAny = default;

        return FindDescendant<T, PredicateByAny<T>>(element, ref predicateByAny);
    }


    /// <summary>
    /// Find the first descendant element matching a given predicate, using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <param name="element">The root element.</param>
    /// <param name="predicate">The predicatee to use to match the descendant nodes.</param>
    /// <returns>The descendant that was found, or <see langword="null"/>.</returns>
    public static T? FindDescendant<T>(this DependencyObject element, Func<T, bool> predicate) where T : notnull, DependencyObject
    {
        PredicateByFunc<T> predicateByFunc = new(predicate);

        return FindDescendant<T, PredicateByFunc<T>>(element, ref predicateByFunc);
    }

    /// <summary>
    /// Find the first descendant element matching a given predicate, using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <typeparam name="TPredicate">The type of predicate in use.</typeparam>
    /// <param name="element">The root element.</param>
    /// <param name="predicate">The predicatee to use to match the descendant nodes.</param>
    /// <returns>The descendant that was found, or <see langword="null"/>.</returns>
    private static T? FindDescendant<T, TPredicate>(this DependencyObject element, ref TPredicate predicate) where T : notnull, DependencyObject where TPredicate : struct, IPredicate<T>
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);

        for (var i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);

            if (child is T result && predicate.Match(result))
            {
                return result;
            }

            T? descendant = FindDescendant<T, TPredicate>(child, ref predicate);

            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
    /// <summary>
    /// Find the first descendant (or self) element of a given type, using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <param name="element">The root element.</param>
    /// <returns>The descendant (or self) that was found, or <see langword="null"/>.</returns>
    public static T? FindDescendantOrSelf<T>(this DependencyObject element) where T : notnull, DependencyObject
    {
        if (element is T result)
        {
            return result;
        }

        return FindDescendant<T>(element);
    }

    /// <summary>
    /// Find the first descendant (or self) element matching a given predicate, using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <param name="element">The root element.</param>
    /// <param name="predicate">The predicatee to use to match the descendant nodes.</param>
    /// <returns>The descendant (or self) that was found, or <see langword="null"/>.</returns>
    public static T? FindDescendantOrSelf<T>(this DependencyObject element, Func<T, bool> predicate) where T : notnull, DependencyObject
    {
        if (element is T result && predicate(result))
        {
            return result;
        }

        return FindDescendant(element, predicate);
    }

    /// <summary>
    /// Find all descendant elements of the specified element. This method can be chained with
    /// LINQ calls to add additional filters or projections on top of the returned results.
    /// <para>
    /// This method is meant to provide extra flexibility in specific scenarios and it should not
    /// be used when only the first item is being looked for. In those cases, use one of the
    /// available <see cref="FindDescendant{T}(DependencyObject)"/> overloads instead, which will
    /// offer a more compact syntax as well as better performance in those cases.
    /// </para>
    /// </summary>
    /// <param name="element">The root element.</param>
    /// <returns>All the descendant <see cref="DependencyObject"/> instance from <paramref name="element"/>.</returns>
    public static IEnumerable<DependencyObject> FindDescendants(this DependencyObject element)
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);

        for (var i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);

            yield return child;

            foreach (DependencyObject childOfChild in FindDescendants(child))
            {
                yield return childOfChild;
            }
        }
    }

    /// <summary>
    /// Find the first ascendant element of a given type.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <param name="element">The starting element.</param>
    /// <returns>The ascendant that was found, or <see langword="null"/>.</returns>
    public static T? FindAscendant<T>(this DependencyObject element) where T : notnull, DependencyObject
    {
        PredicateByAny<T> predicateByAny = default;

        return FindAscendant<T, PredicateByAny<T>>(element, ref predicateByAny);
    }
    /// <summary>
    /// Find the first ascendant element matching a given predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements to match.</typeparam>
    /// <typeparam name="TPredicate">The type of predicate in use.</typeparam>
    /// <param name="element">The starting element.</param>
    /// <param name="predicate">The predicatee to use to match the ascendant nodes.</param>
    /// <returns>The ascendant that was found, or <see langword="null"/>.</returns>
    private static T? FindAscendant<T, TPredicate>(this DependencyObject element, ref TPredicate predicate)
#if HAS_UNO
		where T : class, DependencyObject
#else
        where T : notnull, DependencyObject
#endif
        where TPredicate : struct, IPredicate<T>
    {
        while (true)
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(element);

            if (parent is null)
            {
                return null;
            }

            if (parent is T result && predicate.Match(result))
            {
                return result;
            }

            element = parent;
        }
    }

    interface IPredicate<in T>
    where T : class
    {
        /// <summary>
        /// Performs a match with the current predicate over a target <typeparamref name="T"/> instance.
        /// </summary>
        /// <param name="element">The input element to match.</param>
        /// <returns>Whether the match evaluation was successful.</returns>
        bool Match(T element);
    }
    readonly struct PredicateByFunc<T> : IPredicate<T>
    where T : class
    {
        /// <summary>
        /// The predicatee to use to match items.
        /// </summary>
        private readonly Func<T, bool> predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateByFunc{T}"/> struct.
        /// </summary>
        /// <param name="predicate">The predicatee to use to match items.</param>
        public PredicateByFunc(Func<T, bool> predicate)
        {
            this.predicate = predicate;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(T element)
        {
            return this.predicate(element);
        }
    }
    readonly struct PredicateByAny<T> : IPredicate<T>
    where T : class
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(T element)
        {
            return true;
        }
    }
}