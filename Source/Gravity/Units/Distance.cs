#if USE_LARGE_WORLDS
using Real = System.Double;
using Mathr = FlaxEngine.Mathd;
#else
using Real = System.float;
using Mathr = FlaxEngine.Mathf;
#endif

namespace Gravity.Gravity.Units;
struct Distance(Real centimeters)
{
    public const Real MetersPerKilometer = 1000;
    public const Real CentimetersPerMeter = 1000;

    private readonly Real centimeters = centimeters;

    public readonly Real Kilometers { get => this.Meters / MetersPerKilometer; }

    public readonly Real Meters { get => this.centimeters / CentimetersPerMeter; }

    public readonly Real Centimeters { get => this.centimeters; }

    public static Distance FromMeters(Real meters)
    {
        return new Distance(meters * CentimetersPerMeter);
    }

    public static Distance FromKilometers(Real kilometers)
    {
        return FromMeters(kilometers * MetersPerKilometer);
    }

    public static Distance operator +(Distance left, Distance right)
    {
        return new Distance(left.centimeters + right.centimeters);
    }

    public static Distance operator -(Distance left, Distance right)
    {
        return new Distance(left.centimeters - right.centimeters);
    }

    public static Distance operator *(Distance left, Distance right)
    {
        return new Distance(left.centimeters * right.centimeters);
    }

    public static Distance operator /(Distance left, Distance right)
    {
        return new Distance(left.centimeters / right.centimeters);
    }

    public static Distance operator -(Distance distance)
    {
        return new Distance(-distance.centimeters);
    }

    public override readonly bool Equals(object obj)
    {
        if (obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }

        return this.centimeters.Equals(((Distance)obj).centimeters);
    }

    public override readonly int GetHashCode()
    {
        return this.centimeters.GetHashCode();
    }

    public static bool operator ==(Distance left, Distance right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Distance left, Distance right)
    {
        return !left.Equals(right);
    }
}
