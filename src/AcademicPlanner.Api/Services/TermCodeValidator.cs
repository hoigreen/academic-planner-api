namespace AcademicPlanner.Api.Services;

public static class TermCodeValidator
{
    public static bool IsValid(int termCode)
    {
        var termNo = termCode % 10;
        return termCode is >= 20000 and <= 29999 && (termNo == 1 || termNo == 2 || termNo == 3);
    }
}
