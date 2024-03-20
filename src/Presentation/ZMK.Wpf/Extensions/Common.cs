using MathNet.Numerics;

namespace ZMK.Wpf.Extensions;

public static class Common
{
    private const int RoundDigits = 2;

    public static double RoundForDisplay(this double value)
    {
        return value.Round(RoundDigits);
    }
}