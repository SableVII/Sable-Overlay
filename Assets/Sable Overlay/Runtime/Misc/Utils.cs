using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static GameObject LoadPrefab(string resourcePrefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePrefabPath);
        if (prefab == null)
        {
            Logger.LogError("Failed to load prefab at resources path: " + resourcePrefabPath);
        }

        return prefab;
    }

    private static List<Char> _invalidFileNameChars = new List<Char>();

    public static string ConvertToValidPath(string inString)
    {
        if (_invalidFileNameChars.Count == 0)
        {
            _invalidFileNameChars.AddRange(System.IO.Path.GetInvalidFileNameChars());
            _invalidFileNameChars.Add('.');
        }

        Char[] inStringChars = inString.ToCharArray();

        string outString = "";

        bool foundInvalid = false;
        foreach (char c in inStringChars)
        {
            foundInvalid = _invalidFileNameChars.Contains(c);

            if (foundInvalid)
            {
                outString += '_';
                continue;
            }

            outString += c;
        }

        return outString;
    }
}
