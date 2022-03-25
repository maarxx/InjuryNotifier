namespace InjuryNotifier;

public struct PawnPartProblem
{
    public readonly int pawnIDNumber;
    public readonly string part_label;
    public readonly string hediff_label;

    public PawnPartProblem(int pawnIDNumber, string part_label, string hediff_label)
    {
        this.pawnIDNumber = pawnIDNumber;
        this.part_label = part_label;
        this.hediff_label = hediff_label;
    }

    public override int GetHashCode()
    {
        return pawnIDNumber.GetHashCode() * part_label.GetHashCode() * hediff_label.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is PawnPartProblem problem && Equals(problem);
    }

    public bool Equals(PawnPartProblem ppp)
    {
        return ppp.pawnIDNumber == pawnIDNumber && ppp.part_label == part_label && ppp.hediff_label == hediff_label;
    }
}