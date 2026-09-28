using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class MatchSelection
{
    public Guid? Left { get; private set; }
    public Guid? Right { get; private set; }

    public (Guid Left, Guid Right)? Select(LearningSession session, Guid cardId, bool isLeft)
    {
        if (session.Mode != LearningMode.Match || session.IsComplete || session.MatchedIds.Contains(cardId) ||
            !session.Questions.Any(question => question.CardId == cardId)) return null;
        if (isLeft) Left = Left == cardId ? null : cardId;
        else Right = Right == cardId ? null : cardId;
        if (Left is not { } left || Right is not { } right) return null;
        Clear();
        return (left, right);
    }

    public void Clear() { Left = null; Right = null; }
}
