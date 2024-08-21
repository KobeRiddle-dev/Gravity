#if USE_LARGE_WORLDS
using Real = System.Double;
using Mathr = FlaxEngine.Mathd;
#else
using Real = System.float;
using Mathr = FlaxEngine.Mathf;
#endif

namespace Gravity.Gravity.Units;

struct Mass(Real grams)
{
    public const Real GramsPerKilogram = 1000;

    private readonly Real grams = grams;

    public readonly Real Meters { get => this.grams / GramsPerKilogram; }

    public readonly Real Grams { get => this.grams; }

    public static Mass FromKilograms(Real meters)
    {
        return new Mass(meters * GramsPerKilogram);
    }

    public static Mass operator +(Mass left, Mass right)
    {
        return new Mass(left.grams + right.grams);
    }

    public static Mass operator -(Mass left, Mass right)
    {
        return new Mass(left.grams - right.grams);
    }

    public static Mass operator *(Mass left, Mass right)
    {
        return new Mass(left.grams * right.grams);
    }

    public static Mass operator /(Mass left, Mass right)
    {
        return new Mass(left.grams / right.grams);
    }

    public static Mass operator -(Mass Mass)
    {
        return new Mass(-Mass.grams);
    }

    public override readonly bool Equals(object obj)
    {
        if (obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }

        return this.grams.Equals(((Mass)obj).grams);
    }

    public override readonly int GetHashCode()
    {
        return this.grams.GetHashCode();
    }

    public static bool operator ==(Mass left, Mass right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Mass left, Mass right)
    {
        return !left.Equals(right);
    }
}
