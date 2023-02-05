// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using System.Globalization;

namespace OSG
{
    using UnityEngine;
    using UnityEditor;

    public class ScriptModificationProcessor : AssetModificationProcessor
    {
        public static void OnWillCreateAsset(string path)
        {
            path = path.Replace(".meta", "");
            int index = path.LastIndexOf(".");
            if (index < 0)
                return;
            string file = path.Substring(index);
            if (file != ".cs" && file != ".js" && file != ".boo") return;
            string dataPath = Application.dataPath;
            index = dataPath.LastIndexOf("Assets", StringComparison.Ordinal);
            path = dataPath.Substring(0, index) + path;
            file = System.IO.File.ReadAllText(path);

            // old way: file = file.Replace("#CREATIONDATE#", DateTime.Now.Date.ToLongDateString() + "");
            // Date and Time formatting: https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1
            file = file.Replace("#CREATIONDATE#", DateTime.Now.Date.ToString("dddd, MMMM dd, yyyy", new CultureInfo("en-US")));
            file = file.Replace("#PROJECTNAME#", PlayerSettings.productName);
            file = file.Replace("#COMPANY#", PlayerSettings.companyName);
            file = file.Replace("#COMPANYNAME#", PlayerSettings.companyName);
            file = file.Replace("#AUTHORTITLECASE#", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Environment.UserName.ToLower()));
            file = file.Replace("#AUTHOR#", Environment.UserName);
            if (file.Contains("#NAMESPACE#"))
            {
                if (path.Contains("OSG"))
                {
                    file = file.Replace("#NAMESPACE#", "namespace OSG {");
                    file += "\n}";
                }
                else
                {
                    file = file.Replace("#NAMESPACE#", "");
                }
            }

            //file += "// " + path;

            System.IO.File.WriteAllText(path, file);
            AssetDatabase.Refresh();
        }
        
    }
}