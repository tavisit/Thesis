public enum HarvardSpectralClassLetter
{
    O,
    B,
    A,
    F,
    G,
    K,
    M,
    Unknown
}

public class StarClassifier
{
    public static (HarvardSpectralClassLetter, int) ClassifyHarvardSpectral(double temperature)
    {
        if (temperature >= 33000)
        {
            return (HarvardSpectralClassLetter.O, 0);
        }
        else if (temperature >= 10000)
        {
            return (HarvardSpectralClassLetter.B, (int)((temperature - 10000) / 1000) + 0);
        }
        else if (temperature >= 7500)
        {
            return (HarvardSpectralClassLetter.A, (int)((temperature - 7500) / 500) + 0);
        }
        else if (temperature >= 6000)
        {
            return (HarvardSpectralClassLetter.F, (int)((temperature - 6000) / 500) + 0);
        }
        else if (temperature >= 5200)
        {
            return (HarvardSpectralClassLetter.G, (int)((temperature - 5200) / 300) + 0);
        }
        else if (temperature >= 3700)
        {
            return (HarvardSpectralClassLetter.K, (int)((temperature - 3700) / 300) + 0);
        }
        else if (temperature >= 2000)
        {
            return (HarvardSpectralClassLetter.M, (int)((temperature - 2000) / 200) + 0);
        }
        else
        {
            return (HarvardSpectralClassLetter.Unknown, 0);
        }
    }
}