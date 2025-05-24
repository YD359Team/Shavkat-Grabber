using System;

namespace Shavkat_grabber.Helpers;

public static class RandomHelper
{
    private static readonly char[] Letters = "abcdefghijklmnopqrstuwxyz".ToCharArray();
    private static readonly char[] Digits = "0123456789".ToCharArray();

    public static char[] GetRandomLetters(int count)
    {
        return Random.Shared.GetItems(Letters, count);
    }

    public static char[] GetRandomDigits(int count)
    {
        return Random.Shared.GetItems(Digits, count);
    }
}
