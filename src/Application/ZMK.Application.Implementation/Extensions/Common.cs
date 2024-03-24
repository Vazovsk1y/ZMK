using MathNet.Numerics;

namespace ZMK.Application.Implementation.Extensions;

public static class Common
{
    private const int RoundDigits = 2;

    public static double RoundForReport(this double value)
    {
        return value.Round(RoundDigits);
    }
}