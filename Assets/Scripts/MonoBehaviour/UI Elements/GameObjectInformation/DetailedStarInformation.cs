using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DetailedStarInformation
{
    public GameObject informationPanel;
    public GameObject star;

    float innerHabitableSunAbsolute = 142118977f;
    float outerHabitableSunAbsolute = 205049590f;

    StarClassifier starClassifier;

    public DetailedStarInformation(GameObject informationPanel, GameObject star)
    {
        this.informationPanel = informationPanel;
        this.star = star;

        starClassifier = StarClassifier.Instance;
    }

    public void drawMap()
    {
        Dictionary<string, GameObject> allChildren = GetAllChildren(informationPanel);

        var relativeLuminousity = star.GetComponentInChildren<StellarBody>().relativeLuminousity;
        var starTemperature = star.GetComponentInChildren<StellarBody>().starTemperature;

        // assign name to the main title
        allChildren.GetValueOrDefault("Star Name").GetComponent<TMP_Text>().text = star.name;

        // give star the right color
        Color color = TemperatureToRGB(starTemperature);

        // resize to actual star size:
        allChildren.GetValueOrDefault("Star").transform.localScale = new Vector3(1, 1, 1) * star.transform.localScale[0];

        // Adjust its color
        allChildren.GetValueOrDefault("Star").GetComponent<Image>().color = color;

        // inner and outer habitable zones:
        float innerBoundary = CalculateInnerBoundary(relativeLuminousity, starTemperature);
        float outerBoundary = CalculateOuterBoundary(relativeLuminousity, starTemperature);

        //size up the boundaries
        allChildren.GetValueOrDefault("BlackMask").transform.localScale = new Vector3(1, 1, 1) * innerBoundary;
        allChildren.GetValueOrDefault("LowerHabitableZoneDistance").transform.localScale = new Vector3(1, 1, 1) * innerBoundary;

        allChildren.GetValueOrDefault("ActualZone").transform.localScale = new Vector3(1, 1, 1) * outerBoundary;
        allChildren.GetValueOrDefault("UpperHabitableZoneDistance").transform.localScale = new Vector3(1, 1, 1) * outerBoundary;

        (HarvardSpectralClassLetter, int) harvardSpectralAndNumber = starClassifier.ClassifyHarvardSpectral(starTemperature);
        int harvardSpectral = Array.IndexOf(Enum.GetValues(harvardSpectralAndNumber.ToTuple().Item1.GetType()), harvardSpectralAndNumber.ToTuple().Item1);
        // Detailed Text information:
        string text = "\n";
        text += "\nPosition [kiloparsecs]: \n" + star.transform.position.ToString();
        text += "\nMagnitude of Velocity [kiloparsecs per Million years]: \n" + star.GetComponent<Body>().velocity.magnitude.ToString();
        text += "\nVelocity [kiloparsecs per Million years]: \n" + star.GetComponent<Body>().velocity.ToString();
        text += "\nMagnitude of Acceleration [kiloparsecs per Million years squared]: \n" + star.GetComponent<Body>().acceleration.magnitude.ToString();
        text += "\nAcceleration [kiloparsecs per Million years squared]: \n" + star.GetComponent<Body>().acceleration.ToString();
        text += "\n\n";

        text += "\nStar Classification: ";
        text += "\n                    Harvard Spectral Class:  " + harvardSpectralAndNumber.ToString();
        text += "\n                                           <sprite=7>";
        text += $"\n                                           <sprite={harvardSpectral}>";
        text += "\n                                           <sprite=8>";
        text += $"\n                                           <sprite={harvardSpectral}>";
        text += "\n                                           <sprite=9>";
        text += $"\n                                           <sprite={harvardSpectral}>";
        text += "\n                                           <sprite=10>";
        text += $"\n                                           <sprite={harvardSpectral}>";
        text += "\n                                           <sprite=11>";
        text += $"\n                                           <sprite={harvardSpectral}>";
        text += "\n                                           <sprite=12>";
        text += "\n\n";

        text += "\nStar Color [RGB]: " + color.ToString();
        text += "\nStar Temperature [Kelvin]: " + starTemperature.ToString();
        text += "\nStar Relative Luminousity: " + relativeLuminousity.ToString();
        text += "\n\n";

        text += "\nMass [kg]: " + (star.GetComponent<Body>().mass * Constants.SUN_MASS);
        text += "\nRelative mass respective to Sun: " + star.transform.localScale[0].ToString();
        text += "\n\n";

        text += "\nLower threshold for habitable [relative to Sun]: " + innerBoundary.ToString();
        text += "\nLower threshold for habitable [km]: " + (innerBoundary * innerHabitableSunAbsolute).ToString();
        text += "\nUpper threshold for habitable [relative to Sun]: " + outerBoundary.ToString();
        text += "\nUpper threshold for habitable [km]: " + (outerBoundary * outerHabitableSunAbsolute).ToString();

        text += "\n\n\n\n";

        allChildren.GetValueOrDefault("DetailedInformationTextBox").GetComponent<TMP_Text>().text = text;

    }

    Dictionary<string, GameObject> GetAllChildren(GameObject obj)
    {
        Dictionary<string, GameObject> children = new();

        if (obj.tag == "NotFetch") return children;

        foreach (Transform child in obj.transform)
        {
            children.Add(child.gameObject.name, child.gameObject);
            children.AddRange(GetAllChildren(child.gameObject));
        }

        return children;
    }

    float CalculateInnerBoundary(double luminosity, double temperature)
    {
        float innerBoundary = (float)(Math.Sqrt(luminosity) * Math.Sqrt(Constants.SUN_TEMPERATURE / temperature));

        return innerBoundary;
    }
    float CalculateOuterBoundary(double luminosity, double temperature)
    {
        float outerBoundary = (float)(Math.Sqrt(luminosity) * Math.Sqrt(Constants.SUN_TEMPERATURE / temperature) * 1.4);

        return outerBoundary;
    }

    Color TemperatureToRGB(double temperature)
    {
        temperature -= 3000;
        double red, green, blue;

        if (temperature <= 3600)
        {
            red = 255;
            green = temperature / 100 - 2;
            blue = 0;
        }
        else
        {
            red = temperature / 100 - 55;
            green = 255 * Math.Pow((temperature / 1000 - 0.66), -0.133);
            blue = 255;
        }

        int r = (int)Math.Clamp(red, 0, 255);
        int g = (int)Math.Clamp(green, 0, 255);
        int b = (int)Math.Clamp(blue, 0, 255);

        return new Color(r / 255f, g / 255f, b / 255f);
    }
}
