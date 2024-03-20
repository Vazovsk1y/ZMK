namespace ZMK.Domain.Common;

public interface IMarkAuditable
{
    double MarkCount { get; }

    string MarkTitle { get; }

    string MarkCode { get; }

    int MarkOrder { get; }

    double MarkWeight { get; }
}