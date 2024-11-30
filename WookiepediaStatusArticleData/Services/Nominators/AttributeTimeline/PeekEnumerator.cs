using System.Collections;

namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

/// <summary>
/// A wrapper around an enumerator found on StackOverflow here: https://codereview.stackexchange.com/a/33011
/// Thanks SO user `tia` for this. I modified it to not throw during `Peek` and instead return null. 
/// </summary>
public class PeekEnumerator<T>(IEnumerator<T> enumerator) : IEnumerator<T>
    where T : class
{
    private T? _peek;
    private bool _didPeek;

    #region IEnumerator implementation

    public bool MoveNext()
    {
        return _didPeek ? !(_didPeek = false) : enumerator.MoveNext();
    }

    public void Reset()
    {
        enumerator.Reset();
        _didPeek = false;
    }

    object IEnumerator.Current => Current;

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        enumerator.Dispose();
    }

    #endregion

    #region IEnumerator implementation

    public T Current => (_didPeek ? _peek : enumerator.Current) ??
                        throw new InvalidOperationException("Did not expect _peek to be null!");

    #endregion

    private void TryFetchPeek()
    {
        if (!_didPeek && (_didPeek = enumerator.MoveNext()))
        {
            _peek = enumerator.Current;
        }
    }

    public T? Peek
    {
        get
        {
            TryFetchPeek();
            if (!_didPeek)
                return null;

            return _peek;
        }
    }
}